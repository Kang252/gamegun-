using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 10f; // Đạn quái bay chậm hơn đạn người chơi để người chơi né
    public float damage = 10f;
    public GameObject impactEffect;

    void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = transform.right * speed;
        }
        Destroy(gameObject, 4f); // Tự hủy sau 4 giây
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Kiểm tra xem có trúng Player không
        PlayerHealth player = hitInfo.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(damage);
            
            if (impactEffect != null) Instantiate(impactEffect, transform.position, transform.rotation);
            Destroy(gameObject);
            return;
        }

        // Nếu chạm tường (không phải quái khác)
        if (hitInfo.gameObject.CompareTag("Untagged") || hitInfo.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (impactEffect != null) Instantiate(impactEffect, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}
