using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(EnemyMovement))]
public class BossOrc : MonoBehaviour
{
    public static BossOrc ActiveBoss { get; private set; }

    [Header("Boss Identity")]
    public string bossName = "ĐẠI TƯỚNG ORC (TRÙM CUỐI)";
    public bool isBoss = true;

    [Header("Animation Sprites (Auto-loaded or assigned)")]
    public Sprite[] idleSprites;
    public Sprite[] walkSprites;
    public Sprite[] attackSprites;
    public Sprite[] hurtSprites;
    public Sprite[] deathSprites;
    public float animFps = 9f;

    [Header("Visual Effects")]
    public bool isEnraged = false;

    private Enemy enemy;
    private SpriteRenderer sr;
    private EnemyMovement movement;

    private int currentAnimFrame = 0;
    private float animTimer = 0f;
    private bool isDying = false;
    private bool isAttacking = false;
    private float hurtTimer = 0f;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        sr = GetComponent<SpriteRenderer>();
        movement = GetComponent<EnemyMovement>();

        // Ensure boss stats are monstrous (Máu 10000, Đi Chậm 0.85, Sát Thương Nhà Chính 15)
        if (enemy != null)
        {
            enemy.isBoss = true;
            enemy.baseMaxHealth = 10000f;
            enemy.baseMoveSpeed = 0.85f;
            enemy.baseGoldReward = 500;
            enemy.baseCastleDamage = 15;
            enemy.maxHealth = 10000f;
            enemy.moveSpeed = 0.85f;
            enemy.goldReward = 500;
            enemy.castleDamage = 15;
        }

        transform.localScale = new Vector3(4.0f, 4.0f, 1.0f);
        if (sr != null)
        {
            sr.sortingOrder = 20; // Render in front
        }
    }

    private void OnEnable()
    {
        ActiveBoss = this;
        isDying = false;
        isAttacking = false;
        isEnraged = false;
        currentAnimFrame = 0;
        animTimer = 0f;
        hurtTimer = 0f;

        if (enemy != null)
        {
            enemy.OnHealthChanged += HandleHealthChanged;
        }

        Debug.Log($"[BossOrc] ⚠️ {bossName} HAS SPAWNED! (HP: {enemy?.maxHealth})");
    }

    private void OnDisable()
    {
        if (ActiveBoss == this)
        {
            ActiveBoss = null;
        }

        if (enemy != null)
        {
            enemy.OnHealthChanged -= HandleHealthChanged;
        }
    }

    private void Update()
    {
        if (isDying) return;

        UpdateAnimation();
    }

    private void HandleHealthChanged(float curHp, float maxHp)
    {
        float ratio = maxHp > 0 ? curHp / maxHp : 1f;

        // Enrage below 40% HP (increases speed)
        if (ratio <= 0.40f && !isEnraged)
        {
            isEnraged = true;
            if (enemy != null)
            {
                enemy.baseMoveSpeed *= 1.35f;
                enemy.moveSpeed *= 1.35f;
            }
            Debug.Log($"[BossOrc] ⚡ {bossName} IS ENRAGED! Speed increased!");
        }

        if (curHp <= 0f && !isDying)
        {
            StartCoroutine(PlayDeathSequence());
        }
        else if (hurtSprites != null && hurtSprites.Length > 0 && curHp > 0f)
        {
            hurtTimer = 0.15f;
        }
    }

    private void UpdateAnimation()
    {
        if (sr == null) return;

        // Flash hurt sprite briefly when damaged
        if (hurtTimer > 0f)
        {
            hurtTimer -= Time.deltaTime;
            if (hurtSprites != null && hurtSprites.Length > 0)
            {
                sr.sprite = hurtSprites[0];
                return;
            }
        }

        animTimer += Time.deltaTime;
        float frameInterval = 1f / Mathf.Max(1f, animFps);

        if (animTimer >= frameInterval)
        {
            animTimer -= frameInterval;

            if (isAttacking && attackSprites != null && attackSprites.Length > 0)
            {
                currentAnimFrame = (currentAnimFrame + 1) % attackSprites.Length;
                sr.sprite = attackSprites[currentAnimFrame];
            }
            else if (walkSprites != null && walkSprites.Length > 0)
            {
                currentAnimFrame = (currentAnimFrame + 1) % walkSprites.Length;
                sr.sprite = walkSprites[currentAnimFrame];
            }
            else if (idleSprites != null && idleSprites.Length > 0)
            {
                currentAnimFrame = (currentAnimFrame + 1) % idleSprites.Length;
                sr.sprite = idleSprites[currentAnimFrame];
            }
        }
    }

    private IEnumerator PlayDeathSequence()
    {
        isDying = true;

        if (deathSprites != null && deathSprites.Length > 0 && sr != null)
        {
            for (int i = 0; i < deathSprites.Length; i++)
            {
                sr.sprite = deathSprites[i];
                yield return new WaitForSeconds(0.12f);
            }
        }

        yield return new WaitForSeconds(0.2f);
    }
}
