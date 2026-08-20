using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyHealthBar : MonoBehaviour
{
    [Header("Position Offset")]
    public Vector3 offset = new Vector3(0f, 0.65f, 0f);
    public Vector2 barSize = new Vector2(0.7f, 0.09f);

    private Enemy enemy;
    private GameObject barRoot;
    private SpriteRenderer bgRenderer;
    private SpriteRenderer fillRenderer;

    private static Sprite solidSprite;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        CreateHealthBar();
    }

    private void OnEnable()
    {
        if (enemy != null)
        {
            enemy.OnHealthChanged += UpdateHealth;
        }
        UpdateHealth(enemy != null ? enemy.CurrentHealth : 100f, enemy != null ? enemy.maxHealth : 100f);
    }

    private void OnDisable()
    {
        if (enemy != null)
        {
            enemy.OnHealthChanged -= UpdateHealth;
        }
    }

    private void CreateHealthBar()
    {
        if (barRoot != null) return;

        if (solidSprite == null)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            solidSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }

        // Root container for health bar
        barRoot = new GameObject("HealthBar_Root");
        barRoot.transform.SetParent(transform);
        barRoot.transform.localPosition = offset;
        barRoot.transform.localScale = Vector3.one;

        // Background / Border bar
        GameObject bgObj = new GameObject("Bar_Background");
        bgObj.transform.SetParent(barRoot.transform);
        bgObj.transform.localPosition = Vector3.zero;
        bgObj.transform.localScale = new Vector3(barSize.x + 0.04f, barSize.y + 0.04f, 1f);

        bgRenderer = bgObj.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = solidSprite;
        bgRenderer.color = new Color(0.12f, 0.12f, 0.12f, 0.9f);
        bgRenderer.sortingOrder = 15;

        // Inner Red background (underneath green fill)
        GameObject redBgObj = new GameObject("Bar_RedUnderlay");
        redBgObj.transform.SetParent(barRoot.transform);
        redBgObj.transform.localPosition = Vector3.zero;
        redBgObj.transform.localScale = new Vector3(barSize.x, barSize.y, 1f);

        SpriteRenderer redBgRenderer = redBgObj.AddComponent<SpriteRenderer>();
        redBgRenderer.sprite = solidSprite;
        redBgRenderer.color = new Color(0.6f, 0.1f, 0.1f, 0.9f);
        redBgRenderer.sortingOrder = 16;

        // Health Fill bar
        GameObject fillObj = new GameObject("Bar_Fill");
        fillObj.transform.SetParent(barRoot.transform);
        fillObj.transform.localPosition = new Vector3(-barSize.x * 0.5f, 0f, 0f);
        fillObj.transform.localScale = new Vector3(barSize.x, barSize.y, 1f);

        // Shift fill pivot by childing a centered quad
        GameObject fillQuad = new GameObject("Fill_Quad");
        fillQuad.transform.SetParent(fillObj.transform);
        fillQuad.transform.localPosition = new Vector3(0.5f, 0f, 0f);
        fillQuad.transform.localScale = Vector3.one;

        fillRenderer = fillQuad.AddComponent<SpriteRenderer>();
        fillRenderer.sprite = solidSprite;
        fillRenderer.color = new Color(0.2f, 0.95f, 0.35f, 1f);
        fillRenderer.sortingOrder = 17;
    }

    public void UpdateHealth(float current, float max)
    {
        if (fillRenderer == null || barRoot == null) return;

        float pct = Mathf.Clamp01(max > 0 ? current / max : 0f);

        Transform fillObj = barRoot.transform.Find("Bar_Fill");
        if (fillObj != null)
        {
            fillObj.localScale = new Vector3(barSize.x * pct, barSize.y, 1f);
        }

        // Dynamic health color
        if (pct > 0.5f)
        {
            fillRenderer.color = new Color(0.2f, 0.95f, 0.35f, 1f); // Green
        }
        else if (pct > 0.25f)
        {
            fillRenderer.color = new Color(1.0f, 0.85f, 0.15f, 1f); // Yellow-Orange
        }
        else
        {
            fillRenderer.color = new Color(1.0f, 0.2f, 0.2f, 1f);   // Red
        }
    }
}
