using UnityEngine;

public class EnemyData : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int maxHealth = 3;
    public int goldReward = 10;

    private int currentHealth;

    private void OnEnable()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Reward gold
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(goldReward);
            Debug.Log($"Enemy killed! Added {goldReward} gold. Current Gold: {GameManager.Instance.CurrentGold}");
        }

        // Return enemy to object pool
        EnemyMovement movement = GetComponent<EnemyMovement>();
        if (movement != null)
        {
            movement.Despawn();
        }
        else
        {
            Destroy(gameObject); // Fallback
        }
    }
}