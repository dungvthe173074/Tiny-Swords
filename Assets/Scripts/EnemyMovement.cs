using UnityEngine;
using UnityEngine.Pool;

public class EnemyMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    private Transform[] waypoints;
    private int waveIndex = 0;
    private IObjectPool<EnemyMovement> objectPool;

    public void SetPool(IObjectPool<EnemyMovement> pool)
    {
        objectPool = pool;
    }

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
        if (waypoints == null || waypoints.Length == 0 || waveIndex >= waypoints.Length) return;

        Transform target = waypoints[waveIndex];
        Vector3 dir = target.position - transform.position;
        transform.Translate(dir.normalized * moveSpeed * Time.deltaTime, Space.World);

        if (Vector3.Distance(transform.position, target.position) <= 0.1f)
        {
            if (waveIndex >= waypoints.Length - 1)
            {
                if (objectPool != null)
                {
                    objectPool.Release(this);
                }
                else
                {
                    Destroy(gameObject); // Fallback
                }
                return;
            }
            waveIndex++;
        }
    }
}