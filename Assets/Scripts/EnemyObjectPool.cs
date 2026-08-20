using System.Collections.Generic;
using UnityEngine;

public class EnemyObjectPool : MonoBehaviour
{
    public static EnemyObjectPool Instance { get; private set; }

    private readonly Dictionary<GameObject, Queue<GameObject>> poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();
    private readonly Dictionary<GameObject, GameObject> instanceToPrefabMap = new Dictionary<GameObject, GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Retrieve an enemy instance from the pool or instantiate a new one.
    /// </summary>
    public GameObject GetEnemy(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
        {
            Debug.LogError("[EnemyObjectPool] Cannot spawn null prefab!");
            return null;
        }

        if (!poolDictionary.ContainsKey(prefab))
        {
            poolDictionary[prefab] = new Queue<GameObject>();
        }

        GameObject enemyObj = null;
        Queue<GameObject> queue = poolDictionary[prefab];

        while (queue.Count > 0 && enemyObj == null)
        {
            enemyObj = queue.Dequeue();
        }

        if (enemyObj == null)
        {
            enemyObj = Instantiate(prefab, position, rotation, transform);
            instanceToPrefabMap[enemyObj] = prefab;
        }
        else
        {
            enemyObj.transform.position = position;
            enemyObj.transform.rotation = rotation;
        }

        enemyObj.SetActive(true);
        return enemyObj;
    }

    /// <summary>
    /// Return an active enemy instance to the pool.
    /// </summary>
    public void ReturnEnemy(GameObject enemyObj)
    {
        if (enemyObj == null) return;

        enemyObj.SetActive(false);

        if (instanceToPrefabMap.TryGetValue(enemyObj, out GameObject prefab))
        {
            if (!poolDictionary.ContainsKey(prefab))
            {
                poolDictionary[prefab] = new Queue<GameObject>();
            }
            poolDictionary[prefab].Enqueue(enemyObj);
        }
        else
        {
            // Fallback if not tracked
            enemyObj.transform.SetParent(transform);
        }
    }
}
