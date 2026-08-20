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
                    col.size = new Vector2(cellSize * 0.95f, cellSize * 0.95f);
                    col.isTrigger = true;

                    tileObj.AddComponent<GridTile>();
                }

                tileObj.name = $"Tile_{x}_{y}";
                tileObj.transform.position = worldPos;

                GridTile tile = tileObj.GetComponent<GridTile>();
                if (tile != null)
                {
                    tile.gridCoord = new Vector2Int(x, y);
                    bool isBridge = IsPositionOnBridge(worldPos);
                    bool isInsideCamera = IsTileFullyInsideCamera(worldPos);
                    tile.SetTileState(!isBridge && isInsideCamera);

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
        foreach (var tile in allTiles)
        {
            if (tile == null) continue;
            bool isBridge = IsPositionOnBridge(tile.transform.position);
            bool isInsideCamera = IsTileFullyInsideCamera(tile.transform.position);
            tile.SetTileState(!isBridge && isInsideCamera);
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

    private bool IsPositionOnBridge(Vector3 pos)
    {
        // Bottom horizontal bridge: y ≈ 3.5, x from -16.5 to -9.0
        if (Mathf.Abs(pos.y - 3.5f) < 0.70f && pos.x <= -8.8f) return true;

        // Vertical bridge 1: x ≈ -9.5, y from 3.0 to 7.0
        if (Mathf.Abs(pos.x - (-9.5f)) < 0.70f && pos.y >= 3.0f && pos.y <= 7.0f) return true;

        // Middle horizontal bridge: y ≈ 6.5, x from -10.0 to -5.0
        if (Mathf.Abs(pos.y - 6.5f) < 0.70f && pos.x >= -10.0f && pos.x <= -5.0f) return true;

        // Vertical bridge 2: x ≈ -5.5, y from 6.0 to 10.0
        if (Mathf.Abs(pos.x - (-5.5f)) < 0.70f && pos.y >= 6.0f && pos.y <= 10.0f) return true;

        // Top horizontal bridge: y ≈ 9.5, x from -6.0 to 1.5
        if (Mathf.Abs(pos.y - 9.5f) < 0.70f && pos.x >= -6.0f) return true;

        return false;
    }

    private Sprite CreateTileBorderSprite()
    {
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
                // Thin 1-pixel border
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

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
