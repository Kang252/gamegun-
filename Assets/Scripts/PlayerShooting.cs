using UnityEngine;
using UnityEngine.UI; 

public enum FireMode { Single, Parallel, Spread, Beam }

[System.Serializable]
public struct WeaponData
{
    public string weaponName;       
    public GameObject bulletPrefab; 
    public float fireRate;          
    public AudioClip shootSound;    
    public Sprite weaponIcon;       
    public RuntimeAnimatorController weaponAnimator; 

    [Header("Cơ chế bắn")]
    public FireMode fireMode;
    public int bulletCount;
    public float spreadSpacing; 
    public GameObject impactEffect; // Hoạt ảnh chạm của đạn (Collision_Fx)
    public GameObject muzzleFlash;  // Hoạt ảnh chớp nòng của súng (Shoot1, Shoot2)
    [Header("Cơ chế Beam")]
    public Material beamMaterial;
}

public class PlayerShooting : MonoBehaviour
{
    public WeaponData[] weapons;    
    public int currentWeaponIndex = 0; 

    [Header("Cài đặt UI")]
    public Transform firePoint;     
    public Image weaponUI;          
    public Rect weaponCropRect = new Rect(250, 100, 240, 240); // Toa do va kich thuoc vung sung de cat
    
    private float nextFireTime = 0f;
    private AudioSource audioSource;
    private Animator anim;
    
    private GameObject activeBeam = null;
    private LineRenderer beamLine = null;
    
    [Header("Laser Animation")]
    public Sprite[] laserSprites; // Các sprite cho Laser (0-4)
    private float laserFrameTimer = 0f;
    private int currentLaserFrame = 0;
    public float laserFrameRate = 0.05f; 

    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        
        anim = GetComponent<Animator>();
        
        UpdateWeaponUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SwitchWeapon();
        }

        if (Input.GetKey(KeyCode.K))
        {
            if (activeBeam != null && beamLine != null)
            {
                // Animate texture offset and cycle sprites
                float moveSpeed = 4f;
                float offset = Time.time * moveSpeed;
                beamLine.material.mainTextureOffset = new Vector2(-offset, 0);

                if (laserSprites != null && laserSprites.Length > 0)
                {
                    laserFrameTimer += Time.deltaTime;
                    if (laserFrameTimer >= laserFrameRate)
                    {
                        laserFrameTimer = 0f;
                        currentLaserFrame = (currentLaserFrame + 1) % laserSprites.Length;
                        beamLine.material.mainTexture = laserSprites[currentLaserFrame].texture;
                    }
                }
            }

            if (weapons.Length > 0 && weapons[currentWeaponIndex].fireMode == FireMode.Beam)
            {
                FireBeam();
            }
            else if (Time.time >= nextFireTime)
            {
                Shoot();
            }
        }
        else
        {
            StopBeam();
        }
    }

    void FireBeam()
    {
        WeaponData currentWeapon = weapons[currentWeaponIndex];
        
        // Kích hoạt hoạt ảnh bắn skeleton cho súng điện
        if (anim != null && !anim.GetCurrentAnimatorStateInfo(0).IsName("Shoot")) 
            anim.Play("Shoot"); 
        
        if (activeBeam == null)
        {
            activeBeam = new GameObject("ContinuousBeam");
            activeBeam.transform.parent = firePoint;
            activeBeam.transform.localPosition = Vector3.zero;
            
            beamLine = activeBeam.AddComponent<LineRenderer>();
            beamLine.startWidth = 1.6f; // Mở rộng đáng kể bề ngang tia Beam
            beamLine.endWidth = 1.6f;
            if (currentWeapon.beamMaterial != null)
                beamLine.material = currentWeapon.beamMaterial;
            else
                beamLine.material = new Material(Shader.Find("Sprites/Default"));
            beamLine.startColor = Color.white;
            beamLine.endColor = Color.white;
            beamLine.positionCount = 2;
            beamLine.sortingOrder = 10;

            if (currentWeapon.shootSound != null && audioSource != null)
            {
                audioSource.clip = currentWeapon.shootSound;
                audioSource.loop = true;
                if (!audioSource.isPlaying) audioSource.Play();
            }
        }

        int facingDirection = transform.localScale.x > 0 ? 1 : -1;
        float beamLength = 8f; // Khoảng bằng 1/2 bản đồ hiển thị
        Vector3 beamEnd = firePoint.position + (Vector3.right * facingDirection * beamLength);

        beamLine.SetPosition(0, firePoint.position);
        beamLine.SetPosition(1, beamEnd);

        // CircleCast để beam có độ dày, dễ trúng kẻ địch hơn
        if (Time.time >= nextFireTime)
        {
            // Radius 0.8f = độ dày tia sét, trùng với visual beam width
            RaycastHit2D[] hits = Physics2D.CircleCastAll(firePoint.position, 0.8f, Vector2.right * facingDirection, beamLength);
            foreach (var hit in hits)
            {
                // Bỏ qua va chạm với chính Player
                if (hit.collider.CompareTag("Player")) continue;

                Enemy enemy = hit.collider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(2, hit.point); // Giảm sát thương để thấy rõ hiệu ứng Stun
                    enemy.TriggerElectricStun(0.15f); // Gây hiệu ứng điện giật (instant recovery)
                    if (currentWeapon.impactEffect != null)
                        Instantiate(currentWeapon.impactEffect, hit.point, Quaternion.identity);
                }
            }
            nextFireTime = Time.time + currentWeapon.fireRate;
        }
    }

    void StopBeam()
    {
        if (activeBeam != null)
        {
            Destroy(activeBeam);
            activeBeam = null;
            
            if (audioSource != null && weapons[currentWeaponIndex].fireMode == FireMode.Beam)
            {
                audioSource.Stop();
                audioSource.loop = false;
            }
        }
    }

    void SwitchWeapon()
    {
        if (weapons.Length == 0) return;

        currentWeaponIndex++; 
        if (currentWeaponIndex >= weapons.Length)
        {
            currentWeaponIndex = 0; 
        }

        StopBeam(); // Tắt tia nếu đang bắn mà đổi súng
        UpdateWeaponUI(); 
    }

    void UpdateWeaponUI()
    {
        if (weapons.Length > 0 && currentWeaponIndex < weapons.Length)
        {
            WeaponData current = weapons[currentWeaponIndex];
            if (weaponUI != null)
            {
                if (current.weaponIcon != null)
                {
                    // Tu dong tao Sprite moi chi lay phan sung (Crop)
                    Texture2D tex = current.weaponIcon.texture;
                    
                    // Kiem tra hop le cua Rect
                    float x = Mathf.Clamp(weaponCropRect.x, 0, tex.width);
                    float y = Mathf.Clamp(weaponCropRect.y, 0, tex.height);
                    float w = Mathf.Min(weaponCropRect.width, tex.width - x);
                    float h = Mathf.Min(weaponCropRect.height, tex.height - y);
                    
                    Rect crop = new Rect(x, y, w, h);
                    weaponUI.sprite  = Sprite.Create(tex, crop, new Vector2(0.5f, 0.5f));
                    weaponUI.color   = Color.white;
                    weaponUI.enabled = true;
                }
                else
                {
                    weaponUI.enabled = false;
                }
            }
            if (anim != null && current.weaponAnimator != null)
            {
                anim.runtimeAnimatorController = current.weaponAnimator;
            }
        }
    }

    void Shoot()
    {
        if (weapons.Length == 0) return;

        WeaponData currentWeapon = weapons[currentWeaponIndex]; 
        
        // Bật hoạt ảnh bắn súng
        if (anim != null) anim.SetTrigger("Shoot"); 
        
        if (currentWeapon.shootSound != null) 
        {
            audioSource.PlayOneShot(currentWeapon.shootSound);
        }

        int facingDirection = transform.localScale.x > 0 ? 1 : -1;
        Quaternion baseRotation = facingDirection == 1 ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 0, 180f);

        // Sinh hoạt ảnh chớp nòng (Shoot1/Shoot2) và khói lửa
        if (currentWeapon.muzzleFlash != null)
        {
            GameObject flash = Instantiate(currentWeapon.muzzleFlash, firePoint.position, baseRotation, firePoint);
            Destroy(flash, 0.6f); // Hủy sau 0.6s (đủ chạy xong 6-8 frame animation của khói súng)
        }

        if (currentWeapon.bulletPrefab != null)
        {
            if (currentWeapon.fireMode == FireMode.Single)
            {
                GameObject bullet = Instantiate(currentWeapon.bulletPrefab, firePoint.position, baseRotation);
                Bullet bScript = bullet.GetComponent<Bullet>();
                if (bScript != null) bScript.impactEffect = currentWeapon.impactEffect;
            }
            else if (currentWeapon.fireMode == FireMode.Parallel)
            {
                int rounds = Mathf.Max(1, currentWeapon.bulletCount);
                for (int i = 0; i < rounds; i++)
                {
                    // Lệch đạn lên/xuống theo trục Y
                    float yOffset = (i - (rounds - 1) / 2f) * currentWeapon.spreadSpacing;
                    Vector3 spawnPos = firePoint.position + new Vector3(0, yOffset, 0);
                    GameObject bullet = Instantiate(currentWeapon.bulletPrefab, spawnPos, baseRotation);

                    Bullet bScript = bullet.GetComponent<Bullet>();
                    if (bScript != null) bScript.impactEffect = currentWeapon.impactEffect;
                }
            }
            else if (currentWeapon.fireMode == FireMode.Spread)
            {
                int rounds = Mathf.Max(1, currentWeapon.bulletCount);
                for (int i = 0; i < rounds; i++)
                {
                    // Tỏa góc chéo theo trục Z
                    float angleOffset = (i - (rounds - 1) / 2f) * currentWeapon.spreadSpacing;
                    Quaternion finalRot = baseRotation * Quaternion.Euler(0, 0, angleOffset);
                    GameObject bullet = Instantiate(currentWeapon.bulletPrefab, firePoint.position, finalRot);
                    
                    // Gán hiệu ứng nổ cho viên đạn
                    Bullet bScript = bullet.GetComponent<Bullet>();
                    if (bScript != null) bScript.impactEffect = currentWeapon.impactEffect;
                }
            }
        }

        nextFireTime = Time.time + currentWeapon.fireRate;
    }
}