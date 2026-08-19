using UnityEngine;

public class BuildNode : MonoBehaviour
{
    [Header("Node Settings")]
    public Vector3 spawnOffset = Vector3.zero;

    private GameObject currentTower;
    private SpriteRenderer rend;
    private Color originalColor;

    private void Start()
    {
        rend = GetComponent<SpriteRenderer>();
        if (rend != null) originalColor = rend.color;
    }

    private void OnMouseDown()
    {
        if (currentTower != null)
        {
            Debug.Log("[BuildNode] Tile already occupied!");
            return;
        }

        if (BuildManager.Instance == null) return;

        GameObject towerToBuild = BuildManager.Instance.GetTowerToBuild();
        if (towerToBuild == null)
        {
            Debug.Log("[BuildNode] No tower selected to build!");
            return;
        }

        TowerData data = towerToBuild.GetComponent<TowerData>();
        int cost = (data != null) ? data.cost : 0;

        if (GameManager.Instance != null && GameManager.Instance.TrySpendGold(cost))
        {
            currentTower = Instantiate(towerToBuild, transform.position + spawnOffset, Quaternion.identity);
            Debug.Log($"[BuildNode] Tower built! Remaining Gold: {GameManager.Instance.CurrentGold}");
        }
        else
        {
            Debug.Log("[BuildNode] Not enough gold!");
        }
    }

    private void OnMouseEnter()
    {
        if (rend != null && currentTower == null && BuildManager.Instance != null && BuildManager.Instance.GetTowerToBuild() != null)
        {
            rend.color = Color.green;
        }
    }

    private void OnMouseExit()
    {
        if (rend != null) rend.color = originalColor;
    }
}