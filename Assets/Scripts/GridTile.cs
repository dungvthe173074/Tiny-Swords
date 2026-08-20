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
    public Color placementModeColor = new Color(1f, 1f, 1f, 0.38f); // Soft, thin, elegant white grid
    public Color placementBlockedColor = new Color(1f, 1f, 1f, 0.10f); // Very faint on bridges/offscreen
    public Color hoverBuildableColor = new Color(0.2f, 1.0f, 0.4f, 0.70f); // Soft green hover
    public Color hoverUnbuildableColor = new Color(1.0f, 0.2f, 0.2f, 0.70f); // Soft red hover

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
                spriteRenderer.color = (isBuildable && !isOccupied && TowerPlacementManager.Instance.CanAffordSelectedTower) 
                    ? hoverBuildableColor 
                    : hoverUnbuildableColor;
            }
            else
            {
                // Full grid is displayed across the whole screen!
                spriteRenderer.color = (isBuildable && !isOccupied) ? placementModeColor : placementBlockedColor;
            }
        }
        else
        {
            // Idle mode (no tower selected): Grid is completely hidden / clear
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
