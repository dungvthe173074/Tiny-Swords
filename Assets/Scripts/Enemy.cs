using System;
using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
public class Enemy : MonoBehaviour
{
    [Header("Base Enemy Stats")]
    public float baseMaxHealth = 150f;
    public float baseMoveSpeed = 3f;
    public int baseGoldReward = 15;
    public int baseCastleDamage = 1;

    [Header("Current Runtime Stats")]
    public float maxHealth = 150f;
    public float moveSpeed = 3f;
    public int goldReward = 15;
    public int castleDamage = 1;

    public float CurrentHealth { get; private set; }
    public bool IsBuffed { get; private set; } = false;

    public event Action<float, float> OnHealthChanged;

    private EnemyMovement enemyMovement;
    private SpriteRenderer spriteRenderer;
    private Action<Enemy> onDespawnCallback;
    private bool isDespawned = false;
    private Color originalSpriteColor = Color.white;

    private void Awake()
    {
        enemyMovement = GetComponent<EnemyMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalSpriteColor = spriteRenderer.color;
        }

        if (GetComponent<EnemyHealthBar>() == null)
        {
            gameObject.AddComponent<EnemyHealthBar>();
        }

        // Initialize base stats if not explicitly set
        if (baseMaxHealth <= 0f) baseMaxHealth = maxHealth > 0f ? maxHealth : 150f;
        if (baseMoveSpeed <= 0f) baseMoveSpeed = moveSpeed > 0f ? moveSpeed : 3f;
        if (baseGoldReward <= 0) baseGoldReward = goldReward > 0 ? goldReward : 15;
    }

    private void OnEnable()
    {
        if (DayNightCycle.Instance != null)
        {
            DayNightCycle.Instance.OnDayNightChanged += HandleDayNightChanged;
            DayNightCycle.Instance.OnDarknessChanged += HandleDarknessChanged;
        }
        ApplyDayNightBuff(DayNightCycle.Instance != null && DayNightCycle.Instance.IsNight, 
            DayNightCycle.Instance != null ? DayNightCycle.Instance.DarknessFactor : 0f);
    }

    private void OnDisable()
    {
        if (DayNightCycle.Instance != null)
        {
            DayNightCycle.Instance.OnDayNightChanged -= HandleDayNightChanged;
            DayNightCycle.Instance.OnDarknessChanged -= HandleDarknessChanged;
        }
    }

    /// <summary>
    /// Initialize or reset the enemy when spawned from the object pool.
    /// </summary>
    public void Initialize(Transform[] waypoints, Action<Enemy> onDespawn)
    {
        maxHealth = baseMaxHealth;
        CurrentHealth = maxHealth;
        goldReward = baseGoldReward;
        isDespawned = false;
        onDespawnCallback = onDespawn;

        // Reset status effects
        isSlowed = false;
        slowTimer = 0f;
        slowMultiplier = 1f;
        isBurning = false;
        burnTimer = 0f;
        burnDps = 0f;
        burnTickTimer = 0f;

        ApplyDayNightBuff(DayNightCycle.Instance != null && DayNightCycle.Instance.IsNight,
            DayNightCycle.Instance != null ? DayNightCycle.Instance.DarknessFactor : 0f);

        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (enemyMovement != null)
        {
            enemyMovement.Initialize(this, waypoints);
        }
    }

    private void HandleDayNightChanged(bool isNight)
    {
        IsBuffed = isNight;
        if (DayNightCycle.Instance != null)
        {
            castleDamage = DayNightCycle.Instance.CurrentCastleDamage;
        }
    }

    [Header("Status Effects (Runtime)")]
    public bool isSlowed = false;
    public float slowTimer = 0f;
    public float slowMultiplier = 1f;

    public bool isBurning = false;
    public float burnTimer = 0f;
    public float burnDps = 0f;
    private float burnTickTimer = 0f;

    private void Update()
    {
        if (isDespawned) return;

        UpdateStatusEffects();
    }

    private void UpdateStatusEffects()
    {
        // 1. Slow Timer Countdown
        if (slowTimer > 0f)
        {
            slowTimer -= Time.deltaTime;
            isSlowed = true;
            if (slowTimer <= 0f)
            {
                isSlowed = false;
                slowMultiplier = 1f;
                RecalculateSpeed();
            }
        }

        // 2. Burn DoT (Damage over Time)
        if (burnTimer > 0f)
        {
            burnTimer -= Time.deltaTime;
            isBurning = true;
            burnTickTimer += Time.deltaTime;

            if (burnTickTimer >= 0.5f)
            {
                burnTickTimer -= 0.5f;
                TakeDamage(burnDps * 0.5f);
            }

            if (burnTimer <= 0f)
            {
                isBurning = false;
                burnTickTimer = 0f;
            }
        }

        // 3. Status Tint
        UpdateSpriteColor();
    }

    private void UpdateSpriteColor()
    {
        if (spriteRenderer == null) return;

        Color targetColor = originalSpriteColor;

        if (isBurning)
        {
            targetColor = new Color(1.0f, 0.45f, 0.15f, 1f); // Fiery orange
        }
        else if (isSlowed)
        {
            targetColor = new Color(0.35f, 1.0f, 0.45f, 1f); // Toxic venom green
        }
        else if (DayNightCycle.Instance != null && DayNightCycle.Instance.DarknessFactor > 0.05f)
        {
            Color buffTint = new Color(1.0f, 0.38f, 0.38f, 1.0f);
            targetColor = Color.Lerp(originalSpriteColor, buffTint, DayNightCycle.Instance.DarknessFactor);
        }

        spriteRenderer.color = targetColor;
    }

    /// <summary>
    /// Apply Slow from Poison Tower.
    /// </summary>
    public void ApplySlow(float speedMult, float duration)
    {
        if (isDespawned) return;

        slowMultiplier = Mathf.Min(slowMultiplier, speedMult);
        slowTimer = Mathf.Max(slowTimer, duration);
        isSlowed = true;
        RecalculateSpeed();
    }

    /// <summary>
    /// Apply Burn DoT from Fire Tower.
    /// </summary>
    public void ApplyBurn(float dps, float duration)
    {
        if (isDespawned) return;

        burnDps = Mathf.Max(burnDps, dps);
        burnTimer = Mathf.Max(burnTimer, duration);
        isBurning = true;
    }

    private void RecalculateSpeed()
    {
        float nightMultiplier = DayNightCycle.Instance != null ? DayNightCycle.Instance.CurrentSpeedMultiplier : 1f;
        moveSpeed = baseMoveSpeed * nightMultiplier * (isSlowed ? slowMultiplier : 1f);
    }

    private void HandleDarknessChanged(float darknessFactor)
    {
        RecalculateSpeed();
        if (DayNightCycle.Instance != null)
        {
            castleDamage = DayNightCycle.Instance.CurrentCastleDamage;
        }
        UpdateSpriteColor();
    }

    /// <summary>
    /// Apply Night Buff or Day Debuff smoothly.
    /// </summary>
    public void ApplyDayNightBuff(bool isNight, float darkness)
    {
        IsBuffed = isNight;
        HandleDarknessChanged(darkness);
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
        Debug.Log($"[Enemy] {gameObject.name} reached the goal! Dealing {castleDamage} damage.");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.TakeBaseDamage(castleDamage);
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

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalSpriteColor;
        }

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
