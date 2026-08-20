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
    public Color normalColor = new Color(1f, 1f, 1f, 0.08f);
    public Color placementModeColor = new Color(1f, 1f, 1f, 0.25f);
    public Color hoverBuildableColor = new Color(0.2f, 1.0f, 0.4f, 0.55f);
    public Color hoverUnbuildableColor = new Color(1.0f, 0.2f, 0.2f, 0.55f);

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private bool isHovered = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.isTrigger = true;
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

        if (isHovered)
        {
            if (TowerPlacementManager.Instance != null && TowerPlacementManager.Instance.HasSelectedTower)
            {
                spriteRenderer.color = (isBuildable && !isOccupied && TowerPlacementManager.Instance.CanAffordSelectedTower) 
                    ? hoverBuildableColor 
                    : hoverUnbuildableColor;
            }
            else
            {
                spriteRenderer.color = isBuildable && !isOccupied ? hoverBuildableColor : hoverUnbuildableColor;
            }
        }
        else
        {
            if (TowerPlacementManager.Instance != null && TowerPlacementManager.Instance.HasSelectedTower)
            {
                spriteRenderer.color = isBuildable && !isOccupied ? placementModeColor : normalColor;
            }
            else
            {
                spriteRenderer.color = normalColor;
            }
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
