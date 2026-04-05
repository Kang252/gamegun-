using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class PlayerAnimationSetup : EditorWindow
{
    [MenuItem("Tools/Cài Đặt Hoạt Ảnh Nhân Vật (1-Click)")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<PlayerAnimationSetup>("Hoạt Ảnh Player");
    }

    void OnGUI()
    {
        GUILayout.Label("Gán Animations cho Player tùy theo Súng", EditorStyles.boldLabel);
        if (GUILayout.Button("Chạy Tự Động", GUILayout.Height(40)))
        {
            BuildPlayerAnimators();
        }
    }

    public static void BuildPlayerAnimators()
    {
        string[] guns = { "Gun01", "Gun02", "Gun03", "Gun04" };
        
        PlayerShooting ps = FindObjectOfType<PlayerShooting>();
        if (ps == null)
        {
            Debug.LogError("[PlayerAnimationSetup] Không tìm thấy PlayerShooting trong Scene!");
            return;
        }

        for (int idx = 0; idx < guns.Length; idx++)
        {
            string gunFolder = guns[idx];
            string animControllerName = "Player_" + gunFolder;

            AnimatorController controller = CreateAnimatorForGun(gunFolder, animControllerName);
            
            if (controller != null && idx < ps.weapons.Length)
            {
                ps.weapons[idx].weaponAnimator = controller;
            }
        }

        EditorUtility.SetDirty(ps);
        AssetDatabase.SaveAssets();

        // 2. Chỉnh PlayerController để nhận diện anim "Speed"
        // PlayerController đã có sẵn: anim.SetFloat("Speed", movement.sqrMagnitude);
        // => Không cần sửa code PlayerController!
        
        Debug.Log("<color=cyan>[PlayerAnimationSetup] Hoàn tất gán các Hoạt ảnh (Idle, Walk, Death, Shoot FX1) cho 4 loại súng!</color>");
    }

    static AnimatorController CreateAnimatorForGun(string subFolderName, string controllerName)
    {
        string baseFolder = "Assets/Assets for the assessment/Mad Doctor Assets/Sprites/Mad Doctor - Main Character/" + subFolderName;
        
        if (!AssetDatabase.IsValidFolder(baseFolder))
        {
            Debug.LogError("Không tìm thấy folder súng: " + baseFolder);
            return null;
        }

        string animFolder = "Assets/Animations/PlayerWeaponAnimators";
        if (!AssetDatabase.IsValidFolder("Assets/Animations")) AssetDatabase.CreateFolder("Assets", "Animations");
        if (!AssetDatabase.IsValidFolder(animFolder)) AssetDatabase.CreateFolder("Assets/Animations", "PlayerWeaponAnimators");
        
        string specificFolder = animFolder + "/" + controllerName;
        if (!AssetDatabase.IsValidFolder(specificFolder)) AssetDatabase.CreateFolder(animFolder, controllerName);

        string controllerPath = specificFolder + "/" + controllerName + ".controller";
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        }
        else
        {
            // Xóa states cũ đi làm mới cho an toàn
            var layer = controller.layers[0];
            layer.stateMachine.states = new ChildAnimatorState[0];
            controller.layers = new AnimatorControllerLayer[] { layer };
            controller.parameters = new AnimatorControllerParameter[0];
        }

        // Tạo Parameters
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Shoot", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);

        // Folders chứa sprite
        string[] sourceFolders = { "Idle", "Walk", "Death", "Shoot FX1" };
        string[] stateNames = { "Idle", "Walk", "Death", "Shoot" };

        AnimatorState stateIdle = null;
        AnimatorState stateWalk = null;
        AnimatorState stateDeath = null;
        AnimatorState stateShoot = null;

        for (int i = 0; i < sourceFolders.Length; i++)
        {
            string sFolder = baseFolder + "/" + sourceFolders[i];
            if (!AssetDatabase.IsValidFolder(sFolder)) continue;

            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { sFolder });
            if (guids.Length == 0) continue;

            Sprite[] sprites = new Sprite[guids.Length];
            for (int k = 0; k < guids.Length; k++)
                sprites[k] = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guids[k]));

            AnimationClip clip = new AnimationClip();
            clip.frameRate = 12f;
            
            if (stateNames[i] == "Idle" || stateNames[i] == "Walk")
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

            string clipPath = specificFolder + "/" + stateNames[i] + ".anim";
            AnimationClip existingClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (existingClip != null)
            {
                EditorUtility.CopySerialized(clip, existingClip);
                clip = existingClip;
            }
            else
            {
                AssetDatabase.CreateAsset(clip, clipPath);
            }

            AnimatorState animState = controller.layers[0].stateMachine.AddState(stateNames[i]);
            animState.motion = clip;

            if (stateNames[i] == "Idle") stateIdle = animState;
            else if (stateNames[i] == "Walk") stateWalk = animState;
            else if (stateNames[i] == "Death") stateDeath = animState;
            else if (stateNames[i] == "Shoot") stateShoot = animState;
        }

        // Tạo Transitions Logical (Blend Tree kiểu cũ)
        if (stateIdle != null && stateWalk != null)
        {
            // Set Default
            controller.layers[0].stateMachine.defaultState = stateIdle;

            // Idle -> Walk
            AnimatorStateTransition idleToWalk = stateIdle.AddTransition(stateWalk);
            idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.01f, "Speed");
            idleToWalk.hasExitTime = false;
            idleToWalk.duration = 0f;

            // Walk -> Idle
            AnimatorStateTransition walkToIdle = stateWalk.AddTransition(stateIdle);
            walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.01f, "Speed");
            walkToIdle.hasExitTime = false;
            walkToIdle.duration = 0f;
        }

        if (stateDeath != null)
        {
            // Any -> Death
            AnimatorStateTransition anyToDeath = controller.layers[0].stateMachine.AddAnyStateTransition(stateDeath);
            anyToDeath.AddCondition(AnimatorConditionMode.If, 0f, "Death");
            anyToDeath.hasExitTime = false;
            anyToDeath.duration = 0f;
        }

        if (stateShoot != null && stateIdle != null)
        {
            // Any -> Shoot
            AnimatorStateTransition anyToShoot = controller.layers[0].stateMachine.AddAnyStateTransition(stateShoot);
            anyToShoot.AddCondition(AnimatorConditionMode.If, 0f, "Shoot");
            anyToShoot.hasExitTime = false;
            anyToShoot.duration = 0f; // Bắn ngay lập tức

            // Shoot -> Idle (sau khi chạy hết 1 vòng hoạt ảnh)
            AnimatorStateTransition shootToIdle = stateShoot.AddTransition(stateIdle);
            shootToIdle.hasExitTime = true;
            shootToIdle.exitTime = 1f; // Chờ 100% hoạt ảnh
            shootToIdle.duration = 0f; 
        }

        EditorUtility.SetDirty(controller);
        return controller;
    }
}
