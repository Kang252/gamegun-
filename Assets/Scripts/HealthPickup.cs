using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Số máu hồi phục")]
    public float healAmount = 20f;
    
    // Hàm được gọi tự động khi một đối tượng (có Collider là Trigger) va chạm với Item này
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xen có phải là Player chạm vào không
        if (collision.CompareTag("Player"))
        {
            // Lấy script chứa cấu trúc Máu của Player
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Bơm máu và hủy bình máu đi
                playerHealth.Heal(healAmount);
                Destroy(gameObject);
            }
        }
    }
}
