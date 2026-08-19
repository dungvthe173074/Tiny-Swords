using UnityEditor.Search;
using UnityEngine;

public class TowerData : MonoBehaviour
{
    [Header("Tower Settings")]
    public int cost = 50;
    public float range = 10f;
    public float attackRate = 1f;
    public int damage = 1;

    private float attackCountdown = 0f;

    private void Update()
    {
        attackCountdown -= Time.deltaTime;

        if (attackCountdown <= 0f)
        {
            GameObject target = FindNearestEnemy();
            if (target != null)
            {
                Attack(target);
                attackCountdown = 1f / attackRate;
            }
        }
    }

    private GameObject FindNearestEnemy()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, range);
        GameObject nearest = null;
        float shortestDistance = Mathf.Infinity;

        foreach (var hit in hitEnemies)
        {
            if (hit.CompareTag("Enemy"))
            {
                float distanceToEnemy = Vector3.Distance(transform.position, hit.transform.position);
                if (distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    nearest = hit.gameObject;
                }
            }
        }
        return nearest;
    }

    private void Attack(GameObject target)
    {
        EnemyData enemyData = target.GetComponent<EnemyData>();
        if (enemyData != null)
        {
            enemyData.TakeDamage(damage);
        }
    }

    // Show attack range in Scene
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}