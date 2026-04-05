using UnityEngine;

/// <summary>
/// Script dành riêng cho đạn của kẻ địch. 
/// Chỉ gây sát thương cho Player, bay xuyên qua Enemy khác.
/// </summary>
public class EnemyBullet : MonoBehaviour
{
    public float speed = 12f;
    public float damage = 10f;
    public GameObject impactEffect;

    private string shooterInstanceID; // ID của kẻ địch bắn ra viên đạn này

    public void Init(float dmg, GameObject shooter)
    {
        damage = dmg;
        shooterInstanceID = shooter.GetInstanceID().ToString();
    }

    void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = transform.right * speed;
        }
        Destroy(gameObject, 5f); // Tự hủy sau 5 giây
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Bỏ qua va chạm với các Enemy khác (kể cả kẻ địch bắn ra viên đạn)
        if (hitInfo.GetComponent<Enemy>() != null) return;

        // Chỉ xử lý khi va chạm với Player
        PlayerHealth playerHealth = hitInfo.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
        else
        {
            // Va vào tường hoặc vật thể khác - không cần làm gì đặc biệt
            // Chỉ bỏ qua nếu không phải Player
            return;
        }

        // Tạo hiệu ứng nổ và hủy viên đạn
        if (impactEffect != null) Instantiate(impactEffect, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Bỏ qua va chạm vật lý với Enemy
        if (collision.gameObject.GetComponent<Enemy>() != null) return;

        if (impactEffect != null) Instantiate(impactEffect, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
