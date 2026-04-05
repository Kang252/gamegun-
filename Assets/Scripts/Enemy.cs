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
    public GameObject healthPickupPrefab;
    [Range(0f, 1f)]
    public float dropChance = 0.3f;

    public GameObject deathEffect;

    // --- Internal state ---
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Transform player;
    private bool isDead = false;
    private bool isStunned = false;
    private float stunEndTime = 0f;
    private string currentAnim = "";

    // --- Get Electric visual flash ---
    private float electricFlashTimer = 0f;
    private Color originalColor;
    private static readonly Color ElectricColor = new Color(0.3f, 0.8f, 1f); // cyan-blue

    void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;

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
                // Trả lại màu gốc khi hết stun
                if (spriteRenderer != null) spriteRenderer.color = originalColor;
            }
            else
            {
                // Nhấp nháy màu electric
                electricFlashTimer += Time.deltaTime;
                if (spriteRenderer != null)
                    spriteRenderer.color = (Mathf.Sin(electricFlashTimer * 20f) > 0) ? ElectricColor : originalColor;

                // Phát animation "Get Hit" (nếu có "Get Electric" thay bằng tên đúng trong Animator)
                PlayAnim("Get Hit");
                return; // Ngừng mọi hoạt động khi bị choáng
            }
        }

        float distance = Vector2.Distance(transform.position, player.position);

        // --- Xử lý hướng nhìn ---
        if (player.position.x > transform.position.x)
            transform.localScale = new Vector3(-1, 1, 1); // Quay phải
        else if (player.position.x < transform.position.x)
            transform.localScale = new Vector3(1, 1, 1);  // Quay trái

        // === KẺ ĐỊCH BẮN XA (Ranged) ===
        if (type == EnemyType.Ranged)
        {
            if (distance <= stopDistance)
            {
                PlayAnim("Idle");
                if (Time.time >= nextFireTime)
                {
                    ShootStraight(); // Bắn thẳng theo hướng nhìn
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

    void PlayAnim(string animName)
    {
        if (anim != null && currentAnim != animName)
        {
            // Kiểm tra xem state có tồn tại không trước khi Play
            if (HasState(animName))
            {
                currentAnim = animName;
                anim.Play(animName);
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
        if (anim != null) anim.Play("Hit");
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
        Debug.Log($"[{gameObject.name}] bị điện giật trong {duration}s!");
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(scoreValue);

        if (healthPickupPrefab != null && Random.value <= dropChance)
            Instantiate(healthPickupPrefab, transform.position, Quaternion.identity);

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
        if (collision.gameObject.CompareTag("Player"))
        {
            // Gây 20% sát thương va chạm (nhỏ, chủ yếu dùng AttackPlayer)
            PlayerHealth ph = collision.gameObject.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(attackDamage * 0.2f);
        }
    }

    // ===== ATTACK =====

    void AttackPlayer()
    {
        // Animation đánh (dùng "Hit" vì controller chưa có state "Attack" riêng)
        currentAnim = "";
        if (anim != null) anim.Play("Hit");
        Invoke(nameof(ResetAnimState), 0.35f);

        // Tìm PlayerHealth trực tiếp trong scene nếu player ref không có component
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth == null)
            playerHealth = player.GetComponentInChildren<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
            Debug.Log($"[{gameObject.name}] đánh Player: -{attackDamage} HP");
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] Không tìm thấy PlayerHealth trên Player!");
        }
    }

    // ===== SHOOT (Bay thẳng theo hướng nhìn) =====

    void ShootStraight()
    {
        if (enemyBulletPrefab == null) return;

        // Lấy vị trí bắn
        Vector3 firePos = firePoint != null ? firePoint.position : transform.position;

        // Hướng bắn: chỉ bay ngang theo hướng kẻ địch đang nhìn
        // localScale.x < 0 => đang quay phải => bắn sang phải
        // localScale.x > 0 => đang quay trái => bắn sang trái
        int facingDir = transform.localScale.x < 0 ? 1 : -1;
        float angle = facingDir > 0 ? 0f : 180f; // 0° = phải, 180° = trái

        GameObject bulletObj = Instantiate(enemyBulletPrefab, firePos, Quaternion.Euler(0, 0, angle));

        // Truyền damage cho EnemyBullet
        EnemyBullet eb = bulletObj.GetComponent<EnemyBullet>();
        if (eb != null)
        {
            eb.Init(attackDamage, gameObject);
        }
        else
        {
            Bullet old = bulletObj.GetComponent<Bullet>();
            if (old != null) old.damage = (int)attackDamage;
        }

        // Muzzle flash
        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, firePos, Quaternion.Euler(0, 0, angle), transform);
            Destroy(flash, 0.15f);
        }
    }
}