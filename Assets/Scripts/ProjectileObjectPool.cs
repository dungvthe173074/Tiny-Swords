using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ProjectileObjectPool : MonoBehaviour
{
    public static ProjectileObjectPool Instance { get; private set; }

    [Header("Pool Configuration")]
    [SerializeField] private int defaultCapacity = 20;
    [SerializeField] private int maxPoolSize = 250;
    [SerializeField] private bool collectionCheck = true;

    private readonly Dictionary<GameObject, IObjectPool<GameObject>> poolDictionary = new Dictionary<GameObject, IObjectPool<GameObject>>();
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

    private IObjectPool<GameObject> GetOrCreatePool(GameObject prefab)
    {
        if (poolDictionary.TryGetValue(prefab, out IObjectPool<GameObject> existingPool))
        {
            return existingPool;
        }

        IObjectPool<GameObject> newPool = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                GameObject obj = Instantiate(prefab, transform);
                instanceToPrefabMap[obj] = prefab;
                return obj;
            },
            actionOnGet: obj =>
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            },
            actionOnRelease: obj =>
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            },
            actionOnDestroy: obj =>
            {
                if (obj != null)
                {
                    instanceToPrefabMap.Remove(obj);
                    Destroy(obj);
                }
            },
            collectionCheck: collectionCheck,
            defaultCapacity: defaultCapacity,
            maxSize: maxPoolSize
        );

        poolDictionary[prefab] = newPool;
        return newPool;
    }

    /// <summary>
    /// Retrieve a projectile instance from the pool.
    /// </summary>
    public GameObject GetProjectile(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
        {
            Debug.LogError("[ProjectileObjectPool] Cannot spawn null prefab!");
            return null;
        }

        IObjectPool<GameObject> pool = GetOrCreatePool(prefab);
        GameObject projObj = pool.Get();

        if (projObj != null)
        {
            projObj.transform.position = position;
            projObj.transform.rotation = rotation;
        }

        return projObj;
    }

    /// <summary>
    /// Return an active projectile instance to the pool.
    /// </summary>
    public void ReturnProjectile(GameObject projObj)
    {
        if (projObj == null) return;

        if (instanceToPrefabMap.TryGetValue(projObj, out GameObject prefab))
        {
            if (poolDictionary.TryGetValue(prefab, out IObjectPool<GameObject> pool))
            {
                pool.Release(projObj);
                return;
            }
        }

        // Fallback if not tracked in pool
        projObj.SetActive(false);
    }
}
