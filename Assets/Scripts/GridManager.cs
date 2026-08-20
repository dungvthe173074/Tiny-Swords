using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Dimensions (Full Camera View)")]
    public Vector2 origin = new Vector2(-16.0f, 1.5f);
    public int columns = 17;
    public int rows = 10;
    public float cellSize = 1.0f;

    [Header("Visual Settings")]
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
    }

    private void Start()
    {
        GenerateGrid();
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

                GameObject tileObj = new GameObject($"Tile_{x}_{y}");
                tileObj.transform.SetParent(transform);
                tileObj.transform.position = worldPos;

                SpriteRenderer sr = tileObj.AddComponent<SpriteRenderer>();
                sr.sprite = tileBorderSprite;
                sr.sortingOrder = 0;

                BoxCollider2D col = tileObj.AddComponent<BoxCollider2D>();
                col.size = new Vector2(cellSize * 0.95f, cellSize * 0.95f);
                col.isTrigger = true;

                GridTile tile = tileObj.AddComponent<GridTile>();
                tile.gridCoord = new Vector2Int(x, y);

                bool isBridge = IsPositionOnBridge(worldPos);
                tile.SetTileState(!isBridge);

                allTiles.Add(tile);
            }
        }
    }

    public void RefreshAllTileVisuals()
    {
        foreach (var tile in allTiles)
        {
            if (tile != null) tile.UpdateVisuals();
        }
    }

    private bool IsPositionOnBridge(Vector3 pos)
    {
        //// Bottom horizontal bridge: y ≈ 3.5, x from -16.5 to -9.0
        //if (Mathf.Abs(pos.y - 3.5f) < 0.70f && pos.x <= -8.8f) return true;

        //// Vertical bridge 1: x ≈ -9.5, y from 3.0 to 7.0
        //if (Mathf.Abs(pos.x - (-9.5f)) < 0.70f && pos.y >= 3.0f && pos.y <= 7.0f) return true;

        //// Middle horizontal bridge: y ≈ 6.5, x from -10.0 to -5.0
        //if (Mathf.Abs(pos.y - 6.5f) < 0.70f && pos.x >= -10.0f && pos.x <= -5.0f) return true;

        //// Vertical bridge 2: x ≈ -5.5, y from 6.0 to 10.0
        //if (Mathf.Abs(pos.x - (-5.5f)) < 0.70f && pos.y >= 6.0f && pos.y <= 10.0f) return true;

        //// Top horizontal bridge: y ≈ 9.5, x from -6.0 to 1.5
        //if (Mathf.Abs(pos.y - 9.5f) < 0.70f && pos.x >= -6.0f) return true;

        return false;
    }

    private Sprite CreateTileBorderSprite()
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        Color[] pixels = new Color[size * size];

        Color borderColor = new Color(1f, 1f, 1f, 0.45f);
        Color innerColor = new Color(1f, 1f, 1f, 0.02f);

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

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
