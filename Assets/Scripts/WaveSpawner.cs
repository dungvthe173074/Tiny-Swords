using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class WaveConfig
    {
        public string waveName = "Wave";
        public GameObject enemyPrefab;
        public int enemyCount = 5;
        public float spawnInterval = 2f;
    }

    [Header("Wave Setup")]
    public WaveConfig[] waves;
    public Transform[] waypoints;
    public float timeBetweenWaves = 10f;

    [Header("Current Status (Read-Only)")]
    public int currentWaveIndex = 0;
    public int activeEnemiesInWave = 0;
    public bool isWaveInProgress = false;
    public float waveCooldownTimer = 0f;

    private void Awake()
    {
        // Ensure Object Pools are present
        if (EnemyObjectPool.Instance == null)
        {
            if (GetComponent<EnemyObjectPool>() == null)
            {
                gameObject.AddComponent<EnemyObjectPool>();
            }
        }

        if (ProjectileObjectPool.Instance == null)
        {
            if (GetComponent<ProjectileObjectPool>() == null)
            {
                gameObject.AddComponent<ProjectileObjectPool>();
            }
        }
    }

    private void Start()
    {
        if (waves == null || waves.Length == 0)
        {
            Debug.LogWarning("[WaveSpawner] No waves configured!");
            return;
        }

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("[WaveSpawner] No waypoints assigned!");
            return;
        }

        StartCoroutine(StartWaveSequence());
    }

    private IEnumerator StartWaveSequence()
    {
        currentWaveIndex = 0;
        yield return StartCoroutine(SpawnWave(currentWaveIndex));
    }

    private IEnumerator SpawnWave(int waveIndex)
    {
        if (waveIndex >= waves.Length) yield break;

        WaveConfig wave = waves[waveIndex];
        isWaveInProgress = true;
        activeEnemiesInWave = wave.enemyCount;

        Debug.Log($"[WaveSpawner] Starting {wave.waveName} (Wave {waveIndex + 1}/{waves.Length}) - Spawning {wave.enemyCount} enemies every {wave.spawnInterval}s.");

        for (int i = 0; i < wave.enemyCount; i++)
        {
            SpawnEnemy(wave.enemyPrefab);

            if (i < wave.enemyCount - 1)
            {
                yield return new WaitForSeconds(wave.spawnInterval);
            }
        }
    }

    private void SpawnEnemy(GameObject enemyPrefab)
    {
        if (enemyPrefab == null)
        {
            string fallbackPath = $"Assets/Prefabs/Enemy{currentWaveIndex + 1}.prefab";
#if UNITY_EDITOR
            enemyPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(fallbackPath);
            if (enemyPrefab == null && currentWaveIndex == 4)
            {
                enemyPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Boss_Orc.prefab");
            }
#endif
        }

        if (enemyPrefab == null)
        {
            Debug.LogError("[WaveSpawner] Enemy Prefab is missing for current wave!");
            activeEnemiesInWave--;
            if (activeEnemiesInWave <= 0)
            {
                OnEnemyDespawned(null);
            }
            return;
        }

        Vector3 spawnPos = waypoints != null && waypoints.Length > 0 ? waypoints[0].position : transform.position;
        GameObject enemyObj = EnemyObjectPool.Instance.GetEnemy(enemyPrefab, spawnPos, Quaternion.identity);

        if (enemyObj != null)
        {
            Enemy enemy = enemyObj.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.Initialize(waypoints, OnEnemyDespawned);
            }
            else
            {
                Debug.LogError("[WaveSpawner] Spawned object is missing Enemy component!");
            }
        }
    }

    private void OnEnemyDespawned(Enemy enemy)
    {
        activeEnemiesInWave--;
        Debug.Log($"[WaveSpawner] Enemy despawned. Remaining in wave: {activeEnemiesInWave}");

        if (activeEnemiesInWave <= 0)
        {
            isWaveInProgress = false;
            currentWaveIndex++;

            if (currentWaveIndex < waves.Length)
            {
                StartCoroutine(WaveCooldownRoutine());
            }
            else
            {
                Debug.Log("[WaveSpawner] CONGRATULATIONS! ALL WAVES COMPLETED!");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.Victory();
                }
            }
        }
    }

    private IEnumerator WaveCooldownRoutine()
    {
        waveCooldownTimer = timeBetweenWaves;
        Debug.Log($"[WaveSpawner] Wave cleared! Next wave will begin in {timeBetweenWaves} seconds...");

        while (waveCooldownTimer > 0f)
        {
            waveCooldownTimer -= Time.deltaTime;
            yield return null;
        }

        waveCooldownTimer = 0f;
        yield return StartCoroutine(SpawnWave(currentWaveIndex));
    }
}
