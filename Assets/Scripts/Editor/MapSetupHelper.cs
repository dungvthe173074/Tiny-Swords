#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

[InitializeOnLoad]
public static class MapSetupHelper
{
    static MapSetupHelper()
    {
        EditorApplication.delayCall += () =>
        {
            if (!Application.isPlaying && SceneManager.GetActiveScene().name == "Map 1")
            {
                RebuildMap();
            }
        };
    }

    [MenuItem("Tools/Rebuild Map 1 (Setup Balance & Prefabs)")]
    [MenuItem("Tools/Setup Boss & Final Wave (Boss Orc)")]
    public static void RebuildMap()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name != "Map 1") return;

        // Ensure Prefab directories exist
        if (!Directory.Exists("Assets/Prefabs/Projectiles"))
        {
            Directory.CreateDirectory("Assets/Prefabs/Projectiles");
        }
        if (!Directory.Exists("Assets/Prefabs/Towers"))
        {
            Directory.CreateDirectory("Assets/Prefabs/Towers");
        }
        AssetDatabase.Refresh();

        // 0. Set Background Transform
        GameObject bgObj = GameObject.Find("Background");
        if (bgObj != null)
        {
            bgObj.transform.position = Vector3.zero;
            bgObj.transform.localScale = new Vector3(18f, 18f, 1f);
            EditorUtility.SetDirty(bgObj);
        }

        // 1. Shift Camera to frame the entire map with headroom for top towers
        GameObject camObj = GameObject.Find("Main Camera");
        if (camObj != null)
        {
            camObj.transform.position = new Vector3(-7.5f, 6.7f, -10f);
            Camera cam = camObj.GetComponent<Camera>();
            if (cam != null)
            {
                cam.orthographicSize = 5.2f;
                EditorUtility.SetDirty(cam);
            }
            EditorUtility.SetDirty(camObj);
        }

        // 2. Clean up any loose test GameObjects in the scene root
        string[] looseNames = new string[] { "Tower_0", "Arrow", "Poison", "Fire", "Grid" };
        foreach (string ln in looseNames)
        {
            GameObject loose = GameObject.Find(ln);
            if (loose != null && loose.transform.parent == null)
            {
                GameObject.DestroyImmediate(loose);
            }
        }

        // 3. Update WaveSpawner with waypoints and balanced waves
        GameObject waveSpawnerObj = GameObject.Find("WaveSpawner");
        if (waveSpawnerObj != null)
        {
            WaveSpawner waveSpawner = waveSpawnerObj.GetComponent<WaveSpawner>();
            if (waveSpawner != null)
            {
                // If waypoints are not yet assigned, automatically link from existing "Waypoints" in Scene if available
                if (waveSpawner.waypoints == null || waveSpawner.waypoints.Length == 0)
                {
                    GameObject waypointsParent = GameObject.Find("Waypoints");
                    if (waypointsParent != null && waypointsParent.transform.childCount > 0)
                    {
                        Transform[] wpTransforms = new Transform[waypointsParent.transform.childCount];
                        for (int i = 0; i < waypointsParent.transform.childCount; i++)
                        {
                            wpTransforms[i] = waypointsParent.transform.GetChild(i);
                        }
                        waveSpawner.waypoints = wpTransforms;
                    }
                }

                waveSpawner.timeBetweenWaves = 8f;

                GameObject e1 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy1.prefab");
                GameObject e2 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy2.prefab");
                GameObject e3 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy3.prefab");
                GameObject e4 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy4.prefab");

                GameObject bossPrefab = SetupBossPrefab();

                waveSpawner.waves = new WaveSpawner.WaveConfig[]
                {
                    new WaveSpawner.WaveConfig { waveName = "Wave 1 - Đội Tiên Phong (Warriors)", enemyPrefab = e1, enemyCount = 10, spawnInterval = 1.8f },
                    new WaveSpawner.WaveConfig { waveName = "Wave 2 - Cung Thủ Tập Kích (Archers)", enemyPrefab = e2, enemyCount = 10, spawnInterval = 1.6f },
                    new WaveSpawner.WaveConfig { waveName = "Wave 3 - Kỵ Sĩ Thiết Giáp (Lancers)", enemyPrefab = e3, enemyCount = 10, spawnInterval = 1.6f },
                    new WaveSpawner.WaveConfig { waveName = "Wave 4 - Đội Thầy Tu Phòng Thủ (Monks)", enemyPrefab = e4, enemyCount = 10, spawnInterval = 1.8f },
                    new WaveSpawner.WaveConfig { waveName = "Wave 5 - 👹 TRÙM CUỐI (ĐẠI TƯỚNG ORC)", enemyPrefab = bossPrefab, enemyCount = 1, spawnInterval = 1.0f },
                };
                EditorUtility.SetDirty(waveSpawner);

                // Remove loose Boss 1 object in scene if dragged manually
                GameObject looseBoss = GameObject.Find("Boss 1");
                if (looseBoss != null) Undo.DestroyObjectImmediate(looseBoss);
            }
        }

        // 6.1 Update Enemy Prefab Stats (Giảm 1/3 Máu Quái Thường & Tăng 3x Máu Boss)
        UpdateEnemyPrefab("Assets/Prefabs/Enemy1.prefab", 380f, 2.2f, 16);
        UpdateEnemyPrefab("Assets/Prefabs/Enemy2.prefab", 280f, 2.8f, 18);
        UpdateEnemyPrefab("Assets/Prefabs/Enemy3.prefab", 550f, 2.3f, 22);
        UpdateEnemyPrefab("Assets/Prefabs/Enemy4.prefab", 1200f, 1.4f, 35);
        if (File.Exists("Assets/Prefabs/Enemy.prefab"))
        {
            UpdateEnemyPrefab("Assets/Prefabs/Enemy.prefab", 380f, 2.2f, 16);
        }

        // 7. Setup Projectile Prefabs: Arrow, Poison, Fire (Tăng tốc độ bay & sát thương cân bằng)
        GameObject arrowPrefab = SetupProjectilePrefab("Arrow", ProjectileType.Arrow, 14f, 25f,
            "Assets/Sprites/Units/Blue Units/Archer/Arrow.png", Color.white, new Vector3(0.8f, 0.8f, 1f));

        GameObject poisonPrefab = SetupProjectilePrefab("Poison", ProjectileType.Poison, 10f, 42f,
            "Assets/Sprites/Particle FX/Explosion_01.png", new Color(0.2f, 1.0f, 0.35f, 1.0f), new Vector3(0.9f, 0.9f, 1f), "Explosion_01_0");

        GameObject firePrefab = SetupProjectilePrefab("Fire", ProjectileType.Fire, 9.0f, 75f,
            "Assets/Sprites/Tiny Swords (Update 010)/Effects/Fire/Fire.png", Color.white, new Vector3(0.6f, 0.6f, 1f), "Fire_0");

        // 8. Setup 3 Tower Prefabs (Cân Bằng Tháp: Tầm Bắn Xa Hơn, DPS Hợp Lý)
        List<Tower> towerPrefabs = new List<Tower>();
        towerPrefabs.Add(SetupTowerPrefab("Tower_Arrow", "Tháp Tên", 50, 4.5f, 1.6f, 25f, arrowPrefab));
        towerPrefabs.Add(SetupTowerPrefab("Tower_Poison", "Tháp Độc", 75, 3.8f, 1.2f, 42f, poisonPrefab));
        towerPrefabs.Add(SetupTowerPrefab("Tower_Fire", "Tháp Lửa", 100, 3.2f, 0.85f, 75f, firePrefab));

        // 9. Setup GameManager in Scene (Máu Nhà Chính: 15)
        GameObject gmObj = GameObject.Find("GameManager");
        if (gmObj == null)
        {
            gmObj = new GameObject("GameManager");
            Undo.RegisterCreatedObjectUndo(gmObj, "Create GameManager");
        }
        GameManager gameManager = gmObj.GetComponent<GameManager>();
        if (gameManager == null) gameManager = gmObj.AddComponent<GameManager>();
        gameManager.maxBaseHealth = 15;
        gameManager.currentBaseHealth = 15;
        EditorUtility.SetDirty(gameManager);

        // 9.1 Setup Castle building at destination goal (Nhà chính)
        GameObject castleObj = GameObject.Find("MainCastle");
        if (castleObj == null)
        {
            castleObj = new GameObject("MainCastle");
            Undo.RegisterCreatedObjectUndo(castleObj, "Create MainCastle");
        }
        castleObj.transform.position = new Vector3(1.0f, 9.85f, 0f);
        castleObj.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
        SpriteRenderer castleSr = castleObj.GetComponent<SpriteRenderer>();
        if (castleSr == null) castleSr = castleObj.AddComponent<SpriteRenderer>();
        Sprite castleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Buildings/Blue Buildings/Castle.png");
        if (castleSprite == null)
        {
            Object[] sub = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Buildings/Blue Buildings/Castle.png");
            foreach (var o in sub) { if (o is Sprite sp) { castleSprite = sp; break; } }
        }
        if (castleSprite != null) castleSr.sprite = castleSprite;
        castleSr.sortingOrder = 2;

        CastleHealthBar chb = castleObj.GetComponent<CastleHealthBar>();
        if (chb == null) chb = castleObj.AddComponent<CastleHealthBar>();

        EditorUtility.SetDirty(castleObj);

        // 9.2 Setup DayNightCycle in Scene (Chu Kỳ 30s Mượt Mà Liên Tục, Tốc Độ Đêm +30% Hợp Lý)
        GameObject dncObj = GameObject.Find("DayNightCycle");
        if (dncObj == null)
        {
            dncObj = new GameObject("DayNightCycle");
            Undo.RegisterCreatedObjectUndo(dncObj, "Create DayNightCycle");
        }
        DayNightCycle dnc = dncObj.GetComponent<DayNightCycle>();
        if (dnc == null) dnc = dncObj.AddComponent<DayNightCycle>();
        dnc.cycleDuration = 30.0f;
        dnc.maxLightIntensity = 1.0f;
        dnc.minLightIntensity = 0.2f; // 80% visibility reduction at night
        dnc.nightSpeedMultiplier = 1.30f; // Buff +30% speed
        dnc.nightCastleDamage = 1;
        dnc.daySpeedMultiplier = 1.0f; // Debuff to standard
        dnc.dayCastleDamage = 1;
        dnc.FindGlobalLightIfNeeded();
        EditorUtility.SetDirty(dnc);

        // 9.3 Setup GridBorder Sprite & GridTile Prefab in Assets/Prefabs
        Sprite gridBorderSprite = GetOrCreateGridBorderSprite();
        GameObject gridTilePrefab = SetupGridTilePrefab(gridBorderSprite);

        // 10. Setup GridManager in Scene with GridTile Prefab Instances
        GameObject gridObj = GameObject.Find("GridManager");
        if (gridObj == null)
        {
            gridObj = new GameObject("GridManager");
            Undo.RegisterCreatedObjectUndo(gridObj, "Create GridManager");
        }
        gridObj.transform.position = Vector3.zero;

        GridManager gridManager = gridObj.GetComponent<GridManager>();
        if (gridManager == null) gridManager = gridObj.AddComponent<GridManager>();
        gridManager.tileBorderSprite = gridBorderSprite;
        gridManager.gridTilePrefab = gridTilePrefab;
        gridManager.origin = new Vector2(-17.0f, 1.0f);
        gridManager.columns = 19;
        gridManager.rows = 11;
        gridManager.cellSize = 1.0f;
        gridManager.GenerateGrid();

        // Save GridManager as Prefab in Assets/Prefabs/
        PrefabUtility.SaveAsPrefabAssetAndConnect(gridObj, "Assets/Prefabs/GridManager.prefab", InteractionMode.AutomatedAction);
        EditorUtility.SetDirty(gridManager);

        // 11. Setup TowerPlacementManager in Scene (Vàng khởi đầu 250G)
        GameObject pmObj = GameObject.Find("TowerPlacementManager");
        if (pmObj == null)
        {
            pmObj = new GameObject("TowerPlacementManager");
            Undo.RegisterCreatedObjectUndo(pmObj, "Create TowerPlacementManager");
        }
        TowerPlacementManager placementManager = pmObj.GetComponent<TowerPlacementManager>();
        if (placementManager == null) placementManager = pmObj.AddComponent<TowerPlacementManager>();
        placementManager.playerGold = 250;
        placementManager.availableTowers = towerPrefabs;
        EditorUtility.SetDirty(placementManager);

        // 12. Setup TowerPlacementUI (Top-Left HUD & End Game Overlay) in Scene
        TowerPlacementUI placementUI = pmObj.GetComponent<TowerPlacementUI>();
        if (placementUI == null) placementUI = pmObj.AddComponent<TowerPlacementUI>();
        placementUI.gameOverTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/End Game/GameOver.png");
        placementUI.winTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/End Game/YouWin.png");
        placementUI.playButtonTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/End Game/PlayButton.png");
        EditorUtility.SetDirty(placementUI);

        EditorSceneManager.MarkSceneDirty(currentScene);
        EditorSceneManager.SaveOpenScenes();
        MainMenuSetupHelper.EnsureBuildSettings();
        Debug.Log("[MapSetupHelper] Game Rebalanced: 250G Start, 15 HP Castle, Balanced Enemies & Towers!");
    }

    private static GameObject SetupProjectilePrefab(string name, ProjectileType type, float speed, float damage, string spritePath, Color tint, Vector3 scale, string subSpriteName = null)
    {
        string path = $"Assets/Prefabs/Projectiles/{name}.prefab";

        GameObject temp = new GameObject(name);
        temp.transform.localScale = scale;

        SpriteRenderer sr = temp.AddComponent<SpriteRenderer>();
        Sprite s = null;
        if (!string.IsNullOrEmpty(subSpriteName))
        {
            Object[] subs = AssetDatabase.LoadAllAssetsAtPath(spritePath);
            foreach (var o in subs)
            {
                if (o is Sprite sp && sp.name == subSpriteName)
                {
                    s = sp;
                    break;
                }
            }
        }
        if (s == null)
        {
            s = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        }
        if (s == null)
        {
            Object[] subs = AssetDatabase.LoadAllAssetsAtPath(spritePath);
            foreach (var o in subs) { if (o is Sprite sp) { s = sp; break; } }
        }

        if (s != null) sr.sprite = s;
        sr.color = tint;
        sr.sortingOrder = 6;

        Projectile proj = temp.AddComponent<Projectile>();
        proj.projectileType = type;
        proj.speed = speed;
        proj.damage = damage;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, path);
        GameObject.DestroyImmediate(temp);
        return prefab;
    }

    private static Tower SetupTowerPrefab(string fileName, string towerName, int cost, float range, float rate, float dmg, GameObject projPrefab)
    {
        string path = $"Assets/Prefabs/Towers/{fileName}.prefab";
        string spritePath = "Assets/Sprites/Buildings/Blue Buildings/Tower.png";

        GameObject temp = new GameObject(fileName);
        temp.transform.localScale = new Vector3(0.65f, 0.65f, 1f);

        SpriteRenderer sr = temp.AddComponent<SpriteRenderer>();
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (s == null)
        {
            Object[] sub = AssetDatabase.LoadAllAssetsAtPath(spritePath);
            foreach (var o in sub) { if (o is Sprite sp) { s = sp; break; } }
        }
        if (s != null) sr.sprite = s;
        sr.sortingOrder = 3;

        temp.layer = 2; // Ignore Raycast layer

        // Fire point at the top battlements of the tower
        GameObject fpObj = new GameObject("FirePoint");
        fpObj.transform.SetParent(temp.transform);
        fpObj.transform.localPosition = new Vector3(0f, 0.7f, 0f);

        Tower tower = temp.AddComponent<Tower>();
        tower.towerName = towerName;
        tower.cost = cost;
        tower.attackRange = range;
        tower.fireRate = rate;
        tower.damage = dmg;
        tower.projectilePrefab = projPrefab;
        tower.firePoint = fpObj.transform;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, path);
        GameObject.DestroyImmediate(temp);
        return prefab.GetComponent<Tower>();
    }

    private static Sprite GetOrCreateGridBorderSprite()
    {
        string path = "Assets/Sprites/Terrain/GridBorder.png";

        int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        Color[] pixels = new Color[size * size];
        Color borderColor = Color.white;
        Color innerColor = new Color(1f, 1f, 1f, 0.08f); // Soft 8% inner fill
        int borderWidth = 2; // 2-pixel crisp border ensures all 4 edges are bold and sharp

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (x < borderWidth || x >= size - borderWidth || y < borderWidth || y >= size - borderWidth)
                {
                    pixels[y * size + x] = borderColor;
                }
                else
                {
                    pixels[y * size + x] = innerColor;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        byte[] pngData = ImageConversion.EncodeToPNG(texture);
        File.WriteAllBytes(path, pngData);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 64;
            importer.filterMode = FilterMode.Point;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static GameObject SetupGridTilePrefab(Sprite borderSprite)
    {
        string path = "Assets/Prefabs/GridTile.prefab";

        GameObject temp = new GameObject("GridTile");

        SpriteRenderer sr = temp.AddComponent<SpriteRenderer>();
        sr.sprite = borderSprite;
        sr.sortingOrder = 0;

        BoxCollider2D col = temp.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1.0f, 1.0f);
        col.isTrigger = true;

        GridTile tile = temp.AddComponent<GridTile>();
        tile.normalColor = new Color(1f, 1f, 1f, 0.0f);
        tile.placementModeColor = new Color(1f, 1f, 1f, 0.35f);
        tile.placementBlockedColor = new Color(1f, 1f, 1f, 0.08f);
        tile.hoverBuildableColor = new Color(0.15f, 1.0f, 0.35f, 0.90f);
        tile.hoverUnbuildableColor = new Color(1.0f, 0.20f, 0.20f, 0.90f);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, path);
        GameObject.DestroyImmediate(temp);
        return prefab;
    }

    public static GameObject SetupBossPrefab()
    {
        string path = "Assets/Prefabs/Boss_Orc.prefab";
        string spritePath = "Assets/Sprites/Tiny RPG Character Asset Pack v1.03 -Free Soldier&Orc/Characters(100x100)/Orc/Orc with shadows/Orc.png";

        Object[] subs = AssetDatabase.LoadAllAssetsAtPath(spritePath);
        Dictionary<string, Sprite> spriteDict = new Dictionary<string, Sprite>();
        foreach (var o in subs)
        {
            if (o is Sprite sp) spriteDict[sp.name] = sp;
        }

        GameObject temp = new GameObject("Boss_Orc");
        temp.transform.localScale = new Vector3(4.0f, 4.0f, 1.0f);

        SpriteRenderer sr = temp.AddComponent<SpriteRenderer>();
        if (spriteDict.ContainsKey("Orc_0")) sr.sprite = spriteDict["Orc_0"];
        sr.sortingOrder = 20;

        EnemyMovement em = temp.AddComponent<EnemyMovement>();

        Enemy enemy = temp.AddComponent<Enemy>();
        enemy.baseMaxHealth = 10000f;
        enemy.baseMoveSpeed = 0.85f;
        enemy.baseGoldReward = 500;
        enemy.baseCastleDamage = 15;
        enemy.maxHealth = 10000f;
        enemy.moveSpeed = 0.85f;
        enemy.goldReward = 500;
        enemy.castleDamage = 15;

        BossOrc boss = temp.AddComponent<BossOrc>();
        boss.bossName = "ĐẠI TƯỚNG ORC (TRÙM CUỐI)";

        // 1. Idle: Orc_0 to Orc_5 (6 frames)
        List<Sprite> idles = new List<Sprite>();
        for (int i = 0; i <= 5; i++) if (spriteDict.ContainsKey($"Orc_{i}")) idles.Add(spriteDict[$"Orc_{i}"]);
        boss.idleSprites = idles.ToArray();

        // 2. Walk: Orc_6 to Orc_13 (8 frames)
        List<Sprite> walks = new List<Sprite>();
        for (int i = 6; i <= 13; i++) if (spriteDict.ContainsKey($"Orc_{i}")) walks.Add(spriteDict[$"Orc_{i}"]);
        boss.walkSprites = walks.ToArray();

        // 3. Attack: Orc_14 to Orc_26 (Attack 1 & Attack 2 combo - 13 frames)
        List<Sprite> attacks = new List<Sprite>();
        for (int i = 14; i <= 26; i++) if (spriteDict.ContainsKey($"Orc_{i}")) attacks.Add(spriteDict[$"Orc_{i}"]);
        boss.attackSprites = attacks.ToArray();

        // 4. Hurt: Orc_27 to Orc_30 (4 frames)
        List<Sprite> hurts = new List<Sprite>();
        for (int i = 27; i <= 30; i++) if (spriteDict.ContainsKey($"Orc_{i}")) hurts.Add(spriteDict[$"Orc_{i}"]);
        boss.hurtSprites = hurts.ToArray();

        // 5. Death: Orc_31 to Orc_34 (4 frames)
        List<Sprite> deaths = new List<Sprite>();
        for (int i = 31; i <= 34; i++) if (spriteDict.ContainsKey($"Orc_{i}")) deaths.Add(spriteDict[$"Orc_{i}"]);
        boss.deathSprites = deaths.ToArray();

        boss.animFps = 9f;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, path);
        GameObject.DestroyImmediate(temp);
        return prefab;
    }

    private static void UpdateEnemyPrefab(string path, float hp, float speed, int gold)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab != null)
        {
            Enemy enemy = prefab.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.baseMaxHealth = hp;
                enemy.maxHealth = hp;
                enemy.baseMoveSpeed = speed;
                enemy.moveSpeed = speed;
                enemy.baseGoldReward = gold;
                enemy.goldReward = gold;
                enemy.baseCastleDamage = 1;
                enemy.castleDamage = 1;
                EditorUtility.SetDirty(prefab);
            }
        }
    }
}
#endif
