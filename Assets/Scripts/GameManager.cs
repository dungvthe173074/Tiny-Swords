using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Base / Castle Health (Máu Nhà Chính)")]
    public int maxBaseHealth = 10;
    public int currentBaseHealth = 10;

    public bool IsGameOver { get; private set; } = false;
    public bool IsVictory { get; private set; } = false;

    public bool IsGameEnded => IsGameOver || IsVictory;

    public event Action<int, int> OnBaseHealthChanged;
    public event Action OnGameOver;
    public event Action OnVictory;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        currentBaseHealth = maxBaseHealth;
    }

    public void TakeBaseDamage(int damage)
    {
        if (IsGameEnded) return;

        currentBaseHealth = Mathf.Max(0, currentBaseHealth - damage);
        OnBaseHealthChanged?.Invoke(currentBaseHealth, maxBaseHealth);

        Debug.Log($"[GameManager] Nhà chính bị tấn công! Máu còn lại: {currentBaseHealth}/{maxBaseHealth}");

        if (currentBaseHealth <= 0)
        {
            GameOver();
        }
    }

    public void Victory()
    {
        if (IsGameEnded) return;
        IsVictory = true;
        Debug.Log("[GameManager] 🏆 VICTORY! Bạn đã bảo vệ thành công Nhà Chính và vượt qua tất cả các đợt quái!");
        OnVictory?.Invoke();
    }

    private void GameOver()
    {
        if (IsGameEnded) return;
        IsGameOver = true;
        Debug.Log("[GameManager] 💀 GAME OVER! Nhà chính đã bị phá hủy.");
        OnGameOver?.Invoke();
    }

    public void RestartGame()
    {
        IsGameOver = false;
        IsVictory = false;
        currentBaseHealth = maxBaseHealth;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
