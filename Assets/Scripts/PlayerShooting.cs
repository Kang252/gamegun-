using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject bulletPrefab; // Kéo Prefab viên đạn vào đây
    public Transform firePoint;     // Điểm mọc ra viên đạn (đầu nòng súng)
    public float fireRate = 0.5f;
    private float nextFireTime = 0f;

    public AudioClip shootSound;    // Kéo file Audio/Shoot1.wav vào đây
    private AudioSource audioSource;
    private Animator anim;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime) // Chuột trái
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        anim.SetTrigger("Shoot"); // Chạy animation bắn
        if (shootSound != null) audioSource.PlayOneShot(shootSound);
        
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }
}