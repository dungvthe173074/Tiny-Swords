using UnityEngine;
using UnityEngine.Pool;

public class Spawner : MonoBehaviour
{
    [Header("Pool Configuration")]
    public EnemyMovement enemyPrefab;
    public Transform[] waypoints;
    public float spawnInterval = 2f;
    public int defaultCapacity = 10;
    public int maxSize = 20;

    private IObjectPool<EnemyMovement> enemyPool;

    private void Awake()
    {
        // Initialize the object pool
        enemyPool = new ObjectPool<EnemyMovement>(
            createFunc: CreateEnemy,
            actionOnGet: OnGetEnemy,
            actionOnRelease: OnReleaseEnemy,
            actionOnDestroy: OnDestroyEnemy,
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
    }

    private void Start()
    {
        InvokeRepeating(nameof(SpawnEnemy), 1f, spawnInterval);
    }

    void SpawnEnemy()
    {
        // Retrieve an enemy from the pool
        enemyPool.Get();
    }

    private EnemyMovement CreateEnemy()
    {
        EnemyMovement enemy = Instantiate(enemyPrefab);
        enemy.SetPool(enemyPool);
        return enemy;
    }

    private void OnGetEnemy(EnemyMovement enemy)
    {
        enemy.transform.position = waypoints[0].position;
        enemy.transform.rotation = Quaternion.identity;
        enemy.SetWaypoints(waypoints);
        enemy.gameObject.SetActive(true);
    }

    private void OnReleaseEnemy(EnemyMovement enemy)
    {
        enemy.gameObject.SetActive(false);
    }

    private void OnDestroyEnemy(EnemyMovement enemy)
    {
        Destroy(enemy.gameObject);
    }
}