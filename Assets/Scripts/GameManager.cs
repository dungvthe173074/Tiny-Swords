using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Base / Castle Health (Máu Nhà Chính)")]
    public int maxBaseHealth = 15;
    public int currentBaseHealth = 15;

    public bool IsGameOver { get; private set; } = false;
    public bool IsVictory { get; private set; } = false;
    public string GameOverReason { get; private set; } = "";

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
        GameOverReason = "";
    }

    public void TakeBaseDamage(int damage)
    {
        if (IsGameEnded) return;

        currentBaseHealth = Mathf.Max(0, currentBaseHealth - damage);
        OnBaseHealthChanged?.Invoke(currentBaseHealth, maxBaseHealth);

        Debug.Log($"[GameManager] Nhà chính bị tấn công! Máu còn lại: {currentBaseHealth}/{maxBaseHealth}");
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("TowerDamage");
        }

        if (currentBaseHealth <= 0)
        {
            GameOver("Nhà chính đã bị phá hủy!");
        }
    }

    public void DefeatByBoss()
    {
        if (IsGameEnded) return;

        currentBaseHealth = 0;
        OnBaseHealthChanged?.Invoke(currentBaseHealth, maxBaseHealth);

        Debug.Log("[GameManager] 💀 TRÙM CUỐI (BOSS) ĐÃ XÂM NHẬP VÀO NHÀ CHÍNH! Thua trận ngay lập tức.");
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("TowerDamage");
        }

        GameOver("💀 Trùm Cuối (Boss) đã xâm nhập vào Nhà Chính!");
    }

    public void Victory()
    {
        if (IsGameEnded) return;
        IsVictory = true;
        Time.timeScale = 0f;
        Debug.Log("[GameManager] 🏆 VICTORY! Bạn đã bảo vệ thành công Nhà Chính và vượt qua tất cả các đợt quái!");
        OnVictory?.Invoke();
    }

    public void GameOver(string reason = "Nhà chính đã bị phá hủy!")
    {
        if (IsGameEnded) return;
        IsGameOver = true;
        GameOverReason = reason;
        Time.timeScale = 0f;
        Debug.Log($"[GameManager] 💀 GAME OVER! {reason}");
        OnGameOver?.Invoke();
    }

    public void RestartGame()
    {
        IsGameOver = false;
        IsVictory = false;
        GameOverReason = "";
        currentBaseHealth = maxBaseHealth;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
