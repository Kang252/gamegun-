using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    
    private Rigidbody2D rb;
    private Animator anim;
    private bool facingRight = true;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); 
        
        // Tắt trọng lực để player không bị kéo xuống màn hình
        if (rb != null)
        {
            rb.gravityScale = 0f;
        }
    }

    void Update()
    {
        // Lấy Input từ các phím WASD hoặc Mũi tên
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Xử lý góc quay mặt của player
        if (movement.x > 0 && !facingRight) Flip();
        else if (movement.x < 0 && facingRight) Flip();

        // Cập nhật giá trị "Speed" cho Animation (lớn hơn 0 khi di chuyển)
        if (anim != null)
        {
            anim.SetFloat("Speed", movement.sqrMagnitude);
        }
    }

    void FixedUpdate() 
    {
        // Di chuyển bằng linearVelocity (nhân thêm normalized để đi chéo không bị nhanh gấp đôi)
        rb.linearVelocity = movement.normalized * moveSpeed;
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }
}