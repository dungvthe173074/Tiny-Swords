using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Tower Identity")]
    public string towerName = "Tháp Cung";
    public int cost = 50;

    [Header("Combat Stats")]
    public float attackRange = 3.5f;
    public float fireRate = 1.0f; // Attacks per second
    public float damage = 25f;

    [Header("References")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    private float fireCountdown = 0f;
    private Enemy currentTarget;

    private void Start()
    {
        InvokeRepeating(nameof(UpdateTarget), 0f, 0.2f);
    }

    private void UpdateTarget()
    {
#if UNITY_2023_1_OR_NEWER
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
        Enemy[] enemies = FindObjectsOfType<Enemy>();
#endif
        float shortestDistance = Mathf.Infinity;
        Enemy nearestEnemy = null;

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;

            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance && distanceToEnemy <= attackRange)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        currentTarget = nearestEnemy;
    }

    private void Update()
    {
        if (currentTarget == null) return;

        if (fireCountdown <= 0f)
        {
            Shoot();
            fireCountdown = 1f / fireRate;
        }

        fireCountdown -= Time.deltaTime;
    }

    private void Shoot()
    {
        if (currentTarget == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + new Vector3(0f, 0.5f, 0f);

        if (projectilePrefab != null)
        {
            GameObject projObj = null;
            if (ProjectileObjectPool.Instance != null)
            {
                projObj = ProjectileObjectPool.Instance.GetProjectile(projectilePrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                projObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            }

            if (projObj != null)
            {
                Projectile proj = projObj.GetComponent<Projectile>();
                if (proj != null)
                {
                    proj.Seek(currentTarget, damage);
                }
            }
        }
        else
        {
            // Direct damage fallback
            currentTarget.TakeDamage(damage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
