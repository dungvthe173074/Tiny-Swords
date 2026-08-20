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

    [MenuItem("Tools/Rebuild Map 1 Bridges & Waypoints")]
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

        // 3. Load cleaned sub-sprites for Bridges
        Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Tiny Swords (Update 010)/Terrain/Bridge/Bridge_All.png");
        Sprite hSprite = null;
        Sprite vSprite = null;
        foreach (var obj in subAssets)
        {
            if (obj is Sprite s)
            {
                if (s.name == "Bridge_All_1") hSprite = s;
                if (s.name == "Bridge_All_0") vSprite = s;
            }
        }

        // 4. Find or create Bridges parent
        GameObject bridgesParent = GameObject.Find("Bridges");
        if (bridgesParent == null)
        {
            bridgesParent = new GameObject("Bridges");
            Undo.RegisterCreatedObjectUndo(bridgesParent, "Create Bridges Parent");
        }

        for (int i = bridgesParent.transform.childCount - 1; i >= 0; i--)
        {
            GameObject.DestroyImmediate(bridgesParent.transform.GetChild(i).gameObject);
        }

        // Create SEAMLESS bridge network on the far-left grass field:
        // - Level 1 (Bottom Bridge): y = 3.5f, spanning from x = -16.0 to -9.5
        float[] h1X = new float[] { -16.0f, -14.5f, -13.0f, -11.5f, -10.0f, -9.5f };
        for (int i = 0; i < h1X.Length; i++)
        {
            CreateBridgePiece(bridgesParent.transform, $"Bridge_H1_{i + 1}", new Vector3(h1X[i], 3.5f, 0f), hSprite, 1);
        }

        // - Connector 1 (Vertical Bridge 1): x = -9.5f, seamless overlap from y = 3.5 to y = 6.5
        CreateBridgePiece(bridgesParent.transform, "Bridge_V1_1", new Vector3(-9.5f, 4.3f, 0f), vSprite != null ? vSprite : hSprite, 1);
        CreateBridgePiece(bridgesParent.transform, "Bridge_V1_2", new Vector3(-9.5f, 5.7f, 0f), vSprite != null ? vSprite : hSprite, 1);

        // - Level 2 (Middle Bridge): y = 6.5f, spanning from x = -9.5 to -5.5
        float[] h2X = new float[] { -9.5f, -8.0f, -6.5f, -5.5f };
        for (int i = 0; i < h2X.Length; i++)
        {
            CreateBridgePiece(bridgesParent.transform, $"Bridge_H2_{i + 1}", new Vector3(h2X[i], 6.5f, 0f), hSprite, 1);
        }

        // - Connector 2 (Vertical Bridge 2): x = -5.5f, seamless overlap from y = 6.5 to y = 9.5
        CreateBridgePiece(bridgesParent.transform, "Bridge_V2_1", new Vector3(-5.5f, 7.3f, 0f), vSprite != null ? vSprite : hSprite, 1);
        CreateBridgePiece(bridgesParent.transform, "Bridge_V2_2", new Vector3(-5.5f, 8.7f, 0f), vSprite != null ? vSprite : hSprite, 1);

        // - Level 3 (Top Bridge): y = 9.5f, spanning from x = -5.5 to 1.0 (exits camera top-right)
        float[] h3X = new float[] { -5.5f, -4.0f, -2.5f, -1.0f, 0.5f, 1.0f };
        for (int i = 0; i < h3X.Length; i++)
        {
            CreateBridgePiece(bridgesParent.transform, $"Bridge_H3_{i + 1}", new Vector3(h3X[i], 9.5f, 0f), hSprite, 1);
        }

        // 5. Find or create Waypoints parent
        GameObject waypointsParent = GameObject.Find("Waypoints");
        if (waypointsParent == null)
        {
            waypointsParent = new GameObject("Waypoints");
            Undo.RegisterCreatedObjectUndo(waypointsParent, "Create Waypoints Parent");
        }

        for (int i = waypointsParent.transform.childCount - 1; i >= 0; i--)
        {
            GameObject.DestroyImmediate(waypointsParent.transform.GetChild(i).gameObject);
        }

        Vector3[] wpPositions = new Vector3[]
        {
            new Vector3(-16.0f, 3.85f, 0f), // 0: Start
            new Vector3(-9.5f, 3.85f, 0f),  // 1: Turn 1
            new Vector3(-9.5f, 6.85f, 0f),  // 2: Turn 2
            new Vector3(-5.5f, 6.85f, 0f),  // 3: Turn 3
            new Vector3(-5.5f, 9.85f, 0f),  // 4: Turn 4
            new Vector3(1.0f, 9.85f, 0f)    // 5: Goal (Castle)
        };

        Transform[] wpTransforms = new Transform[wpPositions.Length];
        for (int i = 0; i < wpPositions.Length; i++)
        {
            GameObject wp = new GameObject($"Waypoint ({i})");
            wp.transform.SetParent(waypointsParent.transform);
            wp.transform.localPosition = wpPositions[i];
            wpTransforms[i] = wp.transform;
        }

        // 6. Update WaveSpawner with waypoints
        GameObject waveSpawnerObj = GameObject.Find("WaveSpawner");
        if (waveSpawnerObj != null)
        {
            WaveSpawner waveSpawner = waveSpawnerObj.GetComponent<WaveSpawner>();
            if (waveSpawner != null)
            {
                waveSpawner.waypoints = wpTransforms;
                if (waveSpawner.waves != null)
                {
                    for (int w = 0; w < waveSpawner.waves.Length; w++)
                    {
                        if (waveSpawner.waves[w] != null)
                        {
                            waveSpawner.waves[w].spawnInterval = 2f;
                        }
                    }
                }
                EditorUtility.SetDirty(waveSpawner);
            }
        }

        // 7. Setup Projectile Prefabs: Arrow, Poison, Fire
        GameObject arrowPrefab = SetupProjectilePrefab("Arrow", ProjectileType.Arrow, 12f, 20f,
            "Assets/Sprites/Units/Blue Units/Archer/Arrow.png", Color.white, new Vector3(0.8f, 0.8f, 1f));

        GameObject poisonPrefab = SetupProjectilePrefab("Poison", ProjectileType.Poison, 9f, 32f,
            "Assets/Sprites/Particle FX/Explosion_01.png", new Color(0.2f, 1.0f, 0.35f, 1.0f), new Vector3(0.9f, 0.9f, 1f), "Explosion_01_0");

        GameObject firePrefab = SetupProjectilePrefab("Fire", ProjectileType.Fire, 8.5f, 65f,
            "Assets/Sprites/Tiny Swords (Update 010)/Effects/Fire/Fire.png", Color.white, new Vector3(0.6f, 0.6f, 1f), "Fire_0");

        // 8. Setup 3 Tower Prefabs (All using the identical blue stone tower building sprite `Tower_0`)
        List<Tower> towerPrefabs = new List<Tower>();
        towerPrefabs.Add(SetupTowerPrefab("Tower_Arrow", "Tháp Tên", 50, 4.2f, 1.4f, 20f, arrowPrefab));
        towerPrefabs.Add(SetupTowerPrefab("Tower_Poison", "Tháp Độc", 75, 3.6f, 1.0f, 32f, poisonPrefab));
        towerPrefabs.Add(SetupTowerPrefab("Tower_Fire", "Tháp Lửa", 100, 3.0f, 0.7f, 65f, firePrefab));

        // 9. Setup GameManager in Scene
        GameObject gmObj = GameObject.Find("GameManager");
        if (gmObj == null)
        {
            gmObj = new GameObject("GameManager");
            Undo.RegisterCreatedObjectUndo(gmObj, "Create GameManager");
        }
        GameManager gameManager = gmObj.GetComponent<GameManager>();
        if (gameManager == null) gameManager = gmObj.AddComponent<GameManager>();
        gameManager.maxBaseHealth = 10;
        gameManager.currentBaseHealth = 10;
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

        // 9.2 Setup GridBorder Sprite & GridTile Prefab in Assets/Prefabs
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

        // 11. Setup TowerPlacementManager in Scene
        GameObject pmObj = GameObject.Find("TowerPlacementManager");
        if (pmObj == null)
        {
            pmObj = new GameObject("TowerPlacementManager");
            Undo.RegisterCreatedObjectUndo(pmObj, "Create TowerPlacementManager");
        }
        TowerPlacementManager placementManager = pmObj.GetComponent<TowerPlacementManager>();
        if (placementManager == null) placementManager = pmObj.AddComponent<TowerPlacementManager>();
        placementManager.playerGold = 200;
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
        Debug.Log("[MapSetupHelper] Complete Game with Base Health & Range label ready!");
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

        BoxCollider2D col = temp.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1.2f, 1.8f);

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

    private static void CreateBridgePiece(Transform parent, string name, Vector3 pos, Sprite sprite, int sortingOrder)
    {
        GameObject piece = new GameObject(name);
        piece.transform.SetParent(parent);
        piece.transform.localPosition = pos;
        SpriteRenderer sr = piece.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = sortingOrder;
    }

    private static Sprite GetOrCreateGridBorderSprite()
    {
        string path = "Assets/Sprites/Terrain/GridBorder.png";

        int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        Color[] pixels = new Color[size * size];
        Color borderColor = Color.white;
        Color innerColor = Color.clear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (x == 0 || x == size - 1 || y == 0 || y == size - 1)
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
        col.size = new Vector2(0.95f, 0.95f);
        col.isTrigger = true;

        GridTile tile = temp.AddComponent<GridTile>();
        tile.normalColor = new Color(1f, 1f, 1f, 0.0f);
        tile.placementModeColor = new Color(1f, 1f, 1f, 0.38f);
        tile.placementBlockedColor = new Color(1f, 1f, 1f, 0.10f);
        tile.hoverBuildableColor = new Color(0.2f, 1.0f, 0.4f, 0.70f);
        tile.hoverUnbuildableColor = new Color(1.0f, 0.2f, 0.2f, 0.70f);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, path);
        GameObject.DestroyImmediate(temp);
        return prefab;
    }
}
#endif
