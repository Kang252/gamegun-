using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats Cơ Bản")]
    public float health = 30f;
    public float moveSpeed = 3f;
    public float attackDamage = 10f;
    public int scoreValue = 10;

    public enum EnemyType { NormalMelee, Defender, HeavyMelee, Ranged }

    [Header("Loại Kẻ Địch")]
    public EnemyType type;
    
    [Tooltip("Khoảng cách để kẻ địch bắn xa dừng lại. KHÔNG dùng cho cận chiến.")]
    public float stopDistance = 8f;
    
    [Tooltip("Khoảng cách để kẻ địch cận chiến bắt đầu đánh.")]
    public float attackRange = 1.2f;
    
    public GameObject enemyBulletPrefab;
    public GameObject muzzleFlashPrefab;
    public Transform firePoint;
    
    [Tooltip("Thời gian giữa 2 lần bắn/đánh (giây).")]
    public float fireRate = 2f;
    
    private float nextFireTime;
    private float nextAttackTime;

    [Header("Rớt Máu")]
    public GameObject healthPickupPrefab;
    [Range(0f, 1f)]
    public float dropChance = 0.3f;

    public GameObject deathEffect;
    
    private Animator anim;
    private Transform player;
    private bool isDead = false;
    private bool isStunned = false;
    private float stunEndTime = 0f;
    private string currentAnim = "";

    void Start()
    {
        anim = GetComponent<Animator>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (isDead || player == null) return;

        // --- Xử lý trạng thái bị điện giật (Stun) ---
        if (isStunned)
        {
            if (Time.time >= stunEndTime)
            {
                isStunned = false;
            }
            else
            {
                PlayAnim("Get Electric");
                return; // Ngừng mọi hoạt động khi bị choáng
            }
        }

        float distance = Vector2.Distance(transform.position, player.position);

        // --- Xử lý hướng nhìn ---
        if (player.position.x > transform.position.x)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (player.position.x < transform.position.x)
            transform.localScale = new Vector3(1, 1, 1);

        // === LOGIC CHO KẺ ĐỊCH BẮN XA (Ranged) ===
        if (type == EnemyType.Ranged)
        {
            if (distance <= stopDistance)
            {
                // Đứng yên và bắn
                PlayAnim("Idle");
                if (Time.time >= nextFireTime)
                {
                    ShootPlayer();
                    nextFireTime = Time.time + fireRate;
                }
            }
            else
            {
                // Di chuyển lại gần để vào tầm bắn
                transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
                PlayAnim("Walk");
            }
        }
        // === LOGIC CHO KẺ ĐỊCH CẬN CHIẾN (Melee) ===
        else
        {
            if (distance <= attackRange)
            {
                // Trong tầm đánh -> Đứng yên và tấn công
                if (Time.time >= nextAttackTime)
                {
                    AttackPlayer();
                    nextAttackTime = Time.time + fireRate;
                }
            }
            else
            {
                // Ngoài tầm đánh -> Di chuyển về phía Player
                transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
                PlayAnim("Walk");
            }
        }
    }

    // Phát animation không bị gián đoạn nếu đang phát cùng 1 animation
    void PlayAnim(string animName)
    {
        if (anim != null && currentAnim != animName)
        {
            currentAnim = animName;
            anim.Play(animName);
        }
    }

    public void TakeDamage(int damage, Vector2 hitSource)
    {
        if (isDead) return;

        // --- Logic chặn đạn của Defender ---
        if (type == EnemyType.Defender)
        {
            float directionToHit = hitSource.x - transform.position.x;
            float facing = transform.localScale.x;
            if ((facing > 0 && directionToHit < 0) || (facing < 0 && directionToHit > 0))
            {
                Debug.Log("KENG! Đạn trúng khiên!");
                return;
            }
        }

        health -= damage;
        currentAnim = ""; // Reset để có thể phát Hit ngay
        if (anim != null) anim.Play("Hit");
        
        // Sau khi Hit xong, reset currentAnim để vòng update tiếp theo có thể chuyển lại Walk/Idle
        Invoke(nameof(ResetAnimState), 0.3f);

        if (health <= 0) Die();
    }

    // Overload để tương thích với code cũ (float damage)
    public void TakeDamage(float damage, Vector2 hitSource)
    {
        TakeDamage((int)damage, hitSource);
    }

    void ResetAnimState()
    {
        currentAnim = "";
    }

    public void TriggerElectricStun(float duration)
    {
        if (isDead) return;
        isStunned = true;
        stunEndTime = Time.time + duration;
        currentAnim = ""; // Cho phép phát Get Electric ngay
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(scoreValue);

        if (healthPickupPrefab != null && Random.value <= dropChance)
            Instantiate(healthPickupPrefab, transform.position, Quaternion.identity);

        currentAnim = "Death";
        if (anim != null) anim.Play("Death");

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (deathEffect != null) Instantiate(deathEffect, transform.position, Quaternion.identity);

        Destroy(gameObject, 0.8f);
    }

    // Va chạm vật lý với Player (chỉ gây sát thương nhỏ - dự phòng)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead || isStunned) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(attackDamage * 0.5f);
        }
    }

    void AttackPlayer()
    {
        // Phát animation đánh
        currentAnim = ""; // Reset để Hit có thể phát
        if (anim != null) anim.Play("Hit");
        Invoke(nameof(ResetAnimState), 0.4f);
        
        // Gây sát thương
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.TakeDamage(attackDamage);
        
        Debug.Log($"[{gameObject.name}] tấn công Player với {attackDamage} sát thương!");
    }

    void ShootPlayer()
    {
        if (enemyBulletPrefab == null) return;

        // Lấy vị trí bắn - ưu tiên firePoint, fallback dùng vị trí enemy + offset nhỏ về phía Player
        Vector3 firePos;
        if (firePoint != null)
        {
            firePos = firePoint.position;
        }
        else
        {
            // Offset đạn ra khỏi collider của enemy (1 unit về phía player)
            Vector2 dir = (player.position - transform.position).normalized;
            firePos = transform.position + (Vector3)(dir * 0.8f);
        }

        Vector2 direction = (player.position - firePos).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        GameObject bulletObj = Instantiate(enemyBulletPrefab, firePos, Quaternion.Euler(0, 0, angle));

        // Gán script EnemyBullet nếu có, truyền sát thương
        EnemyBullet eb = bulletObj.GetComponent<EnemyBullet>();
        if (eb != null)
        {
            eb.Init(attackDamage, gameObject);
        }
        else
        {
            // Fallback: Nếu dùng script Bullet cũ, đặt damage
            Bullet oldBullet = bulletObj.GetComponent<Bullet>();
            if (oldBullet != null) oldBullet.damage = (int)attackDamage;
        }

        // Muzzle flash
        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, firePos, Quaternion.Euler(0, 0, angle), transform);
            Destroy(flash, 0.15f);
        }
        
        Debug.Log($"[{gameObject.name}] bắn đạn từ {firePos}");
    }
}