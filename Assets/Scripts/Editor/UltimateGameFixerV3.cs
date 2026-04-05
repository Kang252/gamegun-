using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;

public class UltimateGameFixerV3 : EditorWindow
{
    [MenuItem("Tools/Sửa Lỗi Cuối Cùng (Ultimate V3)")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<UltimateGameFixerV3>("Ultimate Fix V3");
    }

    void OnGUI()
    {
        GUILayout.Label("BỘ FIX LỖI CUỐI CÙNG (V3) - HUD CHUẨN MẪU", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Nhấn nút dưới để dựng lại HUD 3 lớp (Nền hồng + Súng crop + Rotate symbol) và fix lỗi nhặt máu.", MessageType.Info);

        if (GUILayout.Button("CHẠY NGAY (HUD 100%)", GUILayout.Height(50)))
        {
            FixAll();
        }
    }

    public static void FixAll()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject cvObj = new GameObject("Canvas");
            canvas = cvObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            cvObj.AddComponent<CanvasScaler>();
            cvObj.AddComponent<GraphicRaycaster>();
        }

        SetupHUD(canvas);
        SetupGameOver(canvas);
        SetupHealthPickups();

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("<color=green>[UltimateFix V3] Đã sửa xong! HUD chuyên nghiệp đã được kích hoạt.</color>");
    }

    static void SetupHUD(Canvas canvas)
    {
        // Xóa HUD cũ
        string[] oldNames = { "HUD_Final", "HUD_V2", "HUD" };
        foreach(var n in oldNames) {
            Transform t = canvas.transform.Find(n);
            if (t != null) DestroyImmediate(t.gameObject);
        }

        GameObject hud = new GameObject("HUD_V3");
        hud.transform.SetParent(canvas.transform, false);
        RectTransform rt = hud.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // ---- 1. KILLS TEXT ----
        GameObject kObj = new GameObject("Kills");
        kObj.transform.SetParent(hud.transform, false);
        RectTransform kRt = kObj.AddComponent<RectTransform>();
        kRt.anchorMin = new Vector2(0, 1);
        kRt.anchorMax = new Vector2(0, 1);
        kRt.pivot = new Vector2(0, 1);
        kRt.anchoredPosition = new Vector2(40, -85); // Dưới thanh HP
        kRt.sizeDelta = new Vector2(300, 60);

        Text kTxt = kObj.AddComponent<Text>();
        kTxt.text = "KILLS: 0";
        kTxt.font = font;
        kTxt.fontSize = 44;
        kTxt.fontStyle = FontStyle.Bold;
        kTxt.color = Color.white;
        kTxt.alignment = TextAnchor.MiddleLeft;

        // ---- 2. WEAPON ICON GROUP ----
        GameObject wGroup = new GameObject("WeaponContainer");
        wGroup.transform.SetParent(hud.transform, false);
        RectTransform wGRt = wGroup.AddComponent<RectTransform>();
        wGRt.anchorMin = new Vector2(0, 1);
        wGRt.anchorMax = new Vector2(0, 1);
        wGRt.pivot = new Vector2(0, 1);
        wGRt.anchoredPosition = new Vector2(40, -145);
        wGRt.sizeDelta = new Vector2(160, 160);

        // a. Background (Pink tilted square)
        GameObject bgObj = new GameObject("BackgroundPink");
        bgObj.transform.SetParent(wGroup.transform, false);
        RectTransform bgRt = bgObj.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        bgRt.localRotation = Quaternion.Euler(0, 0, -10f); // Nghiêng nhẹ

        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.sprite = FindSprite("BGWeapon");
        bgImg.color = new Color(1f, 0f, 1f, 1f); // Magenta / Pink rực rỡ
        bgImg.preserveAspect = true;

        // b. Gun Icon (Cửa sổ súng)
        GameObject gObj = new GameObject("GunIcon");
        gObj.transform.SetParent(wGroup.transform, false);
        RectTransform gRt = gObj.AddComponent<RectTransform>();
        gRt.anchorMin = new Vector2(0.1f, 0.1f);
        gRt.anchorMax = new Vector2(0.9f, 0.9f);
        gRt.offsetMin = Vector2.zero;
        gRt.offsetMax = Vector2.zero;

        Image gImg = gObj.AddComponent<Image>();
        gImg.preserveAspect = true;
        gImg.enabled = false;

        // c. Symbol Rotate (Yellow circle)
        GameObject rbObj = new GameObject("RotateSymbol");
        rbObj.transform.SetParent(wGroup.transform, false);
        RectTransform rbRt = rbObj.AddComponent<RectTransform>();
        rbRt.anchorMin = new Vector2(0.7f, 0f);
        rbRt.anchorMax = new Vector2(1f, 0.3f);
        rbRt.anchoredPosition = new Vector2(0, 10);
        rbRt.sizeDelta = Vector2.zero;

        Image rbImg = rbObj.AddComponent<Image>();
        rbImg.sprite = FindSprite("radioBtn");
        rbImg.color = new Color(1f, 0.85f, 0f, 1f); // Vàng

        // Gán vào scripts
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.scoreText = kTxt;
            EditorUtility.SetDirty(gm);
        }

        PlayerShooting ps = FindObjectOfType<PlayerShooting>();
        if (ps != null)
        {
            ps.weaponUI = gImg;
            // Vùng Crop súng tốt nhất cho bộ nhân vật này
            ps.weaponCropRect = new Rect(280, 120, 220, 220); 
            EditorUtility.SetDirty(ps);
        }
    }

    static void SetupGameOver(Canvas canvas)
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm == null) return;

        // Xóa panels cũ
        string[] oldPanels = { "GameOverPanel_Ultimate", "GameOverPanel_Fresh", "GameOverPanel" };
        foreach(var n in oldPanels) {
            Transform t = canvas.transform.Find(n);
            if (t != null) DestroyImmediate(t.gameObject);
        }
        if (gm.gameOverPanel != null) DestroyImmediate(gm.gameOverPanel);

        GameObject bg = new GameObject("GameOverPanel_V3");
        bg.transform.SetParent(canvas.transform, false);
        RectTransform bgRt = bg.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0.85f);
        bg.transform.SetAsLastSibling();

        GameObject popup = new GameObject("Popup");
        popup.transform.SetParent(bg.transform, false);
        RectTransform popRt = popup.AddComponent<RectTransform>();
        popRt.anchorMin = new Vector2(0.15f, 0.2f);
        popRt.anchorMax = new Vector2(0.85f, 0.8f);
        popRt.offsetMin = Vector2.zero;
        popRt.offsetMax = Vector2.zero;

        Image popImg = popup.AddComponent<Image>();
        popImg.sprite = FindSprite("PopUPbox2") ?? FindSprite("PopUPbox");
        popImg.type = Image.Type.Sliced;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Title
        GameObject tObj = CreateUIText(popup.transform, "Title", "GAME OVER!", font, 85, Color.white, FontStyle.Bold, new Vector2(0, 0.7f), new Vector2(1, 0.95f));
        GameObject sObj = CreateUIText(popup.transform, "Score", "SCORE: 0", font, 50, Color.yellow, FontStyle.Bold, new Vector2(0, 0.45f), new Vector2(1, 0.65f));
        GameObject hObj = CreateUIText(popup.transform, "High", "HIGHSCORE: 0", font, 40, Color.cyan, FontStyle.Normal, new Vector2(0, 0.3f), new Vector2(1, 0.5f));

        // Btn
        GameObject btnObj = new GameObject("RestartBtn");
        btnObj.transform.SetParent(popup.transform, false);
        RectTransform brt = btnObj.AddComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.25f, 0.05f);
        brt.anchorMax = new Vector2(0.75f, 0.25f);
        brt.offsetMin = Vector2.zero;
        brt.offsetMax = Vector2.zero;

        Image bImg = btnObj.AddComponent<Image>();
        bImg.sprite = FindSprite("Btn2") ?? FindSprite("Btn1");
        
        Button b = btnObj.AddComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(b.onClick, gm.RestartGame);

        CreateUIText(btnObj.transform, "Text", "PLAY AGAIN", font, 35, Color.white, FontStyle.Bold, Vector2.zero, Vector2.one);

        gm.gameOverPanel = bg;
        gm.finalScoreText = sObj.GetComponent<Text>();
        gm.highScoreText = hObj.GetComponent<Text>();
        EditorUtility.SetDirty(gm);

        bg.SetActive(false);
    }

    static GameObject CreateUIText(Transform parent, string name, string content, Font font, int size, Color color, FontStyle style, Vector2 animin, Vector2 animax) {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        Text t = obj.AddComponent<Text>();
        t.text = content; t.font = font; t.fontSize = size; t.color = color; t.fontStyle = style; t.alignment = TextAnchor.MiddleCenter;
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = animin; rt.anchorMax = animax;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        return obj;
    }

    static void SetupHealthPickups()
    {
        string[] prefNames = { "HealthPickup_Green", "HealthPickup_Red", "HealthPickup_Purple" };
        GameObject[] potions = new GameObject[3];

        for (int i = 0; i < 3; i++)
        {
            string path = "Assets/Prefabs/" + prefNames[i] + ".prefab";
            GameObject pref = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (pref != null)
            {
                using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
                {
                    scope.prefabContentsRoot.transform.localScale = new Vector3(0.3f, 0.3f, 1);
                    BoxCollider2D bc = scope.prefabContentsRoot.GetComponent<BoxCollider2D>();
                    if (bc != null) { bc.isTrigger = true; bc.size = new Vector2(3.5f, 3.5f); }
                    Rigidbody2D rb = scope.prefabContentsRoot.GetComponent<Rigidbody2D>();
                    if (rb == null) rb = scope.prefabContentsRoot.AddComponent<Rigidbody2D>();
                    rb.bodyType = RigidbodyType2D.Kinematic;
                }
                potions[i] = pref;
            }
        }

        // 1. Sửa tất cả Enemy trong Scene
        Enemy[] sceneEnemies = FindObjectsOfType<Enemy>();
        foreach (var e in sceneEnemies) 
        { 
            e.healthPickupPrefabs = potions; 
            e.dropChance = 0.2f; 
            e.scoreValue = 1; 
            EditorUtility.SetDirty(e); 
        }

        // 2. Sửa tất cả Enemy Prefab trong Project
        string[] enemyGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in enemyGuids)
        {
            string pPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject pObj = AssetDatabase.LoadAssetAtPath<GameObject>(pPath);
            if (pObj != null)
            {
                Enemy eComp = pObj.GetComponent<Enemy>();
                if (eComp != null)
                {
                    using (var scope = new PrefabUtility.EditPrefabContentsScope(pPath))
                    {
                        Enemy ep = scope.prefabContentsRoot.GetComponent<Enemy>();
                        if (ep != null)
                        {
                            ep.scoreValue = 1;
                            ep.healthPickupPrefabs = potions;
                            ep.dropChance = 0.2f;
                        }
                    }
                }
            }
        }
    }

    static Sprite FindSprite(string name)
    {
        string[] guids = AssetDatabase.FindAssets(name + " t:Sprite");
        if (guids.Length > 0)
            return AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guids[0]));
        return null;
    }
}
