using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Economy & Base Settings")]
    public int startingGold = 100;
    public int startingBaseHealth = 20;

    public int CurrentGold { get; private set; }
    public int CurrentBaseHealth { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        CurrentGold = startingGold;
        CurrentBaseHealth = startingBaseHealth;
    }

    public void AddGold(int amount)
    {
        CurrentGold += amount;
    }

    public bool TrySpendGold(int amount)
    {
        if (CurrentGold >= amount)
        {
            CurrentGold -= amount;
            return true;
        }
        return false;
    }

    public void TakeBaseDamage(int damage)
    {
        CurrentBaseHealth = Mathf.Max(0, CurrentBaseHealth - damage);

        Debug.Log($"Base took {damage} damage! Current Base Health: {CurrentBaseHealth}");

        if (CurrentBaseHealth <= 0)
        {
            Debug.Log("Game Over!");
            Time.timeScale = 0f;
        }
    }
}