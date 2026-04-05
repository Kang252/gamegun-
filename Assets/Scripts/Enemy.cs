using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health = 30;
    public float moveSpeed = 3f;
    public int scoreValue = 1;

    public enum EnemyType { NormalMelee, Defender, HeavyMelee, Ranged }

    [Header("Loại Kẻ Địch")]
    public EnemyType type;
    public float stopDistance = 4f; 
    public GameObject enemyBulletPrefab;
    public GameObject muzzleFlashPrefab; 
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

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (!isDead && player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);

            if (type == EnemyType.Ranged && distance <= stopDistance)
            {
                if (Time.time >= nextFireTime)
                {
                    ShootPlayer();
                    nextFireTime = Time.time + fireRate;
                }
            }
            else
            {
                transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            }

            // Xử lý quay mặt (Sprite gốc quay trái)
            if (player.position.x > transform.position.x)
                transform.localScale = new Vector3(-1, 1, 1); // Quay phải
            else if (player.position.x < transform.position.x)
                transform.localScale = new Vector3(1, 1, 1);  // Quay trái
        }
    }

    public void TakeDamage(int damage, Vector2 hitSource)
    {
        if (isDead) return;

        // Đã sửa logic chặn đạn của quái Defender
        if (type == EnemyType.Defender)
        {
            float directionToHit = hitSource.x - transform.position.x;
            float facing = transform.localScale.x; 

            // Nếu quái quay TRÁI (facing > 0) và đạn từ TRÁI bay tới (directionToHit < 0)
            // Hoặc quái quay PHẢI (facing < 0) và đạn từ PHẢI bay tới (directionToHit > 0)
            if ((facing > 0 && directionToHit < 0) || (facing < 0 && directionToHit > 0))
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
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        if (healthPickupPrefab != null && Random.value <= dropChance)
        {
            Instantiate(healthPickupPrefab, transform.position, Quaternion.identity);
        }

        if (anim != null) anim.Play("Death");
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        
        if (deathEffect != null) Instantiate(deathEffect, transform.position, Quaternion.identity);
        
        Destroy(gameObject, 0.1f); 
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Cố gắng lấy PlayerHealth (nếu bạn có script này gắn trên Player)
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(10f); 
                if (anim != null) anim.Play("Hit"); 
                Die(); 
            }
        }
    }

    void ShootPlayer()
    {
        if (enemyBulletPrefab != null)
        {
            Vector3 firePos = firePoint != null ? firePoint.position : transform.position;
            Vector2 direction = (player.position - firePos).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            GameObject bullet = Instantiate(enemyBulletPrefab, firePos, Quaternion.Euler(0, 0, angle));
            
            if (muzzleFlashPrefab != null)
            {
                int facingDirection = transform.localScale.x > 0 ? 1 : -1;
                Quaternion baseRotation = facingDirection == 1 ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 0, 180f);
                GameObject flash = Instantiate(muzzleFlashPrefab, firePos, baseRotation, transform);
                Destroy(flash, 0.6f);
            }

            if (anim != null) anim.Play("Idle");
        }
    }
}