using UnityEngine;

public enum ProjectileType
{
    Arrow,
    Poison,
    Fire
}

public class Projectile : MonoBehaviour
{
    [Header("Projectile Properties")]
    public ProjectileType projectileType = ProjectileType.Arrow;
    public float speed = 10f;
    public float damage = 25f;

    private Enemy targetEnemy;

    public void Seek(Enemy target, float dmg)
    {
        targetEnemy = target;
        damage = dmg;
    }

    private void Update()
    {
        if (targetEnemy == null || !targetEnemy.gameObject.activeInHierarchy)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = targetEnemy.transform.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        // Visual orientation based on projectile type
        if (projectileType == ProjectileType.Arrow)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
        else if (projectileType == ProjectileType.Poison)
        {
            transform.Rotate(0f, 0f, 180f * Time.deltaTime);
        }
        else if (projectileType == ProjectileType.Fire)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
    }

    private void HitTarget()
    {
        if (targetEnemy != null)
        {
            targetEnemy.TakeDamage(damage);

            if (projectileType == ProjectileType.Poison)
            {
                targetEnemy.ApplySlow(0.55f, 3.5f); // 45% Slow for 3.5s
            }
            else if (projectileType == ProjectileType.Fire)
            {
                targetEnemy.ApplyBurn(30f, 3.0f); // 30 DPS Burn for 3.0s (total 90 burn damage)
            }
        }
        Destroy(gameObject);
    }
}
