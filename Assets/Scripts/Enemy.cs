using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health = 30;
    public float moveSpeed = 3f;
    public int scoreValue = 1; // Điểm KILLS nhận được

    public enum EnemyType { NormalMelee, Defender, HeavyMelee, Ranged }

    [Header("Loại Kẻ Địch")]
    public EnemyType type;
    public float stopDistance = 4f; 
    public GameObject enemyBulletPrefab;
    public GameObject muzzleFlashPrefab; // Thêm tia lửa khi kẻ địch nhả đạn
    public Transform firePoint;
    public float fireRate = 2f;
    private float nextFireTime;

    [Header("Rớt Máu")]
    public GameObject healthPickupPrefab;
    [Range(0f, 1f)]
    public float dropChance = 0.5f;

    public GameObject deathEffect;
    
    private Animator anim;
    private Transform player;
    private bool isDead = false;

    void Start()
    {
        anim = GetComponent<Animator>();

        // Tự động tìm nhân vật chính mang Tag "Player" để đuổi theo
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        // Rượt đuổi liên tục
        if (!isDead && player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);

            if (type == EnemyType.Ranged && distance <= stopDistance)
            {
                // Đứng lại sấy súng
                if (Time.time >= nextFireTime)
                {
                    ShootPlayer();
                    nextFireTime = Time.time + fireRate;
                }
            }
            else
            {
                // Đi tiếp
                transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            }

            // Đảo ngược lại logic Flip: vì Sprite Gốc quay mặt bên Trái
            if (player.position.x > transform.position.x)
                transform.localScale = new Vector3(-1, 1, 1); // Trái ngược lại: -1 là quay sang Phải
            else if (player.position.x < transform.position.x)
                transform.localScale = new Vector3(1, 1, 1);  // 1 là quay sang Trái
        }
    }

    public void TakeDamage(int damage, Vector2 hitSource)
    {
        if (isDead) return;

        // --- 100% accurately: Defender Shield Logic ---
        // Nếu là Quái Khiên Vàng (Defender), chỉ trúng từ phía sau
        if (type == EnemyType.Defender)
        {
            float directionToHit = hitSource.x - transform.position.x;
            float facing = transform.localScale.x; // 1 là phải, -1 là trái

            // Nếu hướng đạn bay đến trùng với hướng mặt đang nhìn => Đang bắn vào khiên
            if ((facing > 0 && directionToHit > 0) || (facing < 0 && directionToHit < 0))
            {
                Debug.Log("KENG! Đạn trúng khiên, không gây sát thương!");
                return; 
            }
        }

        health -= damage;
        
        if (anim != null) anim.Play("Get Hit");

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        
        // Báo cho máy chủ tính điểm (GameManager) cộng KILLS
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        // Rớt item máu theo tỉ lệ
        if (healthPickupPrefab != null && Random.value <= dropChance)
        {
            Instantiate(healthPickupPrefab, transform.position, Quaternion.identity);
        }

        if (anim != null) anim.Play("Death");
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        
        if (deathEffect != null) Instantiate(deathEffect, transform.position, Quaternion.identity);
        
        // Hủy quái nhanh chóng
        Destroy(gameObject, 0.1f); 
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(10f); // Gây 10 sát thương
                if (anim != null) anim.Play("Hit"); // Act attack animation
                Die(); // Quái tự hủy sau khi cắn
            }
        }
    }

    void ShootPlayer()
    {
        if (enemyBulletPrefab != null)
        {
            Vector3 firePos = firePoint != null ? firePoint.position : transform.position;
            // Tính hướng bắn về phía Player
            Vector2 direction = (player.position - firePos).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            GameObject bullet = Instantiate(enemyBulletPrefab, firePos, Quaternion.Euler(0, 0, angle));
            
            // Xả tia lửa chớp nòng cho kẻ địch đúng tại đầu súng
            if (muzzleFlashPrefab != null)
            {
                int facingDirection = transform.localScale.x > 0 ? 1 : -1;
                Quaternion baseRotation = facingDirection == 1 ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 0, 180f);
                GameObject flash = Instantiate(muzzleFlashPrefab, firePos, baseRotation, transform);
                Destroy(flash, 0.6f);
            }

            if (anim != null) anim.Play("Idle"); // Shooter enemies often just stand still to shoot
        }
    }
}