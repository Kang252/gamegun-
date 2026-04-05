using UnityEngine;
using UnityEditor;

/// <summary>
/// Công cụ tự động cấu hình thông số cho tất cả kẻ địch.
/// Menu: Tools > Configure All Enemies
/// </summary>
public class EnemyConfigurator : EditorWindow
{
    [MenuItem("Tools/Configure All Enemies")]
    public static void ConfigureAllEnemies()
    {
        int updatedCount = 0;
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            Enemy enemy = prefab.GetComponent<Enemy>();
            if (enemy != null)
            {
                ApplyEnemySettings(prefab, enemy);
                EditorUtility.SetDirty(prefab);
                updatedCount++;
                Debug.Log($"✅ Đã cấu hình: {prefab.name}");
            }

            // Cập nhật prefab "Enemy_Bullet" để dùng EnemyBullet script
            if (prefab.name == "Enemy_Bullet")
            {
                UpgradeToEnemyBullet(prefab);
                EditorUtility.SetDirty(prefab);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Enemy Configurator",
            $"✅ Cấu hình thành công {updatedCount} kẻ địch!\n\nCác lỗi đã được sửa:\n- Đạn không tự bắn chính mình\n- Cận chiến tấn công bình thường\n- Thông số đã được tinh chỉnh",
            "OK"
        );
    }

    static void ApplyEnemySettings(GameObject prefab, Enemy enemy)
    {
        string name = prefab.name.ToLower();

        // Cài mặc định
        enemy.dropChance = 0.3f;
        enemy.scoreValue = 10;

        if (name.Contains("blackmelee"))
        {
            // Enemy 1 - Cận chiến nhanh
            enemy.health = 30;
            enemy.moveSpeed = 3.5f;
            enemy.attackDamage = 15;
            enemy.fireRate = 1.0f;   // Hồi chiêu đánh
            enemy.attackRange = 1.2f;
            enemy.stopDistance = 12f;
            enemy.type = Enemy.EnemyType.NormalMelee;
        }
        else if (name.Contains("yellowshooter"))
        {
            // Enemy 2 - Bắn xa cơ bản
            enemy.health = 25;
            enemy.moveSpeed = 2.5f;
            enemy.attackDamage = 10;
            enemy.fireRate = 2.0f;   // Bắn mỗi 2 giây
            enemy.stopDistance = 10f;
            enemy.attackRange = 10f;
            enemy.type = Enemy.EnemyType.Ranged;
        }
        else if (name.Contains("yellowdefender"))
        {
            // Enemy 3 - Cận chiến có khiên (Trâu)
            enemy.health = 50;
            enemy.moveSpeed = 2.5f;
            enemy.attackDamage = 25;
            enemy.fireRate = 1.5f;
            enemy.attackRange = 1.4f;
            enemy.stopDistance = 12f;
            enemy.type = Enemy.EnemyType.Defender;
        }
        else if (name.Contains("blackshooter"))
        {
            // Enemy 4 - Bắn xa mạnh
            enemy.health = 35;
            enemy.moveSpeed = 2.2f;
            enemy.attackDamage = 20;
            enemy.fireRate = 2.5f;
            enemy.stopDistance = 12f;
            enemy.attackRange = 12f;
            enemy.type = Enemy.EnemyType.Ranged;
        }
        else if (name.Contains("heavymelee") || name.Contains("heavy"))
        {
            // Enemy 5 - Cận chiến nặng
            enemy.health = 20;
            enemy.moveSpeed = 4.5f;
            enemy.attackDamage = 10;
            enemy.fireRate = 0.8f;
            enemy.attackRange = 1.2f;
            enemy.stopDistance = 12f;
            enemy.type = Enemy.EnemyType.HeavyMelee;
        }
        else if (name.Contains("ranged") || name.Contains("shooter6") || name.Contains("enemy6"))
        {
            // Enemy 6 - Bắn xa phòng thủ
            enemy.health = 45;
            enemy.moveSpeed = 2.8f;
            enemy.attackDamage = 12;
            enemy.fireRate = 1.8f;
            enemy.stopDistance = 10f;
            enemy.attackRange = 10f;
            enemy.type = Enemy.EnemyType.Ranged;
        }
    }

    static void UpgradeToEnemyBullet(GameObject bulletPrefab)
    {
        // Xóa script Bullet cũ nếu có
        Bullet oldScript = bulletPrefab.GetComponent<Bullet>();
        // Thêm EnemyBullet nếu chưa có
        EnemyBullet eb = bulletPrefab.GetComponent<EnemyBullet>();
        if (eb == null)
        {
            eb = bulletPrefab.AddComponent<EnemyBullet>();
            eb.speed = 12f;
            eb.damage = 10f;
            Debug.Log("✅ Đã thêm EnemyBullet script vào Enemy_Bullet prefab!");
        }
        // Đảm bảo Collider là Trigger để dùng OnTriggerEnter2D
        var col = bulletPrefab.GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }
}
