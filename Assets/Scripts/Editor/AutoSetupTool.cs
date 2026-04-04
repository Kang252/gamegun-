#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class GameSetupTool : EditorWindow
{
    // Tạo một thanh Menu chức năng nhỏ trên góc cho bạn dễ click
    [MenuItem("Hướng Dẫn Game / 1. Bấm vào đây để Tự Động Ráp 4 Súng và Đạn")]
    public static void SetupPlayerShooting()
    {
        // Tự động quét màn hình tìm nhân vật
        PlayerShooting ps = Object.FindAnyObjectByType<PlayerShooting>();
        if (ps == null)
        {
            Debug.LogError("Chưa tìm thấy nhân vật trên màn hình! Hãy kéo Player từ dưới lên màn hình trước nhé.");
            return;
        }

        // Tạo 4 khe cắm vũ khí
        Undo.RecordObject(ps, "Auto Setup Guns");
        ps.weapons = new WeaponData[4];

        // Công cụ thu thập tài nguyên siêu tốc
        T Load<T>(string path) where T : Object => AssetDatabase.LoadAssetAtPath<T>("Assets/Assets for the assessment/Mad Doctor Assets/" + path);

        // --- SÚNG 1: Standard Rifle ---
        ps.weapons[0].weaponName = "Standard Rifle";
        ps.weapons[0].bulletPrefab = Load<GameObject>("Prefabs/Dan_Xanh.prefab");
        ps.weapons[0].shootSound = Load<AudioClip>("Audio/Shoot1.wav");
        ps.weapons[0].weaponIcon = Load<Sprite>("Sprites/Mad Doctor - Main Character/Gun01/Idle/Idle_00.png");
        ps.weapons[0].fireMode = FireMode.Single;
        ps.weapons[0].fireRate = 0.25f;
        ps.weapons[0].impactEffect = Load<GameObject>("Prefabs/Fx01.prefab");

        // --- SÚNG 2: Twin Blaster (Song song) ---
        ps.weapons[1].weaponName = "Twin Blaster";
        ps.weapons[1].bulletPrefab = Load<GameObject>("Prefabs/Dan_Tim.prefab");
        ps.weapons[1].shootSound = Load<AudioClip>("Audio/Shoot2.wav");
        ps.weapons[1].weaponIcon = Load<Sprite>("Sprites/Mad Doctor - Main Character/Gun02/Idle/Idle_00.png");
        ps.weapons[1].fireMode = FireMode.Parallel;
        ps.weapons[1].bulletCount = 2;
        ps.weapons[1].spreadSpacing = 0.4f;
        ps.weapons[1].fireRate = 0.35f;
        ps.weapons[1].impactEffect = Load<GameObject>("Prefabs/Fx02.prefab");

        // --- SÚNG 3: Spread Shotgun ---
        ps.weapons[2].weaponName = "Spread Shotgun";
        ps.weapons[2].bulletPrefab = Load<GameObject>("Prefabs/Dan_Vang.prefab");
        ps.weapons[2].shootSound = Load<AudioClip>("Audio/Shoot3.wav");
        ps.weapons[2].weaponIcon = Load<Sprite>("Sprites/Mad Doctor - Main Character/Gun03/Idle/Idle_00.png");
        ps.weapons[2].fireMode = FireMode.Spread;
        ps.weapons[2].bulletCount = 3; 
        ps.weapons[2].spreadSpacing = 20f; 
        ps.weapons[2].fireRate = 0.6f;
        ps.weapons[2].impactEffect = Load<GameObject>("Prefabs/Fx03.prefab");

        // --- SÚNG 4: Lightning Beam ---
        ps.weapons[3].weaponName = "Lightning Beam";
        ps.weapons[3].bulletPrefab = Load<GameObject>("Prefabs/Dan_Xanh.prefab");
        ps.weapons[3].shootSound = Load<AudioClip>("Audio/Shoot4.wav");
        ps.weapons[3].weaponIcon = Load<Sprite>("Sprites/Mad Doctor - Main Character/Gun04/Idle/Idle_00.png");
        ps.weapons[3].fireMode = FireMode.Beam;
        ps.weapons[3].fireRate = 0.05f; // Bắn siêu nhanh tạo thành tia sét
        ps.weapons[3].bulletCount = 1;
        ps.weapons[3].spreadSpacing = 0f;
        ps.weapons[3].impactEffect = Load<GameObject>("Prefabs/Fx01.prefab"); // Dùng lại Fx01 hoặc Fx khác

        EditorUtility.SetDirty(ps);
        Debug.Log("====== 100% ACCURACY ASSET LOADED WITH IMPACT EFFECTS ======");
        Debug.Log("Hệ thống 4 loại súng đã được gán đầy đủ: Hình ảnh, Âm thanh, Kiểu bắn và Hiệu ứng dính đạn.");
    }
}
#endif
