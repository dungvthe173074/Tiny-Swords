using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance { get; private set; }

    [Header("Default Building Option")]
    public GameObject defaultTowerPrefab;

    private GameObject towerToBuild;

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
        towerToBuild = defaultTowerPrefab;
    }

    public GameObject GetTowerToBuild()
    {
        return towerToBuild;
    }

    public void SetTowerToBuild(GameObject towerPrefab)
    {
        towerToBuild = towerPrefab;
    }
}