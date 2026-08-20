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

        // 2. Prevent click-through if mouse is hovering over any UI element (like the cancel button or tower buttons)
        if (TowerPlacementUI.IsMouseOverAnyUI())
        {
            if (lastHoveredTile != null)
            {
                lastHoveredTile.SetHovered(false);
                lastHoveredTile = null;
            }
            return;
        }

        // 3. Raycast to detect hover and click on GridTile
        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 worldPoint = cam.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, 10f));
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            GridTile hoveredTile = hit.collider != null ? hit.collider.GetComponent<GridTile>() : null;

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

        if (lastHoveredTile != null)
        {
            lastHoveredTile.SetHovered(false);
            lastHoveredTile = null;
        }

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

        Debug.Log($"[TowerPlacement] Đã xây {SelectedTowerPrefab.towerName} thành công! Vàng còn lại: {playerGold}");
    }

    public void AddGold(int amount)
    {
        playerGold += amount;
        OnGoldChanged?.Invoke(playerGold);
    }
}
