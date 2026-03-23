using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 10;
    public GameObject impactEffect; // Hiệu ứng va chạm (Collision_Fx)

    void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.right * speed;
        Destroy(gameObject, 3f); // Tự hủy sau 3 giây nếu không trúng gì
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Nếu chạm Enemy
        Enemy enemy = hitInfo.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        // Tạo hiệu ứng nổ và hủy viên đạn
        if (impactEffect != null) Instantiate(impactEffect, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}