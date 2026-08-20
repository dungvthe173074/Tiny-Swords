using UnityEngine;

public class CastleHealthBar : MonoBehaviour
{
    [Header("Bar Settings")]
    public Vector3 offset = new Vector3(0f, 1.25f, 0f);
    public Vector2 barSize = new Vector2(1.5f, 0.16f);

    private GameObject barRoot;
    private SpriteRenderer fillRenderer;
    private static Sprite solidSprite;

    private void Awake()
    {
        CreateCastleHealthBar();
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBaseHealthChanged += UpdateHealth;
            UpdateHealth(GameManager.Instance.currentBaseHealth, GameManager.Instance.maxBaseHealth);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBaseHealthChanged -= UpdateHealth;
        }
    }

    private void CreateCastleHealthBar()
    {
        if (barRoot != null) return;

        if (solidSprite == null)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            solidSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }

        barRoot = new GameObject("Castle_HealthBar_Root");
        barRoot.transform.SetParent(transform);
        barRoot.transform.localPosition = offset;
        barRoot.transform.localScale = Vector3.one;

        // Outer Dark Border
        GameObject borderObj = new GameObject("Border");
        borderObj.transform.SetParent(barRoot.transform);
        borderObj.transform.localPosition = Vector3.zero;
        borderObj.transform.localScale = new Vector3(barSize.x + 0.08f, barSize.y + 0.08f, 1f);
        SpriteRenderer borderSr = borderObj.AddComponent<SpriteRenderer>();
        borderSr.sprite = solidSprite;
        borderSr.color = new Color(0.1f, 0.12f, 0.16f, 0.95f);
        borderSr.sortingOrder = 18;

        // Dark-Red Underlay
        GameObject underlayObj = new GameObject("Underlay");
        underlayObj.transform.SetParent(barRoot.transform);
        underlayObj.transform.localPosition = Vector3.zero;
        underlayObj.transform.localScale = new Vector3(barSize.x, barSize.y, 1f);
        SpriteRenderer underlaySr = underlayObj.AddComponent<SpriteRenderer>();
        underlaySr.sprite = solidSprite;
        underlaySr.color = new Color(0.45f, 0.1f, 0.1f, 0.95f);
        underlaySr.sortingOrder = 19;

        // Fill Container
        GameObject fillObj = new GameObject("Fill_Container");
        fillObj.transform.SetParent(barRoot.transform);
        fillObj.transform.localPosition = new Vector3(-barSize.x * 0.5f, 0f, 0f);
        fillObj.transform.localScale = new Vector3(barSize.x, barSize.y, 1f);

        GameObject fillQuad = new GameObject("Fill_Quad");
        fillQuad.transform.SetParent(fillObj.transform);
        fillQuad.transform.localPosition = new Vector3(0.5f, 0f, 0f);
        fillQuad.transform.localScale = Vector3.one;

        fillRenderer = fillQuad.AddComponent<SpriteRenderer>();
        fillRenderer.sprite = solidSprite;
        fillRenderer.color = new Color(0.2f, 0.95f, 0.35f, 1f);
        fillRenderer.sortingOrder = 20;
    }

    public void UpdateHealth(int current, int max)
    {
        if (fillRenderer == null || barRoot == null) return;

        float pct = Mathf.Clamp01(max > 0 ? (float)current / max : 0f);

        Transform fillObj = barRoot.transform.Find("Fill_Container");
        if (fillObj != null)
        {
            fillObj.localScale = new Vector3(barSize.x * pct, barSize.y, 1f);
        }

        // Dynamic Color
        if (pct > 0.5f)
        {
            fillRenderer.color = new Color(0.2f, 0.95f, 0.35f, 1f); // Green
        }
        else if (pct > 0.25f)
        {
            fillRenderer.color = new Color(1.0f, 0.85f, 0.15f, 1f); // Yellow
        }
        else
        {
            fillRenderer.color = new Color(1.0f, 0.2f, 0.2f, 1f);   // Red
        }
    }
}
