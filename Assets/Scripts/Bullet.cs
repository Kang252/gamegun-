using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 10;
    public GameObject impactEffect; // Hiệu ứng va chạm (Collision_Fx)

    void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = transform.right * speed;
        }
        Destroy(gameObject, 3f); // Tự hủy sau 3 giây nếu không trúng gì
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // 1. Tránh việc đạn vừa sinh ra đã va vào nhân vật Player và tự hủy!
        if (hitInfo.GetComponent<PlayerController>() != null) 
        {
            return; // Đạn chạm lưng Player thì bay xuyên qua đi tiếp
        }

        // Nếu chạm Enemy
        Enemy enemy = hitInfo.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, transform.position); // Gửi vị trí đạn để kiểm tra khiên
        }

        // Tạo hiệu ứng nổ và hủy viên đạn
        if (impactEffect != null) Instantiate(impactEffect, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}