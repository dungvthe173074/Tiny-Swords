using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour
{
    public static TilemapManager Instance { get; private set; }

    [Header("Grid & Tilemap References")]
    public Grid grid;
    public Tilemap gridTilemap;
    public Tilemap hoverTilemap;

    [Header("Tile Assets (Optional - auto-generated if null)")]
    public TileBase gridBorderTile;
    public TileBase hoverValidTile;
    public TileBase hoverInvalidTile;

    [Header("Grid Dimensions (Full Camera View)")]
    public Vector2 origin = new Vector2(-17.0f, 1.0f);
    public int columns = 19;
    public int rows = 11;
    public float cellSize = 1.0f;

    [Header("Bridge / Path Bounds")]
    private readonly HashSet<Vector3Int> buildableCells = new HashSet<Vector3Int>();
    private readonly Dictionary<Vector3Int, Tower> occupiedCells = new Dictionary<Vector3Int, Tower>();

    private Vector3Int? currentHoveredCell = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (grid == null) grid = GetComponentInParent<Grid>() ?? GetComponent<Grid>();
        if (grid != null)
        {
            grid.transform.position = new Vector3(origin.x, origin.y, 0f);
        }
        InitializeGridBounds();
    }

    private void Start()
    {
        if (gridTilemap == null || gridTilemap.GetTilesBlock(gridTilemap.cellBounds).Length == 0)
        {
            GenerateTilemapGrid();
        }
    }

    /// <summary>
    /// Converts a world coordinate into cell coordinate on the Tilemap grid.
    /// </summary>
    public Vector3Int WorldToCell(Vector3 worldPos)
    {
        if (grid != null) return grid.WorldToCell(worldPos);
        if (gridTilemap != null) return gridTilemap.WorldToCell(worldPos);

        int x = Mathf.FloorToInt((worldPos.x - origin.x) / cellSize);
        int y = Mathf.FloorToInt((worldPos.y - origin.y) / cellSize);
        return new Vector3Int(x, y, 0);
    }

    /// <summary>
    /// Gets the center world position of a given cell coordinate.
    /// </summary>
    public Vector3 GetCellCenterWorld(Vector3Int cellPos)
    {
        if (grid != null) return grid.GetCellCenterWorld(cellPos);
        if (gridTilemap != null) return gridTilemap.GetCellCenterWorld(cellPos);

        return new Vector3(
            origin.x + cellPos.x * cellSize + cellSize * 0.5f,
            origin.y + cellPos.y * cellSize + cellSize * 0.5f,
            0f
        );
    }

    /// <summary>
    /// Check if cell is within the defined columns & rows.
    /// </summary>
    public bool IsCellWithinGrid(Vector3Int cellPos)
    {
        return cellPos.x >= 0 && cellPos.x < columns && cellPos.y >= 0 && cellPos.y < rows;
    }

    /// <summary>
    /// Check if cell is allowed for building (not on bridges/path).
    /// </summary>
    public bool IsCellBuildable(Vector3Int cellPos)
    {
        if (!IsCellWithinGrid(cellPos)) return false;
        return buildableCells.Contains(cellPos);
    }

    /// <summary>
    /// Check if cell already contains a placed tower.
    /// </summary>
    public bool IsCellOccupied(Vector3Int cellPos)
    {
        return occupiedCells.ContainsKey(cellPos) && occupiedCells[cellPos] != null;
    }

    /// <summary>
    /// Place a tower at the specified cell position.
    /// </summary>
    public bool PlaceTower(Vector3Int cellPos, Tower tower)
    {
        if (!IsCellBuildable(cellPos) || IsCellOccupied(cellPos))
        {
            return false;
        }

        occupiedCells[cellPos] = tower;
        return true;
    }

    /// <summary>
    /// Get the tower placed at a specific cell position (if any).
    /// </summary>
    public Tower GetPlacedTower(Vector3Int cellPos)
    {
        occupiedCells.TryGetValue(cellPos, out Tower tower);
        return tower;
    }

    /// <summary>
    /// Sets the highlight indicator on the hover Tilemap.
    /// </summary>
    public void SetHoveredCell(Vector3Int cellPos, bool isValid)
    {
        if (hoverTilemap == null) return;

        if (currentHoveredCell.HasValue && currentHoveredCell.Value != cellPos)
        {
            hoverTilemap.SetTile(currentHoveredCell.Value, null);
        }

        currentHoveredCell = cellPos;

        TileBase highlightTile = isValid ? hoverValidTile : hoverInvalidTile;
        if (highlightTile == null)
        {
            highlightTile = CreateRuntimeTile(isValid ? new Color(0.2f, 1f, 0.4f, 0.55f) : new Color(1f, 0.2f, 0.2f, 0.55f));
        }

        hoverTilemap.SetTile(cellPos, highlightTile);
    }

    /// <summary>
    /// Clears any active hover indicator.
    /// </summary>
    public void ClearHover()
    {
        if (hoverTilemap == null) return;

        if (currentHoveredCell.HasValue)
        {
            hoverTilemap.SetTile(currentHoveredCell.Value, null);
            currentHoveredCell = null;
        }
    }

    /// <summary>
    /// <summary>
    /// Initializes buildable vs unbuildable cell lookup data based on dynamic obstacle geometry.
    /// </summary>
    public void InitializeGridBounds()
    {
        buildableCells.Clear();
        List<Bounds> obstacleBounds = GetDynamicObstacleBounds();

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                Vector3 worldPos = GetCellCenterWorld(cellPos);

                if (!IsPositionOverlappingObstacles(worldPos, obstacleBounds))
                {
                    buildableCells.Add(cellPos);
                }
            }
        }
    }

    [ContextMenu("Generate Tilemap Grid")]
    public void GenerateTilemapGrid()
    {
        InitializeGridBounds();

        if (gridTilemap == null) return;

        gridTilemap.ClearAllTiles();

        if (gridBorderTile == null)
        {
            gridBorderTile = CreateBorderTile();
        }
        if (hoverValidTile == null)
        {
            hoverValidTile = CreateRuntimeTile(new Color(0.2f, 1f, 0.4f, 0.55f));
        }
        if (hoverInvalidTile == null)
        {
            hoverInvalidTile = CreateRuntimeTile(new Color(1f, 0.2f, 0.2f, 0.55f));
        }

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                gridTilemap.SetTile(cellPos, gridBorderTile);

                // Tint unbuildable cells slightly darker / transparent
                bool isBuildable = buildableCells.Contains(cellPos);
                gridTilemap.SetColor(cellPos, isBuildable ? new Color(1f, 1f, 1f, 0.25f) : new Color(1f, 1f, 1f, 0.05f));
            }
        }

        if (hoverTilemap != null)
        {
            hoverTilemap.ClearAllTiles();
        }
    }

    public List<Bounds> GetDynamicObstacleBounds()
    {
        List<Bounds> boundsList = new List<Bounds>();
        GameObject bridgesObj = GameObject.Find("Bridges");
        if (bridgesObj != null)
        {
            SpriteRenderer[] bridgeRenderers = bridgesObj.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in bridgeRenderers)
            {
                if (sr != null && sr.enabled && sr.gameObject.activeInHierarchy)
                {
                    boundsList.Add(sr.bounds);
                }
            }
        }

        GameObject castleObj = GameObject.Find("MainCastle");
        if (castleObj != null)
        {
            SpriteRenderer castleSr = castleObj.GetComponent<SpriteRenderer>();
            if (castleSr != null) boundsList.Add(castleSr.bounds);
        }

        return boundsList;
    }

    private bool IsPositionOverlappingObstacles(Vector3 pos, List<Bounds> obstacleBounds)
    {
        float checkSize = cellSize * 0.70f;
        Bounds tileBounds = new Bounds(pos, new Vector3(checkSize, checkSize, 10f));

        if (obstacleBounds != null)
        {
            foreach (var b in obstacleBounds)
            {
                if (b.Intersects(tileBounds)) return true;
            }
        }

        Collider2D hit = Physics2D.OverlapBox(pos, new Vector2(checkSize, checkSize), 0f);
        if (hit != null && hit.GetComponent<GridTile>() == null)
        {
            string hitName = hit.gameObject.name.ToLower();
            if (hitName.Contains("bridge") || hitName.Contains("castle") || hit.GetComponent<Tower>() != null)
            {
                return true;
            }
        }

        return false;
    }

    private Tile CreateBorderTile()
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        Color[] pixels = new Color[size * size];

        Color borderColor = new Color(1f, 1f, 1f, 0.55f);
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

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        tile.color = Color.white;
        return tile;
    }

    private Tile CreateRuntimeTile(Color solidColor)
    {
        int size = 16;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = solidColor;
        texture.SetPixels(pixels);
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        tile.color = Color.white;
        return tile;
    }
}
