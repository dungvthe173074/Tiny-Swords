using UnityEngine;

// Deprecated: Kept for backwards compatibility with any existing scene references.
// Please use WaveSpawner instead.
public class Spawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform[] waypoints;
    public float spawnInterval = 3f;

    private void Start()
    {
        // If WaveSpawner is attached, Spawner will yield to WaveSpawner
        if (GetComponent<WaveSpawner>() != null) return;

        InvokeRepeating(nameof(SpawnEnemy), 1f, spawnInterval);
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null || waypoints == null || waypoints.Length == 0) return;

        GameObject enemyObj = Instantiate(enemyPrefab, waypoints[0].position, Quaternion.identity);
        EnemyMovement movement = enemyObj.GetComponent<EnemyMovement>();
        if (movement != null)
        {
            Enemy enemy = enemyObj.GetComponent<Enemy>();
            movement.Initialize(enemy, waypoints);
        }
    }
}