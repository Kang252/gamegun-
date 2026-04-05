using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Số máu hồi phục")]
    public float healAmount = 20f;
    
    // Hàm được gọi tự động khi một đối tượng (có Collider là Trigger) va chạm với Item này
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Cải tiến: Không cần bắt Tag "Player" vì nhỡ quên Tag thì không ăn được.
        // Chỉ cần tìm xem đối tượng chạm vào có mang script PlayerHealth không (kể cả ở Child hay Parent).
        PlayerHealth playerHealth = collision.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null) playerHealth = collision.GetComponentInChildren<PlayerHealth>();

        if (playerHealth != null)
        {
            // Bơm máu và hủy bình máu đi
            playerHealth.Heal(healAmount);
            Destroy(gameObject);
        }
    }
}
