using System;
using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float maxHealth = 150f;
    public float moveSpeed = 3f;
    public int goldReward = 15;

    public float CurrentHealth { get; private set; }

    public event Action<float, float> OnHealthChanged;

    private EnemyMovement enemyMovement;
    private Action<Enemy> onDespawnCallback;
    private bool isDespawned = false;

    private void Awake()
    {
        enemyMovement = GetComponent<EnemyMovement>();
        if (GetComponent<EnemyHealthBar>() == null)
        {
            gameObject.AddComponent<EnemyHealthBar>();
        }
    }

    /// <summary>
    /// Initialize or reset the enemy when spawned from the object pool.
    /// </summary>
    public void Initialize(Transform[] waypoints, Action<Enemy> onDespawn)
    {
        CurrentHealth = maxHealth;
        isDespawned = false;
        onDespawnCallback = onDespawn;

        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (enemyMovement != null)
        {
            enemyMovement.Initialize(this, waypoints);
        }
    }

    /// <summary>
    /// Apply damage to the enemy.
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (isDespawned) return;

        CurrentHealth -= amount;
        if (CurrentHealth <= 0f)
        {
            CurrentHealth = 0f;
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
            Die();
        }
        else
        {
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }
    }

    /// <summary>
    /// Enemy died by player / tower damage.
    /// </summary>
    public void Die()
    {
        if (isDespawned) return;
        Debug.Log($"[Enemy] {gameObject.name} was defeated!");

        if (TowerPlacementManager.Instance != null)
        {
            TowerPlacementManager.Instance.AddGold(goldReward);
        }

        Despawn();
    }

    /// <summary>
    /// Enemy reached the final destination.
    /// </summary>
    public void ReachGoal()
    {
        if (isDespawned) return;
        Debug.Log($"[Enemy] {gameObject.name} reached the goal!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.TakeBaseDamage(1);
        }

        Despawn();
    }

    /// <summary>
    /// Return the enemy back to the object pool.
    /// </summary>
    private void Despawn()
    {
        if (isDespawned) return;
        isDespawned = true;

        Action<Enemy> callback = onDespawnCallback;
        onDespawnCallback = null;
        callback?.Invoke(this);

        if (EnemyObjectPool.Instance != null)
        {
            EnemyObjectPool.Instance.ReturnEnemy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
