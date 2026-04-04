using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class SetupGameAssessment : EditorWindow
{
    [MenuItem("Tools/Tái Hiện Assessment")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<SetupGameAssessment>("Cài đặt Assessment");
    }

    void OnGUI()
    {
        GUILayout.Label("Công cụ Cài đặt Môi trường Game", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox("Nhấn nút bên dưới để tự động tạo 4 loại Đạn, 4 loại Quái, gán chúng vào Player và EnemySpawner giống bản gốc.", MessageType.Info);
        
        if (GUILayout.Button("Tự Động Cấu Hình (1-Click)", GUILayout.Height(40)))
        {
            SetupAll();
        }
    }

    static void SetupAll()
    {
        // 1. Tạo các Prefab Đạn ĐÚNG NHƯ BẠN YÊU CẦU:
        // Gun 2 bắn xa toàn bản đồ
        GameObject bullet2 = CreateBulletPrefab("Bullet_ClawGun", "Bullets", "Bullet 4", 45f, 10, 1f, false);
        // Gun 3 giữ nguyên
        GameObject bullet3 = CreateBulletPrefab("Bullet_HeavyLauncher", "Bullets", "Bullet 9", 10f, 40, 1f, false);
        // Gun 4 giảm kích cỡ đạn, kèm tia lửa đuôi
        GameObject bullet4 = CreateBulletPrefab("Bullet_Blaster", "Bullets", "Bullet 2", 25f, 5, 0.5f, true);
        
        string basePrefabFolder = "Assets/Assets for the assessment/Mad Doctor Assets/Prefabs/";
        GameObject enemyNormalMelee = AssetDatabase.LoadAssetAtPath<GameObject>(basePrefabFolder + "Linh_Do.prefab");

        // Tạo Material cho tia Beam
        Material laserMat = new Material(Shader.Find("Sprites/Default"));
        Texture2D laserTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Assets for the assessment/Mad Doctor Assets/Sprites/Laser/skeleton-animation_0.png");
        if (laserTex != null) laserMat.mainTexture = laserTex;
        
        string beamMatPath = "Assets/Prefabs/LaserBeamMat.mat";
        if (AssetDatabase.LoadAssetAtPath<Material>(beamMatPath) != null) AssetDatabase.DeleteAsset(beamMatPath);
        AssetDatabase.CreateAsset(laserMat, beamMatPath);

        // Tạo các Prefab Hoạt Ảnh (Muzzle Flash và Impact) (đã làm tử tế cho bạn!)
        GameObject muzzleShoot1 = CreateVFXPrefab("Muzzle_Shoot1", "Shoot1");
        GameObject muzzleShoot2 = CreateVFXPrefab("Muzzle_Shoot2", "Shoot2");
        GameObject impactFx01 = CreateVFXPrefab("Impact_Fx01", "Collision_Fx/Fx01");
        GameObject impactFx02 = CreateVFXPrefab("Impact_Fx02", "Collision_Fx/Fx02");
        GameObject impactFx03 = CreateVFXPrefab("Impact_Fx03", "Collision_Fx/Fx03");

        // Tạo Muzzle Flash kết hợp cả Khói (Shoot2) và Tia Lửa (Shoot1) cùng lúc
        GameObject combinedMuzzle = new GameObject("Muzzle_Combined");
        GameObject part1 = Instantiate(muzzleShoot1, combinedMuzzle.transform);
        part1.transform.localPosition = Vector3.zero;
        GameObject part2 = Instantiate(muzzleShoot2, combinedMuzzle.transform);
        part2.transform.localPosition = Vector3.zero;
        
        string combinedPath = "Assets/Prefabs/Muzzle_Combined.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(combinedPath) != null) AssetDatabase.DeleteAsset(combinedPath);
        GameObject finalMuzzle = PrefabUtility.SaveAsPrefabAsset(combinedMuzzle, combinedPath);
        DestroyImmediate(combinedMuzzle);

        // Tải các file Âm Thanh
        AudioClip soundLaser = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Assets for the assessment/Mad Doctor Assets/Audio/Laser.wav");
        AudioClip soundGun2 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Assets for the assessment/Mad Doctor Assets/Audio/Shoot2.wav");
        AudioClip soundGun3 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Assets for the assessment/Mad Doctor Assets/Audio/Shoot6.wav");
        AudioClip soundGun4 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Assets for the assessment/Mad Doctor Assets/Audio/Shoot1.wav");

        // 2. Setup Enemy Bullets
        GameObject enemyBullet = CreateBulletPrefab("Enemy_Bullet", "Bullets", "Bullet 5", 15f, 10, 1f);
        // Quái cũng có thể dùng impactFx01
        Bullet eBull = enemyBullet.GetComponent<Bullet>();
        if (eBull != null) eBull.impactEffect = impactFx01;

        // 3. Setup Enemies
        GameObject enemyNormal = CreateEnemyPrefab("Enemy_BlackMelee", "Enemy Character01", Enemy.EnemyType.NormalMelee, 30, 4f, null, null);
        GameObject enemyDefender = CreateEnemyPrefab("Enemy_YellowDefender", "Enemy Character03", Enemy.EnemyType.Defender, 50, 2.5f, null, null);
        // Chèn thêm finalMuzzle cho quái có súng! (kẻ địch bắn sẽ nẩy tia lửa ở nòng súng)
        GameObject enemyShooterV = CreateEnemyPrefab("Enemy_YellowShooter", "Enemy Character02", Enemy.EnemyType.Ranged, 20, 2f, enemyBullet, finalMuzzle);
        GameObject enemyShooterB = CreateEnemyPrefab("Enemy_BlackShooter", "Enemy Character04", Enemy.EnemyType.Ranged, 20, 2.2f, enemyBullet, finalMuzzle);

        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null)
        {
            // Bổ sung lính đỏ có sẵn vào Spawner thay cho enemy thông thường nếu có
            if (enemyNormalMelee != null)
                spawner.enemyPrefabs = new GameObject[] { enemyNormalMelee, enemyDefender, enemyShooterV, enemyShooterB };
            else
                spawner.enemyPrefabs = new GameObject[] { enemyNormal, enemyDefender, enemyShooterV, enemyShooterB };
            
            EditorUtility.SetDirty(spawner);
        }
        else
        {
            Debug.LogWarning("Không tìm thấy EnemySpawner trong scene hiện tại.");
        }

        // 5. Setup Player Weapons
        PlayerShooting playerShooting = FindObjectOfType<PlayerShooting>();
        if (playerShooting != null)
        {
            playerShooting.weapons = new WeaponData[4];
            
            // W1 - Pulse Rifle
            playerShooting.weapons[0] = new WeaponData {
                weaponName = "Pulse Rifle",
                bulletPrefab = null, // Không xài cục đạn rời nữa, dùng tia beam
                fireRate = 0.1f, // Tốc độ sát thương
                shootSound = soundLaser,
                weaponIcon = GetFirstSpriteInFolder("Gun01"),
                weaponAnimator = CreatePlayerAnimator("Gun01", "Player_Gun01"),
                fireMode = FireMode.Beam,
                beamMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Prefabs/LaserBeamMat.mat"),
                impactEffect = impactFx01,
                muzzleFlash = null,
                bulletCount = 1,
                spreadSpacing = 0
            };

            // W2 - Claw Gun
            playerShooting.weapons[1] = new WeaponData {
                weaponName = "Claw Gun",
                bulletPrefab = bullet2,
                fireRate = 0.35f,
                shootSound = soundGun2,
                weaponIcon = GetFirstSpriteInFolder("Gun02"),
                weaponAnimator = CreatePlayerAnimator("Gun02", "Player_Gun02"),
                fireMode = FireMode.Single, // Đổi thành bắn 1 tia
                impactEffect = impactFx02,
                muzzleFlash = finalMuzzle, // Bật lại hiệu ứng khói VÀ lửa nòng súng
                bulletCount = 1,
                spreadSpacing = 0 
            };

            // W3 - Heavy Launcher
            playerShooting.weapons[2] = new WeaponData {
                weaponName = "Heavy Launcher",
                bulletPrefab = bullet3,
                fireRate = 0.8f, // Tốc độ bắn chậm
                shootSound = soundGun3,
                weaponIcon = GetFirstSpriteInFolder("Gun03"),
                weaponAnimator = CreatePlayerAnimator("Gun03", "Player_Gun03"),
                fireMode = FireMode.Single,
                impactEffect = impactFx03,
                muzzleFlash = finalMuzzle, // Bật lại lửa nòng kết hợp khói
                bulletCount = 1,
                spreadSpacing = 0
            };

            // W4 - Blaster
            playerShooting.weapons[3] = new WeaponData {
                weaponName = "Blaster",
                bulletPrefab = bullet4,
                fireRate = 0.25f, // Giảm tốc độ bắn để không quá nhanh (trước đó là 0.08f)
                shootSound = soundGun4,
                weaponIcon = GetFirstSpriteInFolder("Gun04"),
                weaponAnimator = CreatePlayerAnimator("Gun04", "Player_Gun04"),
                fireMode = FireMode.Single,
                impactEffect = impactFx01,
                muzzleFlash = finalMuzzle, // Bật lại khói nòng kết hợp tia lửa
                bulletCount = 1,
                spreadSpacing = 0
            };

            EditorUtility.SetDirty(playerShooting);
        }
        else
        {
            Debug.LogWarning("Không tìm thấy PlayerShooting (Nhân vật chính) trong scene.");
        }

        AssetDatabase.SaveAssets();
        Debug.Log("<color=green>Đã cấu hình tự động 4 Súng, 4 Đạn, 4 Quái thành công! Hãy trải nghiệm.</color>");
    }

    static GameObject CreateBulletPrefab(string prefabName, string folderName, string spriteNameContains, float speed, int damage, float scale = 1f, bool addTrail = false)
    {
        string folderPath = "Assets/Assets for the assessment/Mad Doctor Assets/Sprites/" + folderName;
        string[] guids = AssetDatabase.FindAssets(spriteNameContains + " t:Sprite", new[] { folderPath });
        
        Sprite bulletSprite = null;
        if (guids.Length > 0)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            bulletSprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }
        
        GameObject go = new GameObject(prefabName);
        go.transform.localScale = new Vector3(scale, scale, 1f); // Gắn tỷ lệ kích thước do đạn to/nhỏ khác nhau
        
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        if (bulletSprite != null) sr.sprite = bulletSprite;
        
        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        
        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        
        if (addTrail)
        {
            TrailRenderer tr = go.AddComponent<TrailRenderer>();
            tr.time = 0.15f;
            tr.startWidth = 0.5f * scale; // Độ rộng vệt dựa trên kích thước
            tr.endWidth = 0f;
            tr.material = new Material(Shader.Find("Sprites/Default"));
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.4f, 0.7f), 0f), new GradientColorKey(new Color(0.6f, 0.1f, 1f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            tr.colorGradient = grad;
            tr.sortingOrder = 15;
        }

        Bullet bulletScript = go.AddComponent<Bullet>();
        bulletScript.speed = speed;
        bulletScript.damage = damage;
        
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        string localPath = "Assets/Prefabs/" + prefabName + ".prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, localPath);
        DestroyImmediate(go);
        
        return prefab;
    }

    static GameObject CreateEnemyPrefab(string prefabName, string folderNameMatch, Enemy.EnemyType type, int health, float speed, GameObject enemyBullet, GameObject muzzleFlash)
    {
        string folderPath = "Assets/Assets for the assessment/Mad Doctor Assets/Sprites/Enemy";
        string[] folders = AssetDatabase.GetSubFolders(folderPath);
        string targetFolder = string.Empty;
        
        foreach(var f in folders) {
            if (f.Contains(folderNameMatch)) {
                targetFolder = f;
                break;
            }
        }

        Sprite enemySprite = null;
        if (!string.IsNullOrEmpty(targetFolder))
        {
            string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { targetFolder });
            if (spriteGuids.Length > 0)
            {
                enemySprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(spriteGuids[0]));
            }
        }

        GameObject go = new GameObject(prefabName);
        // go.tag = "Enemy"; // Bỏ gán Tag do Tag chưa được định nghĩa trong Edit > Project Settings > Tags, và mã nguồn Bullet.cs cũng không cần dùng tag này
        
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        if (enemySprite != null) sr.sprite = enemySprite;
        
        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        if (enemySprite != null) bc.size = sr.bounds.size;
        
        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        Animator anim = go.AddComponent<Animator>(); 
        AnimatorController controller = CreateEnemyAnimator(targetFolder, prefabName);
        if (controller != null) anim.runtimeAnimatorController = controller;

        Enemy eScript = go.AddComponent<Enemy>();
        eScript.type = type;
        eScript.health = health;
        eScript.moveSpeed = speed;
        
        if (type == Enemy.EnemyType.Ranged)
        {
            eScript.enemyBulletPrefab = enemyBullet;
            eScript.stopDistance = 5f;
            eScript.fireRate = 2f;
            eScript.muzzleFlashPrefab = muzzleFlash; // Tia lửa nòng súng quái vật
            
            GameObject firePoint = new GameObject("FirePoint");
            firePoint.transform.parent = go.transform;
            firePoint.transform.localPosition = new Vector3(1.2f, -0.1f, 0); // Kéo ra đúng tầm đầu/nòng súng quái vật
            eScript.firePoint = firePoint.transform;
        }

        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        string localPath = "Assets/Prefabs/" + prefabName + ".prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, localPath);
        DestroyImmediate(go);
        
        return prefab;
    }

    static AnimatorController CreateEnemyAnimator(string sourceFolder, string enemyName)
    {
        string animFolder = "Assets/Animations/Enemies";
        if (!AssetDatabase.IsValidFolder("Assets/Animations")) AssetDatabase.CreateFolder("Assets", "Animations");
        if (!AssetDatabase.IsValidFolder(animFolder)) AssetDatabase.CreateFolder("Assets/Animations", "Enemies");
        
        string specificFolder = animFolder + "/" + enemyName;
        if (!AssetDatabase.IsValidFolder(specificFolder)) AssetDatabase.CreateFolder(animFolder, enemyName);

        string controllerPath = specificFolder + "/" + enemyName + "Controller.controller";
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null) AssetDatabase.DeleteAsset(controllerPath);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        string[] states = { "Idle", "Walk", "Hit", "Death", "Get Hit" };

        foreach(var state in states)
        {
            string spriteFolder = sourceFolder + "/" + state;
            if (!AssetDatabase.IsValidFolder(spriteFolder)) continue;

            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { spriteFolder });
            if (guids.Length == 0) continue;

            Sprite[] sprites = new Sprite[guids.Length];
            for (int i = 0; i < guids.Length; i++)
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guids[i]));

            AnimationClip clip = new AnimationClip();
            clip.frameRate = 12f;
            
            if (state == "Idle" || state == "Walk")
            {
                AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
            }

            EditorCurveBinding curveBinding = new EditorCurveBinding();
            curveBinding.type = typeof(SpriteRenderer);
            curveBinding.propertyName = "m_Sprite";

            ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[sprites.Length];
            for (int i = 0; i < sprites.Length; i++)
            {
                keyFrames[i] = new ObjectReferenceKeyframe();
                keyFrames[i].time = i / clip.frameRate;
                keyFrames[i].value = sprites[i];
            }
            AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keyFrames);

            string clipPath = specificFolder + "/" + state + ".anim";
            if (AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath) != null) AssetDatabase.DeleteAsset(clipPath);
            AssetDatabase.CreateAsset(clip, clipPath);
            
            AnimatorState animState = controller.layers[0].stateMachine.AddState(state);
            animState.motion = clip;

            if (state == "Walk")
            {
                controller.layers[0].stateMachine.defaultState = animState;
            }
        }
        
        return controller;
    }

    static AnimatorController CreatePlayerAnimator(string subFolderName, string controllerName)
    {
        string baseFolder = "Assets/Assets for the assessment/Mad Doctor Assets/Sprites/Mad Doctor - Main Character";
        string[] allFolders = AssetDatabase.GetSubFolders(baseFolder);
        string sourceFolder = "";
        
        foreach (var sub in allFolders)
        {
            if (sub.Contains(subFolderName))
            {
                sourceFolder = sub;
                break;
            }
        }
        
        if (string.IsNullOrEmpty(sourceFolder)) return null;

        string animFolder = "Assets/Animations/PlayerWeaponAnimators";
        if (!AssetDatabase.IsValidFolder("Assets/Animations")) AssetDatabase.CreateFolder("Assets", "Animations");
        if (!AssetDatabase.IsValidFolder(animFolder)) AssetDatabase.CreateFolder("Assets/Animations", "PlayerWeaponAnimators");
        
        string specificFolder = animFolder + "/" + controllerName;
        if (!AssetDatabase.IsValidFolder(specificFolder)) AssetDatabase.CreateFolder(animFolder, controllerName);

        string controllerPath = specificFolder + "/" + controllerName + ".controller";
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null) AssetDatabase.DeleteAsset(controllerPath);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        // Map typical folders for Mad Doctor weapons
        string[] states = { "Idle", "Walk", "Death", "Shoot FX1" };
        string[] animStatesNames = { "Idle", "Walk", "Death", "Shoot" }; 

        for (int i = 0; i < states.Length; i++)
        {
            string state = states[i];
            string mappedName = animStatesNames[i];

            string spriteFolder = sourceFolder + "/" + state;
            if (!AssetDatabase.IsValidFolder(spriteFolder)) continue;

            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { spriteFolder });
            if (guids.Length == 0) continue;

            Sprite[] sprites = new Sprite[guids.Length];
            for (int k = 0; k < guids.Length; k++)
                sprites[k] = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guids[k]));

            AnimationClip clip = new AnimationClip();
            clip.frameRate = 12f;
            
            if (mappedName == "Idle" || mappedName == "Walk")
            {
                AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
            }

            EditorCurveBinding curveBinding = new EditorCurveBinding();
            curveBinding.type = typeof(SpriteRenderer);
            curveBinding.propertyName = "m_Sprite";

            ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[sprites.Length];
            for (int k = 0; k < sprites.Length; k++)
            {
                keyFrames[k] = new ObjectReferenceKeyframe();
                keyFrames[k].time = k / clip.frameRate;
                keyFrames[k].value = sprites[k];
            }
            AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keyFrames);

            string clipPath = specificFolder + "/" + mappedName + ".anim";
            if (AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath) != null) AssetDatabase.DeleteAsset(clipPath);
            AssetDatabase.CreateAsset(clip, clipPath);
            
            AnimatorState animState = controller.layers[0].stateMachine.AddState(mappedName);
            animState.motion = clip;

            if (mappedName == "Idle")
            {
                controller.layers[0].stateMachine.defaultState = animState;
            }
        }
        
        return controller;
    }

    static Sprite GetFirstSpriteInFolder(string subfolderMatch)
    {
        string searchFolder = "Assets/Assets for the assessment/Mad Doctor Assets/Sprites/Mad Doctor - Main Character";
        string[] allFolders = AssetDatabase.GetSubFolders(searchFolder);
        
        foreach (var sub in allFolders)
        {
            if (sub.Contains(subfolderMatch))
            {
                string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { sub });
                if (guids.Length > 0)
                {
                    return AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guids[1 % guids.Length])); // Get generic frame instead of UI sometimes, but let's just use 0
                }
            }
        }
        return null;
    }

    static GameObject CreateVFXPrefab(string prefabName, string subFolder)
    {
        string folderPath = "Assets/Assets for the assessment/Mad Doctor Assets/Sprites/" + subFolder;
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });
        
        if (guids.Length == 0) return null;

        Sprite[] sprites = new Sprite[guids.Length];
        for (int i = 0; i < guids.Length; i++)
            sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guids[i]));

        GameObject go = new GameObject(prefabName);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprites[0];
        sr.sortingOrder = 20; // Phủ lên trên nhân vật và súng (rất quan trọng để biểu diễn lửa nòng)
        
        Animator anim = go.AddComponent<Animator>();

        // Create anim clip
        AnimationClip clip = new AnimationClip();
        clip.frameRate = 15f; // Nhanh
        
        EditorCurveBinding curveBinding = new EditorCurveBinding();
        curveBinding.type = typeof(SpriteRenderer);
        curveBinding.propertyName = "m_Sprite";

        ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            keyFrames[i] = new ObjectReferenceKeyframe();
            keyFrames[i].time = i / clip.frameRate;
            keyFrames[i].value = sprites[i];
        }
        AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keyFrames);

        // Save clip
        if (!AssetDatabase.IsValidFolder("Assets/Animations/VFX"))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Animations")) AssetDatabase.CreateFolder("Assets", "Animations");
            AssetDatabase.CreateFolder("Assets/Animations", "VFX");
        }
        
        string clipPath = "Assets/Animations/VFX/" + prefabName + ".anim";
        if (AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath) != null) AssetDatabase.DeleteAsset(clipPath);
        AssetDatabase.CreateAsset(clip, clipPath);

        // Create Controller
        string tControllerPath = "Assets/Animations/VFX/" + prefabName + ".controller";
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(tControllerPath) != null) AssetDatabase.DeleteAsset(tControllerPath);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(tControllerPath);
        AnimatorState state = controller.layers[0].stateMachine.AddState("Play");
        state.motion = clip;
        controller.layers[0].stateMachine.defaultState = state;
        
        anim.runtimeAnimatorController = controller;
        
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
        
        string localPath = "Assets/Prefabs/" + prefabName + ".prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, localPath);
        DestroyImmediate(go);

        return prefab;
    }
}
