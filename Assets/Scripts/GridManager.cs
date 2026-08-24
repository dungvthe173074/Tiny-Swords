using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Dimensions (Full Camera View)")]
    public Vector2 origin = new Vector2(-17.0f, 1.0f);
    public int columns = 19;
    public int rows = 11;
    public float cellSize = 1.0f;

    [Header("Visual Settings")]
    public GameObject gridTilePrefab;
    public Sprite tileBorderSprite;

    [Header("Dynamic Obstacle Detection")]
    public Transform bridgesParent;
    public Transform castleTransform;

    private readonly List<GridTile> allTiles = new List<GridTile>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (tileBorderSprite == null)
        {
            tileBorderSprite = CreateTileBorderSprite();
        }

        allTiles.Clear();
        allTiles.AddRange(GetComponentsInChildren<GridTile>());
    }

    private void Start()
    {
        if (allTiles.Count == 0)
        {
            GenerateGrid();
        }
        RefreshAllTileVisuals();
    }

    [ContextMenu("Rebuild Grid")]
    public void GenerateGrid()
    {
        // Clear previous children
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        allTiles.Clear();

        if (tileBorderSprite == null)
        {
            tileBorderSprite = CreateTileBorderSprite();
        }

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                Vector3 worldPos = new Vector3(
                    origin.x + x * cellSize + cellSize * 0.5f,
                    origin.y + y * cellSize + cellSize * 0.5f,
                    0f
                );

                GameObject tileObj = null;
#if UNITY_EDITOR
                if (gridTilePrefab != null && !Application.isPlaying)
                {
                    tileObj = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(gridTilePrefab, transform);
                }
#endif
                if (tileObj == null && gridTilePrefab != null)
                {
                    tileObj = Instantiate(gridTilePrefab, transform);
                }

                if (tileObj == null)
                {
                    tileObj = new GameObject($"Tile_{x}_{y}");
                    tileObj.transform.SetParent(transform);

                    SpriteRenderer sr = tileObj.AddComponent<SpriteRenderer>();
                    sr.sprite = tileBorderSprite;
                    sr.sortingOrder = 0;

                    BoxCollider2D col = tileObj.AddComponent<BoxCollider2D>();
                    col.size = new Vector2(cellSize, cellSize);
                    col.isTrigger = true;

                    tileObj.AddComponent<GridTile>();
                }

                tileObj.name = $"Tile_{x}_{y}";
                tileObj.transform.position = worldPos;

                GridTile tile = tileObj.GetComponent<GridTile>();
                if (tile != null)
                {
                    tile.gridCoord = new Vector2Int(x, y);

                    SpriteRenderer sr = tileObj.GetComponent<SpriteRenderer>();
                    if (sr != null && sr.sprite == null)
                    {
                        sr.sprite = tileBorderSprite;
                        sr.sortingOrder = 0;
                    }

                    allTiles.Add(tile);
                }
            }
        }

        UpdateTileBuildableStates();
        RefreshAllTileVisuals();
    }

    public void UpdateTileBuildableStates()
    {
        // 1. Collect all obstacle bounding boxes dynamically from the scene
        List<Bounds> obstacleBoundsList = GetDynamicObstacleBounds();

        // 2. Collect existing placed towers in the scene
#if UNITY_2023_1_OR_NEWER
        Tower[] existingTowers = FindObjectsByType<Tower>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
        Tower[] existingTowers = FindObjectsOfType<Tower>();
#endif

        foreach (var tile in allTiles)
        {
            if (tile == null) continue;
            Vector3 tilePos = tile.transform.position;

            bool isObstacle = IsPositionOverlappingObstacles(tilePos, obstacleBoundsList);
            bool isInsideCamera = IsTileFullyInsideCamera(tilePos);
            tile.SetTileState(!isObstacle && isInsideCamera);

            // Dynamically check if any tower already occupies this tile
            bool hasTower = false;
            Tower occupyingTower = null;
            if (existingTowers != null)
            {
                foreach (var tower in existingTowers)
                {
                    if (tower == null) continue;
                    if (Vector2.Distance(tilePos, tower.transform.position) < cellSize * 0.55f)
                    {
                        hasTower = true;
                        occupyingTower = tower;
                        break;
                    }
                }
            }

            tile.isOccupied = hasTower;
            tile.placedTower = occupyingTower;
        }
    }

    public void RefreshAllTileVisuals()
    {
        UpdateTileBuildableStates();
        foreach (var tile in allTiles)
        {
            if (tile != null) tile.UpdateVisuals();
        }
    }

    public bool IsTileFullyInsideCamera(Vector3 tileCenter)
    {
        Camera cam = Camera.main;
        if (cam == null) return true;

        float halfCell = cellSize * 0.5f;
        float camHalfHeight = cam.orthographicSize;
        float camHalfWidth = camHalfHeight * cam.aspect;
        Vector3 camPos = cam.transform.position;

        float camLeft = camPos.x - camHalfWidth;
        float camRight = camPos.x + camHalfWidth;
        float camBottom = camPos.y - camHalfHeight;
        float camTop = camPos.y + camHalfHeight;

        float tileLeft = tileCenter.x - halfCell;
        float tileRight = tileCenter.x + halfCell;
        float tileBottom = tileCenter.y - halfCell;
        float tileTop = tileCenter.y + halfCell;

        // Tile must be completely inside camera viewport bounds (with small margin)
        float margin = 0.05f;
        bool isInside = (tileLeft >= camLeft + margin) &&
                        (tileRight <= camRight - margin) &&
                        (tileBottom >= camBottom + margin) &&
                        (tileTop <= camTop - margin);

        return isInside;
    }

    /// <summary>
    /// Dynamically discovers all bridges, castle, and obstacle bounding boxes in the scene.
    /// No hardcoded coordinates required!
    /// </summary>
    public List<Bounds> GetDynamicObstacleBounds()
    {
        List<Bounds> boundsList = new List<Bounds>();

        // 1. Bridges parent GameObject (and all its children)
        if (bridgesParent == null)
        {
            GameObject bridgesObj = GameObject.Find("Bridges");
            if (bridgesObj != null) bridgesParent = bridgesObj.transform;
        }

        if (bridgesParent != null)
        {
            SpriteRenderer[] bridgeRenderers = bridgesParent.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in bridgeRenderers)
            {
                if (sr != null && sr.enabled && sr.gameObject.activeInHierarchy)
                {
                    boundsList.Add(sr.bounds);
                }
            }

            Collider2D[] bridgeColliders = bridgesParent.GetComponentsInChildren<Collider2D>();
            foreach (var col in bridgeColliders)
            {
                if (col != null && col.enabled && col.gameObject.activeInHierarchy)
                {
                    boundsList.Add(col.bounds);
                }
            }
        }

        // 2. Scan all objects in scene named "Bridge*" or tagged "Bridge" in case they are outside Bridges parent
#if UNITY_2023_1_OR_NEWER
        SpriteRenderer[] allRenderers = FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
        SpriteRenderer[] allRenderers = FindObjectsOfType<SpriteRenderer>();
#endif
        foreach (var sr in allRenderers)
        {
            if (sr == null || !sr.enabled || !sr.gameObject.activeInHierarchy) continue;
            string objName = sr.gameObject.name.ToLower();
            if (objName.Contains("bridge") || sr.gameObject.CompareTag("Bridge") || objName.Contains("obstacle"))
            {
                if (!boundsList.Contains(sr.bounds))
                {
                    boundsList.Add(sr.bounds);
                }
            }
        }

        // 3. Castle bounds
        if (castleTransform == null)
        {
            GameObject castleObj = GameObject.Find("MainCastle");
            if (castleObj != null) castleTransform = castleObj.transform;
        }
        if (castleTransform != null)
        {
            SpriteRenderer castleSr = castleTransform.GetComponent<SpriteRenderer>();
            if (castleSr != null) boundsList.Add(castleSr.bounds);

            Collider2D castleCol = castleTransform.GetComponent<Collider2D>();
            if (castleCol != null) boundsList.Add(castleCol.bounds);
        }

        return boundsList;
    }

    /// <summary>
    /// Exact mathematical lookup for the tile at any world coordinate.
    /// Immune to overlapping sprites or tall tower colliders blocking raycasts.
    /// </summary>
    public GridTile GetTileAtWorldPos(Vector3 worldPos)
    {
        int col = Mathf.FloorToInt((worldPos.x - origin.x) / cellSize);
        int row = Mathf.FloorToInt((worldPos.y - origin.y) / cellSize);

        if (col < 0 || col >= columns || row < 0 || row >= rows) return null;

        for (int i = 0; i < allTiles.Count; i++)
        {
            if (allTiles[i] != null && allTiles[i].gridCoord.x == col && allTiles[i].gridCoord.y == row)
            {
                return allTiles[i];
            }
        }
        return null;
    }

    /// <summary>
    /// Checks if a tile position overlaps with any dynamic obstacle bounds or physics obstacles.
    /// </summary>
    public bool IsPositionOverlappingObstacles(Vector3 pos, List<Bounds> obstacleBounds)
    {
        // 70% of cell footprint to accurately detect intersections without false border touching
        float checkSize = cellSize * 0.70f;
        Bounds tileBounds = new Bounds(pos, new Vector3(checkSize, checkSize, 10f));

        if (obstacleBounds != null)
        {
            foreach (var b in obstacleBounds)
            {
                if (b.Intersects(tileBounds))
                {
                    return true;
                }
            }
        }

        // Physics 2D overlap check for static obstacle colliders (bridges, castle)
        Collider2D hit = Physics2D.OverlapBox(pos, new Vector2(checkSize, checkSize), 0f);
        if (hit != null && hit.GetComponent<GridTile>() == null && hit.GetComponent<Tower>() == null)
        {
            string hitName = hit.gameObject.name.ToLower();
            if (hitName.Contains("bridge") || hitName.Contains("castle") || hitName.Contains("water") || hitName.Contains("obstacle"))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Creates a crisp 2-pixel border texture with a subtle inner tint so all 4 edges
    /// are prominently visible on any background.
    /// </summary>
    private Sprite CreateTileBorderSprite()
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        Color[] pixels = new Color[size * size];

        Color borderColor = Color.white;
        Color innerColor = new Color(1f, 1f, 1f, 0.08f); // Soft 8% inner fill for great tile clarity
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

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
