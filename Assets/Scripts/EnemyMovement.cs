using UnityEngine;
using UnityEngine.Pool;

public class EnemyMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    public int damageAmount = 1;

    private Transform[] waypoints;
    private int waveIndex = 0;
    private IObjectPool<EnemyMovement> objectPool;

    public void SetPool(IObjectPool<EnemyMovement> pool) => objectPool = pool;

    public void SetWaypoints(Transform[] newWaypoints)
    {
        waypoints = newWaypoints;
    }

    private void OnEnable()
    {
        waveIndex = 0;
    }

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[waveIndex];
        Vector3 dir = target.position - transform.position;
        transform.Translate(dir.normalized * moveSpeed * Time.deltaTime, Space.World);

        if (Vector3.Distance(transform.position, target.position) <= 0.1f)
        {
            waveIndex++;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Base"))
        {
            Debug.Log($"[{gameObject.name}] Hit Base!");
            GameManager.Instance.TakeBaseDamage(damageAmount);

            Despawn();
        }
    }

    public void Despawn()
    {
        if (objectPool != null)
        {
            Debug.Log($"[{gameObject.name}] Returning to ObjectPool.");
            objectPool.Release(this);
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] No ObjectPool assigned! Destroying GameObject fallback.");
            Destroy(gameObject);
        }
    }
}