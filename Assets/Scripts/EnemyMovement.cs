using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyMovement : MonoBehaviour
{
    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private Enemy enemy;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemy = GetComponent<Enemy>();
    }

    /// <summary>
    /// Initialize movement with waypoints and reset index.
    /// </summary>
    public void Initialize(Enemy targetEnemy, Transform[] newWaypoints)
    {
        enemy = targetEnemy != null ? targetEnemy : GetComponent<Enemy>();
        waypoints = newWaypoints;
        currentWaypointIndex = 0;

        if (waypoints != null && waypoints.Length > 0 && waypoints[0] != null)
        {
            transform.position = waypoints[0].position;
            // Move to first target (waypoint 1 if available)
            if (waypoints.Length > 1)
            {
                currentWaypointIndex = 1;
            }
        }
    }

    /// <summary>
    /// Legacy support if SetWaypoints is called directly.
    /// </summary>
    public void SetWaypoints(Transform[] newWaypoints)
    {
        Initialize(enemy, newWaypoints);
    }

    private void Update()
    {
        if (waypoints == null || waypoints.Length == 0 || currentWaypointIndex >= waypoints.Length)
            return;

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        if (targetWaypoint == null) return;

        Vector3 currentPos = transform.position;
        Vector3 targetPos = targetWaypoint.position;
        Vector3 direction = (targetPos - currentPos);

        // Adjust sprite facing direction
        if (direction.x > 0.05f)
        {
            spriteRenderer.flipX = false;
        }
        else if (direction.x < -0.05f)
        {
            spriteRenderer.flipX = true;
        }

        // Calculate movement step
        float speed = enemy != null ? enemy.moveSpeed : 3f;
        transform.position = Vector3.MoveTowards(currentPos, targetPos, speed * Time.deltaTime);

        // Check if reached current waypoint
        if (Vector3.Distance(transform.position, targetPos) <= 0.05f)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
                // Reached the end of the path
                if (enemy != null)
                {
                    enemy.ReachGoal();
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }
}