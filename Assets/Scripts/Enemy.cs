using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats Cơ Bản")]
    public float health = 30f;
    public float moveSpeed = 3f;
    public float attackDamage = 10f;
    public int scoreValue = 1;

    public enum EnemyType { NormalMelee, Defender, HeavyMelee, Ranged }

    [Header("Loại Kẻ Địch")]
    public EnemyType type;

    [Tooltip("Kẻ địch bắn xa: dừng lại ở khoảng cách này để bắn.")]
    public float stopDistance = 8f;

    [Tooltip("Kẻ địch cận chiến: bắt đầu đánh khi trong khoảng cách này.")]
    public float attackRange = 2.5f;

    public GameObject enemyBulletPrefab;
    public GameObject muzzleFlashPrefab;
    public Transform firePoint;

    [Tooltip("Thời gian giữa 2 lần bắn/đánh (giây).")]
    public float fireRate = 2f;

    private float nextFireTime;
    private float nextAttackTime;

    [Header("Rớt Máu")]
    public GameObject[] healthPickupPrefabs;
    [Range(0f, 1f)]
    public float dropChance = 0.2f; // Tỉ lệ 1/5 rớt bình máu

    public GameObject deathEffect;
    public GameObject hitEffectPrefab;

    // --- Internal state ---
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Transform player;
    private bool isDead = false;
    private bool isStunned = false;
    private float stunEndTime = 0f;
    private string currentAnim = "";
    private float lockAnimationUntil = 0f;

    // --- Get Electric visual flash ---
    private float electricFlashTimer = 0f;
    private Vector3 originalScale;
    private Color originalColor;
    private static readonly Color ElectricColor = new Color(0.3f, 0.8f, 1f); // cyan-blue

    void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;

        originalScale = transform.localScale;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (isDead || player == null) return;

        // --- Xử lý Get Electric visual flash ---
        if (isStunned)
        {
            if (Time.time >= stunEndTime)
            {
                isStunned = false;
                if (spriteRenderer != null) 
                {
                    spriteRenderer.color = originalColor;
                    spriteRenderer.enabled = true; 
                }
            }
            else
            {
                // Ép dừng chuyển động khi đang choáng
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;

                // Để Animator xử lý việc hiển thị Skeleton, chỉ cần giữ spriteRenderer bật
                if (spriteRenderer != null) spriteRenderer.enabled = true;

                PlayAnim("Get Electric", 0.5f);
                return; 
            }
        }

        // Nếu đang trong thời gian khóa animation (đang đánh), dừng di chuyển & các anim khác
        if (Time.time < lockAnimationUntil) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // --- Xử lý hướng nhìn (Dựa trên baseScale) ---
        if (player.position.x > transform.position.x)
            transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z); // Quay phải
        else if (player.position.x < transform.position.x)
            transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);  // Quay trái

        // === KẺ ĐỊCH BẮN XA (Ranged) ===
        if (type == EnemyType.Ranged)
        {
            if (distance <= stopDistance)
            {
                PlayAnim("Idle");
                if (Time.time >= nextFireTime)
                {
                    ShootStraight();
                    nextFireTime = Time.time + fireRate;
                }
            }
            else
            {
                transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
                PlayAnim("Walk");
            }
        }
        // === KẺ ĐỊCH CẬN CHIẾN (Melee / Defender / HeavyMelee) ===
        else
        {
            if (distance <= attackRange)
            {
                // Đứng yên, chờ đánh
                PlayAnim("Idle");
                if (Time.time >= nextAttackTime)
                {
                    AttackPlayer();
                    nextAttackTime = Time.time + fireRate;
                }
            }
            else
            {
                // Di chuyển về phía player
                transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
                PlayAnim("Walk");
            }
        }
    }

    void PlayAnim(string animName, float lockDuration = 0f)
    {
        if (anim != null && currentAnim != animName)
        {
            currentAnim = animName;
            anim.Play(animName);
            
            if (lockDuration > 0)
            {
                lockAnimationUntil = Time.time + lockDuration;
            }
        }
    }

    bool HasState(string stateName)
    {
        if (anim == null) return false;
        // Thử kiểm tra state có hợp lệ không
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            // nếu Animator có parameter thì coi như hợp lệ
        }
        // Đơn giản: cứ Play thôi, Unity sẽ log warning nếu không có
        return true;
    }

    // ===== DAMAGE & DEATH =====

    public void TakeDamage(int damage, Vector2 hitSource)
    {
        if (isDead) return;

        // Logic chặn đạn của Defender
        if (type == EnemyType.Defender)
        {
            float directionToHit = hitSource.x - transform.position.x;
            float facing = transform.localScale.x;
            // Khiên ở phía trước: facing > 0 = quay trái, facing < 0 = quay phải
            if ((facing > 0 && directionToHit > 0) || (facing < 0 && directionToHit < 0))
            {
                Debug.Log("KENG! Đạn trúng khiên của Defender!");
                return;
            }
        }

        health -= damage;
        currentAnim = "";
        
        // Luôn thử chạy "Get Hit" trước, nếu không có mới chạy "Hit"
        if (anim != null) 
        {
            anim.Play("Get Hit");
        }
        
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }

        Invoke(nameof(ResetAnimState), 0.35f);

        if (health <= 0) Die();
    }

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
        electricFlashTimer = 0f;
        currentAnim = "";
        
        // Dừng lực đẩy vật lý ngay khi bắt đầu choáng
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
        
        Debug.Log($"[{gameObject.name}] bị điện giật trong {duration}s!");
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(scoreValue);

        if (healthPickupPrefabs != null && healthPickupPrefabs.Length > 0 && Random.value <= dropChance)
        {
            int rndIndex = Random.Range(0, healthPickupPrefabs.Length);
            if (healthPickupPrefabs[rndIndex] != null)
            {
                Instantiate(healthPickupPrefabs[rndIndex], transform.position, Quaternion.identity);
            }
        }

        // Trả lại màu gốc khi chết
        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        currentAnim = "Death";
        if (anim != null) anim.Play("Death");

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        if (deathEffect != null) Instantiate(deathEffect, transform.position, Quaternion.identity);

        Destroy(gameObject, 0.8f);
    }

    // Va chạm vật lý với Player khi tiến vào
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead || isStunned) return;
        // Đã xóa sát thương va chạm vật lý để tránh trừ máu sớm
    }

    // ===== ATTACK =====

    void AttackPlayer()
    {
        // Khóa animation để chạy đòn đánh - Tăng lên 0.8s để chạy hết hoạt ảnh
        PlayAnim("Hit", 0.8f);
        
        // Trì hoãn việc trừ máu 0.4s để khớp với lúc vung tay trong animation
        Invoke(nameof(DealMeleeDamage), 0.4f);
    }

    void DealMeleeDamage()
    {
        if (isDead || isStunned || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        // Tầm đánh thực tế hơi rộng hơn attackRange một chút để bù cho độ trễ
        if (distance <= attackRange + 1.0f) 
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth == null) playerHealth = player.GetComponentInChildren<PlayerHealth>();
            
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log($"[{gameObject.name}] thực sự đánh Player: -{attackDamage} HP");
            }
        }
    }

    // ===== SHOOT (Bay thẳng theo hướng nhìn) =====

    void ShootStraight()
    {
        if (enemyBulletPrefab == null) return;

        // Lấy vị trí bắn
        Vector3 firePos = firePoint != null ? firePoint.position : transform.position;

        // Hướng bắn: 
        // localScale.x < 0 => đang quay phải => góc 0
        // localScale.x > 0 => đang quay trái => góc 180
        float angle = transform.localScale.x < 0 ? 0f : 180f; 
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        GameObject bulletObj = Instantiate(enemyBulletPrefab, firePos, rotation);

        // Truyền damage cho EnemyBullet
        EnemyBullet eb = bulletObj.GetComponent<EnemyBullet>();
        if (eb != null)
        {
            eb.Init(attackDamage, gameObject);
        }

        // Muzzle flash
        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, firePos, rotation, transform);
            Destroy(flash, 0.15f);
        }
    }
}