using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
public class GridTile : MonoBehaviour
{
    [Header("Tile Settings")]
    public Vector2Int gridCoord;
    public bool isBuildable = true;
    public bool isOccupied = false;
    public Tower placedTower = null;

    [Header("Visual Colors")]
    public Color normalColor = new Color(1f, 1f, 1f, 0.0f); // Hidden when idle
    public Color placementModeColor = new Color(1f, 1f, 1f, 0.35f); // Soft, elegant white grid
    public Color placementBlockedColor = new Color(1f, 1f, 1f, 0.08f); // Very faint on bridges/offscreen
    public Color hoverBuildableColor = new Color(0.15f, 1.0f, 0.35f, 0.90f); // Vivid green hover
    public Color hoverUnbuildableColor = new Color(1.0f, 0.20f, 0.20f, 0.90f); // Vivid red hover

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private bool isHovered = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 0;
            if (spriteRenderer.sprite == null && GridManager.Instance != null)
            {
                spriteRenderer.sprite = GridManager.Instance.tileBorderSprite;
            }
        }

        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null) boxCollider.isTrigger = true;

        UpdateVisuals();
    }

    public void SetTileState(bool buildable)
    {
        isBuildable = buildable;
        UpdateVisuals();
    }

    public void SetHovered(bool hovered)
    {
        if (isHovered == hovered) return;
        isHovered = hovered;
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return;

        if (spriteRenderer.sprite == null && GridManager.Instance != null && GridManager.Instance.tileBorderSprite != null)
        {
            spriteRenderer.sprite = GridManager.Instance.tileBorderSprite;
        }

        bool hasTowerSelected = TowerPlacementManager.Instance != null && TowerPlacementManager.Instance.HasSelectedTower;

        if (hasTowerSelected)
        {
            if (isHovered)
            {
                // CRITICAL: Elevate sortingOrder to 10 on hover so all 4 border edges
                // render above all neighboring tiles and terrain sprites without any clipping
                spriteRenderer.sortingOrder = 10;

                bool canBuild = isBuildable && !isOccupied && placedTower == null && TowerPlacementManager.Instance.CanAffordSelectedTower;
                spriteRenderer.color = canBuild ? hoverBuildableColor : hoverUnbuildableColor;
            }
            else
            {
                spriteRenderer.sortingOrder = 1;
                spriteRenderer.color = (isBuildable && !isOccupied && placedTower == null) ? placementModeColor : placementBlockedColor;
            }
        }
        else
        {
            // Idle mode (no tower selected): Grid is completely hidden / clear
            spriteRenderer.sortingOrder = 0;
            spriteRenderer.color = normalColor;
        }
    }

    private void OnMouseEnter()
    {
        SetHovered(true);
    }

    private void OnMouseExit()
    {
        SetHovered(false);
    }

    private void OnMouseDown()
    {
        if (TowerPlacementManager.Instance != null)
        {
            TowerPlacementManager.Instance.OnTileClicked(this);
        }
    }
}
