using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    public Image healthBarFill; // Kéo cái Image màu xanh (Fill) vào đây
    public GameObject hitEffectPrefab;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();

        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (healthBarFill != null)
        {
            // Cập nhật thanh máu dựa trên tỉ lệ phần trăm
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }
    }

    void Die()
    {
        Debug.Log("Player Died!");
        
        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.SetTrigger("Death");

        // Báo cho GameManager để dừng game và hiện màn hình thua
        if (GameManager.Instance != null)
        {
            // Thay vì dừng liền, ta gọi GameOver sau 1 khoảng nhỏ để xem hoạt ảnh
            GameManager.Instance.Invoke("GameOver", 1.2f);
        }
    }
}
