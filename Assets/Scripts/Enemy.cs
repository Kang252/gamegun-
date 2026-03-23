using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health = 30;
    public GameObject deathEffect;
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        anim.SetTrigger("GetHit"); // Animation bị bắn trúng

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        anim.SetTrigger("Death"); // Animation chết
        // Tắt collider để không cản đường nữa
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
        Destroy(gameObject, 2f); // Xóa object sau khi anim chết chạy xong
    }
}