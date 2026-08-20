using System;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class TowerPlacementManager : MonoBehaviour
{
    public static TowerPlacementManager Instance { get; private set; }

    [Header("Economy")]
    public int playerGold = 250;

    [Header("Available Towers")]
    public List<Tower> availableTowers = new List<Tower>();

    public Tower SelectedTowerPrefab { get; private set; }
    public int SelectedTowerIndex { get; private set; } = -1;

    public bool HasSelectedTower => SelectedTowerPrefab != null;
    public bool CanAffordSelectedTower => HasSelectedTower && playerGold >= SelectedTowerPrefab.cost;

    public event Action<int> OnGoldChanged;
    public event Action<Tower> OnTowerSelected;

    private Vector3Int? lastHoveredCell = null;
    private GridTile lastHoveredTile = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        Vector2 mouseScreenPos = Vector2.zero;
        bool rightPressed = false;
        bool leftPressed = false;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            mouseScreenPos = Mouse.current.position.ReadValue();
            rightPressed = Mouse.current.rightButton.wasPressedThisFrame;
            leftPressed = Mouse.current.leftButton.wasPressedThisFrame;
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        mouseScreenPos = Input.mousePosition;
        rightPressed = rightPressed || Input.GetMouseButtonDown(1);
        leftPressed = leftPressed || Input.GetMouseButtonDown(0);
#endif

        // 1. Right Click to cancel
        if (rightPressed && HasSelectedTower)
        {
            DeselectTower();
            return;
        }

        // 2. Prevent click-through if mouse is hovering over any UI element
        if (TowerPlacementUI.IsMouseOverAnyUI())
        {
            ClearAllHovers();
            return;
        }

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 worldPoint = cam.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, 10f));

        // 3A. Check for GridTile Prefab (Raycast 2D)
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);
        GridTile hoveredTile = hit.collider != null ? hit.collider.GetComponent<GridTile>() : null;

        if (hoveredTile != null || lastHoveredTile != null)
        {
            if (hoveredTile != lastHoveredTile)
            {
                if (lastHoveredTile != null) lastHoveredTile.SetHovered(false);
                if (hoveredTile != null) hoveredTile.SetHovered(true);
                lastHoveredTile = hoveredTile;
            }

            if (leftPressed && hoveredTile != null && HasSelectedTower)
            {
                OnTileClicked(hoveredTile);
            }
            return;
        }

        // 3B. Tilemap Cell Detection & Hover (Fallback to TilemapManager if active)
        if (TilemapManager.Instance != null)
        {
            Vector3Int cellPos = TilemapManager.Instance.WorldToCell(worldPoint);

            if (TilemapManager.Instance.IsCellWithinGrid(cellPos))
            {
                lastHoveredCell = cellPos;

                if (HasSelectedTower)
                {
                    bool canBuild = TilemapManager.Instance.IsCellBuildable(cellPos) 
                                    && !TilemapManager.Instance.IsCellOccupied(cellPos) 
                                    && CanAffordSelectedTower;
                    TilemapManager.Instance.SetHoveredCell(cellPos, canBuild);

                    if (leftPressed)
                    {
                        OnCellClicked(cellPos);
                    }
                }
                else
                {
                    bool canBuild = TilemapManager.Instance.IsCellBuildable(cellPos) 
                                    && !TilemapManager.Instance.IsCellOccupied(cellPos);
                    TilemapManager.Instance.SetHoveredCell(cellPos, canBuild);
                }
            }
            else
            {
                TilemapManager.Instance.ClearHover();
                lastHoveredCell = null;
            }
        }
    }

    private void ClearAllHovers()
    {
        if (lastHoveredTile != null)
        {
            lastHoveredTile.SetHovered(false);
            lastHoveredTile = null;
        }

        if (TilemapManager.Instance != null)
        {
            TilemapManager.Instance.ClearHover();
            lastHoveredCell = null;
        }
    }

    public void SelectTower(int index)
    {
        if (index < 0 || index >= availableTowers.Count) return;

        if (SelectedTowerIndex == index)
        {
            DeselectTower();
            return;
        }

        SelectedTowerIndex = index;
        SelectedTowerPrefab = availableTowers[index];
        OnTowerSelected?.Invoke(SelectedTowerPrefab);

        if (GridManager.Instance != null)
        {
            GridManager.Instance.RefreshAllTileVisuals();
        }
    }

    public void DeselectTower()
    {
        SelectedTowerIndex = -1;
        SelectedTowerPrefab = null;
        OnTowerSelected?.Invoke(null);

        ClearAllHovers();

        if (GridManager.Instance != null)
        {
            GridManager.Instance.RefreshAllTileVisuals();
        }
    }

    public void OnTileClicked(GridTile tile)
    {
        if (!HasSelectedTower) return;

        if (!tile.isBuildable || tile.isOccupied)
        {
            Debug.Log("[TowerPlacement] Vị trí này không thể đặt tháp (vướng cầu hoặc đã có tháp)!");
            return;
        }

        if (playerGold < SelectedTowerPrefab.cost)
        {
            Debug.Log($"[TowerPlacement] Không đủ vàng! Cần {SelectedTowerPrefab.cost} vàng, bạn có {playerGold} vàng.");
            return;
        }

        // Build tower
        playerGold -= SelectedTowerPrefab.cost;
        OnGoldChanged?.Invoke(playerGold);

        Vector3 spawnPos = tile.transform.position + new Vector3(0f, 0.2f, 0f);
        GameObject towerObj = Instantiate(SelectedTowerPrefab.gameObject, spawnPos, Quaternion.identity);
        tile.isOccupied = true;
        tile.placedTower = towerObj.GetComponent<Tower>();
        tile.UpdateVisuals();

        if (GridManager.Instance != null)
        {
            GridManager.Instance.RefreshAllTileVisuals();
        }

        Debug.Log($"[TowerPlacement] Đã xây {SelectedTowerPrefab.towerName} thành công! Vàng còn lại: {playerGold}");
    }

    public void OnCellClicked(Vector3Int cellPos)
    {
        if (!HasSelectedTower) return;
        if (TilemapManager.Instance == null) return;

        if (!TilemapManager.Instance.IsCellBuildable(cellPos) || TilemapManager.Instance.IsCellOccupied(cellPos))
        {
            Debug.Log("[TowerPlacement] Vị trí này không thể đặt tháp (vướng cầu hoặc đã có tháp)!");
            return;
        }

        if (playerGold < SelectedTowerPrefab.cost)
        {
            Debug.Log($"[TowerPlacement] Không đủ vàng! Cần {SelectedTowerPrefab.cost} vàng, bạn có {playerGold} vàng.");
            return;
        }

        // Build tower
        playerGold -= SelectedTowerPrefab.cost;
        OnGoldChanged?.Invoke(playerGold);

        Vector3 spawnPos = TilemapManager.Instance.GetCellCenterWorld(cellPos) + new Vector3(0f, 0.2f, 0f);
        GameObject towerObj = Instantiate(SelectedTowerPrefab.gameObject, spawnPos, Quaternion.identity);
        Tower tower = towerObj.GetComponent<Tower>();

        TilemapManager.Instance.PlaceTower(cellPos, tower);
        TilemapManager.Instance.ClearHover();

        Debug.Log($"[TowerPlacement] Đã xây {SelectedTowerPrefab.towerName} tại ô {cellPos}! Vàng còn lại: {playerGold}");
    }

    public void AddGold(int amount)
    {
        playerGold += amount;
        OnGoldChanged?.Invoke(playerGold);
    }
}
