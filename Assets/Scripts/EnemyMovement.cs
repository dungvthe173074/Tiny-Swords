using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    private Transform[] waypoints;
    private int waveIndex = 0;

    public void SetWaypoints(Transform[] newWaypoints)
    {
        waypoints = newWaypoints;
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
                Destroy(gameObject);
                return;
            }
            waveIndex++;
        }
    }
}