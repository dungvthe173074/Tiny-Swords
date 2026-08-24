using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyAnimation : MonoBehaviour
{
    [Header("Animation Configuration")]
    public Sprite[] runSprites;
    public float animFps = 9f;

    private SpriteRenderer spriteRenderer;
    private int currentFrame = 0;
    private float frameTimer = 0f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        currentFrame = 0;
        frameTimer = 0f;
        if (runSprites != null && runSprites.Length > 0 && spriteRenderer != null)
        {
            spriteRenderer.sprite = runSprites[0];
        }
    }

    private void Update()
    {
        if (runSprites == null || runSprites.Length <= 1 || spriteRenderer == null) return;

        frameTimer += Time.deltaTime;
        float interval = 1f / Mathf.Max(1f, animFps);

        if (frameTimer >= interval)
        {
            frameTimer -= interval;
            currentFrame = (currentFrame + 1) % runSprites.Length;
            spriteRenderer.sprite = runSprites[currentFrame];
        }
    }
}
