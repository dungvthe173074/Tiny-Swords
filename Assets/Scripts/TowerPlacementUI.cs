using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class TowerPlacementUI : MonoBehaviour
{
    public static TowerPlacementUI Instance { get; private set; }

    [Header("End Game Textures (Auto-assigned or Loaded)")]
    public Texture2D gameOverTexture;
    public Texture2D winTexture;
    public Texture2D playButtonTexture;

    private GUIStyle panelStyle;
    private GUIStyle goldBadgeStyle;
    private GUIStyle towerBtnStyle;
    private GUIStyle towerBtnActiveStyle;
    private GUIStyle towerBtnDisabledStyle;
    private GUIStyle cancelBtnStyle;
    private GUIStyle tooltipStyle;
    private GUIStyle dayNightTextStyle;
    private GUIStyle dayNightTimerStyle;
    private GUIStyle endSubtitleStyle;
    private GUIStyle gameOverTitleStyle;
    private GUIStyle gameOverBtnStyle;

    private Texture2D mainBarTex;
    private Texture2D goldBadgeTex;
    private Texture2D dayBadgeTex;
    private Texture2D nightBadgeTex;
    private Texture2D btnNormalTex;
    private Texture2D btnHoverTex;
    private Texture2D btnActiveTex;
    private Texture2D btnDisabledTex;
    private Texture2D btnCancelTex;
    private Texture2D tooltipTex;
    private Texture2D dimBackdropTex;

    private int hoveredTowerIndex = -1;
    private bool isPaused = false;
    private static readonly List<Rect> activeUIRects = new List<Rect>();

    private void Awake()
    {
        Instance = this;
        GenerateTextures();
        LoadEndGameTextures();
    }

    public static bool IsMouseOverAnyUI()
    {
        Vector2 mouseGuiPos = InputHelper.MouseGUIPosition;

        for (int i = 0; i < activeUIRects.Count; i++)
        {
            if (activeUIRects[i].Contains(mouseGuiPos)) return true;
        }
        return false;
    }

    private void LoadEndGameTextures()
    {
        if (gameOverTexture == null)
            gameOverTexture = LoadTextureFromFile("Assets/Sprites/End Game/GameOver.png");

        if (winTexture == null)
            winTexture = LoadTextureFromFile("Assets/Sprites/End Game/YouWin.png");

        if (playButtonTexture == null)
            playButtonTexture = LoadTextureFromFile("Assets/Sprites/End Game/PlayButton.png");
    }

    private Texture2D LoadTextureFromFile(string relativePath)
    {
        string fullPath = Path.Combine(Application.dataPath, relativePath.Replace("Assets/", ""));
        if (File.Exists(fullPath))
        {
            byte[] fileData = File.ReadAllBytes(fullPath);
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            if (tex.LoadImage(fileData))
            {
                return tex;
            }
        }
        return null;
    }

    private void GenerateTextures()
    {
        // 1. Sleek Ultra-Compact Top Bar (Sharp Border)
        mainBarTex = CreateSharpBorderTex(64, 64, 
            new Color(0.06f, 0.09f, 0.14f, 0.96f), 
            new Color(0.26f, 0.38f, 0.52f, 1f));

        // 2. Gold Badge
        goldBadgeTex = CreateSharpBorderTex(64, 64, 
            new Color(0.12f, 0.09f, 0.03f, 0.98f), 
            new Color(0.95f, 0.75f, 0.18f, 1f));

        // 3. Tower Buttons Normal
        btnNormalTex = CreateSharpGradientTex(64, 64,
            new Color(0.18f, 0.25f, 0.36f, 0.98f),
            new Color(0.11f, 0.15f, 0.22f, 0.98f),
            new Color(0.40f, 0.52f, 0.70f, 1f));

        // 4. Tower Buttons Hover
        btnHoverTex = CreateSharpGradientTex(64, 64,
            new Color(0.26f, 0.38f, 0.52f, 1f),
            new Color(0.16f, 0.24f, 0.34f, 1f),
            new Color(0.65f, 0.82f, 1f, 1f));

        // 5. Tower Buttons Active (Glowing Emerald)
        btnActiveTex = CreateSharpGradientTex(64, 64,
            new Color(0.18f, 0.72f, 0.35f, 1f),
            new Color(0.10f, 0.46f, 0.20f, 1f),
            new Color(0.60f, 1f, 0.75f, 1f));

        // 6. Tower Buttons Disabled
        btnDisabledTex = CreateSharpGradientTex(64, 64,
            new Color(0.10f, 0.12f, 0.16f, 0.90f),
            new Color(0.06f, 0.08f, 0.10f, 0.90f),
            new Color(0.22f, 0.26f, 0.32f, 0.7f));

        // 7. Cancel Button
        btnCancelTex = CreateSharpGradientTex(64, 64,
            new Color(0.78f, 0.20f, 0.20f, 1f),
            new Color(0.48f, 0.10f, 0.10f, 1f),
            new Color(1f, 0.50f, 0.50f, 1f));

        // 8. Tooltip Panel
        tooltipTex = CreateSharpBorderTex(64, 64,
            new Color(0.05f, 0.07f, 0.10f, 0.96f),
            new Color(0.35f, 0.48f, 0.65f, 0.95f));

        // 9. Day Badge (Warm Amber/Sunlight border)
        dayBadgeTex = CreateSharpBorderTex(64, 64,
            new Color(0.12f, 0.10f, 0.04f, 0.95f),
            new Color(0.95f, 0.75f, 0.20f, 1f));

        // 10. Night Badge (Deep Midnight Blue/Violet border)
        nightBadgeTex = CreateSharpBorderTex(64, 64,
            new Color(0.04f, 0.06f, 0.14f, 0.95f),
            new Color(0.35f, 0.55f, 0.95f, 1f));

        // 11. Fullscreen Dim Backdrop
        dimBackdropTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        dimBackdropTex.SetPixel(0, 0, new Color(0.02f, 0.04f, 0.08f, 0.85f));
        dimBackdropTex.Apply();
    }

    private void OnGUI()
    {
        InitStyles();
        activeUIRects.Clear();

        TowerPlacementManager pm = TowerPlacementManager.Instance;
        GameManager gm = GameManager.Instance;
        if (pm == null) return;

        hoveredTowerIndex = -1;
        Vector2 mousePos = Event.current.mousePosition;

        // 1. TOP-LEFT COMPACT BAR (Only when game is active)
        if (gm == null || !gm.IsGameEnded)
        {
            float startX = 12f;
            float startY = 10f;
            float barH = 38f;

            float goldW = 88f;
            float btnW = 86f;
            float btnH = 28f;
            float spacing = 5f;
            int count = pm.availableTowers.Count;
            float cancelW = pm.HasSelectedTower ? 42f : 0f;

            float totalBarW = goldW + count * (btnW + spacing) + cancelW + 14f;
            Rect mainBarRect = new Rect(startX, startY, totalBarW, barH);
            activeUIRects.Add(mainBarRect);

            // Background Bar
            GUI.Box(mainBarRect, "", panelStyle);

            // Gold Badge
            float goldX = startX + 5f;
            float goldY = startY + 5f;
            float badgeH = barH - 10f;
            GUI.Box(new Rect(goldX, goldY, goldW, badgeH), "", new GUIStyle { normal = { background = goldBadgeTex } });
            GUI.Label(new Rect(goldX, goldY + 1, goldW, badgeH), $"🪙 <color=#FFD700><b>{pm.playerGold}</b></color> <color=#FFEE88><b>G</b></color>", goldBadgeStyle);

            // Tower Action Buttons
            float curBtnX = startX + goldW + 9f;
            float curBtnY = startY + (barH - btnH) * 0.5f;

            for (int i = 0; i < count; i++)
            {
                Tower tower = pm.availableTowers[i];
                if (tower == null) continue;

                Rect btnRect = new Rect(curBtnX, curBtnY, btnW, btnH);
                if (btnRect.Contains(mousePos))
                {
                    hoveredTowerIndex = i;
                }

                bool isSelected = pm.SelectedTowerIndex == i;
                bool canAfford = pm.playerGold >= tower.cost;

                GUIStyle curStyle = isSelected ? towerBtnActiveStyle : (canAfford ? towerBtnStyle : towerBtnDisabledStyle);

                string icon = i == 0 ? "🏹" : (i == 1 ? "🧪" : "🔥");
                string shortName = i == 0 ? "Tên" : (i == 1 ? "Độc" : "Lửa");
                string priceColor = canAfford ? "#FFD700" : "#FF5555";
                string nameColor = isSelected ? "#FFFFFF" : "#E8F0FF";

                string btnContent = $"{icon} <color={nameColor}><b>{shortName}</b></color> <color={priceColor}><b>{tower.cost}G</b></color>";

                if (GUI.Button(btnRect, btnContent, curStyle))
                {
                    pm.SelectTower(i);
                }

                curBtnX += btnW + spacing;
            }

            // Cancel Button (✕)
            if (pm.HasSelectedTower)
            {
                Rect cancelRect = new Rect(curBtnX, curBtnY, 36f, btnH);
                if (GUI.Button(cancelRect, "✕", cancelBtnStyle))
                {
                    pm.DeselectTower();
                }
                curBtnX += 40f;
            }

            // 2. ULTRA-SLIM 1-LINE MICRO TOOLTIP
            int activeInfoIndex = pm.HasSelectedTower ? pm.SelectedTowerIndex : hoveredTowerIndex;
            if (activeInfoIndex >= 0 && activeInfoIndex < pm.availableTowers.Count)
            {
                Tower t = pm.availableTowers[activeInfoIndex];
                if (t != null)
                {
                    float tipW = 290f;
                    float tipH = 22f;
                    float tipY = startY + barH + 3f;
                    Rect tipRect = new Rect(startX, tipY, tipW, tipH);
                    activeUIRects.Add(tipRect);

                    GUI.Box(tipRect, "", new GUIStyle { normal = { background = tooltipTex } });

                    string icon = activeInfoIndex == 0 ? "🏹" : (activeInfoIndex == 1 ? "🧪" : "🔥");
                    string tipContent = $"{icon} <b>{t.towerName}</b>  •  ⚔️ <b>{t.damage}</b>  •  🎯 <b>{t.attackRange:F1}</b>  •  ⚡ <b>{t.fireRate:F1}/s</b>";

                    GUI.Label(new Rect(startX + 8, tipY + 1, tipW - 16, tipH), tipContent, tooltipStyle);
                }
            }

            // 2.5 BOSS HEALTH BAR (PLACED DIRECTLY NEXT TO TOWER PLACEMENT BAR)
            if (BossOrc.ActiveBoss != null)
            {
                BossOrc boss = BossOrc.ActiveBoss;
                Enemy bossEnemy = boss.GetComponent<Enemy>();
                if (bossEnemy != null && bossEnemy.maxHealth > 0f)
                {
                    float bossBarW = 280f;
                    float bossBarH = barH;
                    float bossBarX = startX + totalBarW + 10f;
                    float bossBarY = startY;

                    // Ensure it does not exceed screen width
                    if (bossBarX + bossBarW > Screen.width - 12f)
                    {
                        bossBarW = Screen.width - bossBarX - 12f;
                    }

                    Rect bossRect = new Rect(bossBarX, bossBarY, bossBarW, bossBarH);
                    activeUIRects.Add(bossRect);

                    GUI.Box(bossRect, "", panelStyle);

                    // Title and Enrage tag (Single Line)
                    string enrageTag = boss.isEnraged ? " <color=#FF3333><b>[🔥 CUỒNG NỘ]</b></color>" : "";
                    string title = $"👹 <b>BOSS</b>{enrageTag}";
                    GUI.Label(new Rect(bossBarX + 8f, bossBarY + 3f, 150f, 18f), title, new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        fontSize = 11,
                        fontStyle = FontStyle.Bold,
                        richText = true,
                        normal = { textColor = boss.isEnraged ? new Color(1f, 0.4f, 0.4f, 1f) : new Color(1f, 0.85f, 0.4f, 1f) }
                    });

                    // HP numerical text
                    float hpRatio = Mathf.Clamp01(bossEnemy.CurrentHealth / bossEnemy.maxHealth);
                    string hpText = $"{Mathf.CeilToInt(bossEnemy.CurrentHealth)} / {Mathf.CeilToInt(bossEnemy.maxHealth)}";
                    GUI.Label(new Rect(bossBarX + bossBarW - 130f, bossBarY + 3f, 122f, 18f), hpText, new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleRight,
                        fontSize = 10,
                        fontStyle = FontStyle.Bold,
                        normal = { textColor = new Color(0.9f, 0.9f, 0.9f, 0.95f) }
                    });

                    // Health Bar Track & Fill
                    float innerW = bossBarW - 16f;
                    float innerH = 14f;
                    float innerX = bossBarX + 8f;
                    float innerY = bossBarY + 22f;

                    GUI.DrawTexture(new Rect(innerX, innerY, innerW, innerH), Texture2D.blackTexture);

                    Color hpColor = boss.isEnraged ? new Color(1f, 0.2f, 0.2f, 1f) : new Color(0.95f, 0.25f, 0.2f, 1f);
                    GUI.color = hpColor;
                    GUI.DrawTexture(new Rect(innerX + 1, innerY + 1, (innerW - 2) * hpRatio, innerH - 2), Texture2D.whiteTexture);
                    GUI.color = Color.white;
                }
            }
        }

        // 3. END GAME OVERLAY (BIGGER & PERFECTLY CENTERED)
        if (gm != null && gm.IsGameEnded)
        {
            // Fullscreen backdrop
            Rect fullScreenRect = new Rect(0, 0, Screen.width, Screen.height);
            activeUIRects.Add(fullScreenRect);
            GUI.DrawTexture(fullScreenRect, dimBackdropTex);

            float centerX = Screen.width * 0.5f;
            float centerY = Screen.height * 0.5f;

            if (gm.IsVictory)
            {
                // MASSIVE YOU WIN BANNER
                float winW = 680f;
                float winH = 320f;
                float winX = centerX - winW * 0.5f;
                float winY = centerY - winH * 0.5f - 55f;

                if (winTexture != null)
                {
                    GUI.DrawTexture(new Rect(winX, winY, winW, winH), winTexture, ScaleMode.ScaleToFit);
                }
                else
                {
                    GUI.Label(new Rect(centerX - 250, winY, 500, 80), "🏆 <color=#55FF88><b>YOU WIN!</b></color>", endSubtitleStyle);
                }
            }
            else if (gm.IsGameOver)
            {
                // MASSIVE GAME OVER BANNER
                float goW = 600f;
                float goH = 220f;
                float goX = centerX - goW * 0.5f;
                float goY = centerY - goH * 0.5f - 55f;

                if (gameOverTexture != null)
                {
                    GUI.DrawTexture(new Rect(goX, goY, goW, goH), gameOverTexture, ScaleMode.ScaleToFit);
                }
                else
                {
                    GUI.Label(new Rect(centerX - 250, goY, 500, 80), "💀 <color=#FF4444><b>GAME OVER</b></color>", endSubtitleStyle);
                }
            }

            // END GAME ACTION BUTTONS
            float actionBtnW = 180f;
            float actionBtnH = 46f;
            float actionSpacing = 14f;

            // Restart Button
            Rect restartRect = new Rect(centerX - actionBtnW - actionSpacing * 0.5f, centerY + 80f, actionBtnW, actionBtnH);
            activeUIRects.Add(restartRect);
            if (GUI.Button(restartRect, "🔄  <b>CHƠI LẠI</b>", gameOverBtnStyle))
            {
                gm.RestartGame();
            }

            // Return to Main Menu Button
            Rect menuBtnRect = new Rect(centerX + actionSpacing * 0.5f, centerY + 80f, actionBtnW, actionBtnH);
            activeUIRects.Add(menuBtnRect);
            if (GUI.Button(menuBtnRect, "🏠  <b>MENU CHÍNH</b>", towerBtnStyle))
            {
                Time.timeScale = 1f;
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
        }

        // 4. IN-GAME PAUSE MENU (ESC ONLY)
        if (isPaused && gm != null && !gm.IsGameEnded)
        {
            Rect fullScreenRect = new Rect(0, 0, Screen.width, Screen.height);
            activeUIRects.Add(fullScreenRect);
            GUI.DrawTexture(fullScreenRect, dimBackdropTex);

            float centerX = Screen.width * 0.5f;
            float centerY = Screen.height * 0.5f;

            float pauseW = 400f;
            float pauseH = 300f;
            Rect pauseBox = new Rect(centerX - pauseW * 0.5f, centerY - pauseH * 0.5f, pauseW, pauseH);
            activeUIRects.Add(pauseBox);

            GUI.Box(pauseBox, "", panelStyle);

            GUI.Label(new Rect(pauseBox.x + 10f, pauseBox.y + 22f, pauseW - 20f, 32f), "<b>TẠM DỪNG TRẬN ĐẤU</b>", new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                richText = true,
                normal = { textColor = new Color(1f, 0.45f, 0.45f, 1f) }
            });

            float btnW = 290f;
            float btnH = 46f;
            float startY = pauseBox.y + 74f;
            float spacing = 12f;

            // Resume
            Rect resumeRect = new Rect(centerX - btnW * 0.5f, startY, btnW, btnH);
            activeUIRects.Add(resumeRect);
            if (GUI.Button(resumeRect, "▶  <b>TIẾP TỤC CHƠI</b>", gameOverBtnStyle))
            {
                TogglePause();
            }

            // Restart
            Rect restartPauseRect = new Rect(centerX - btnW * 0.5f, startY + (btnH + spacing), btnW, btnH);
            activeUIRects.Add(restartPauseRect);
            if (GUI.Button(restartPauseRect, "🔄  <b>CHƠI LẠI TỪ ĐẦU</b>", towerBtnStyle))
            {
                Time.timeScale = 1f;
                gm.RestartGame();
            }

            // Main Menu
            Rect menuPauseRect = new Rect(centerX - btnW * 0.5f, startY + (btnH + spacing) * 2, btnW, btnH);
            activeUIRects.Add(menuPauseRect);
            if (GUI.Button(menuPauseRect, "🏠  <b>VỀ MENU CHÍNH</b>", cancelBtnStyle))
            {
                Time.timeScale = 1f;
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
        }
    }

    private void Update()
    {
        if (InputHelper.GetEscapeDown())
        {
            TowerPlacementManager pm = TowerPlacementManager.Instance;
            if (pm != null && pm.HasSelectedTower)
            {
                pm.DeselectTower();
            }
            else
            {
                GameManager gm = GameManager.Instance;
                if (gm != null && !gm.IsGameEnded)
                {
                    TogglePause();
                }
            }
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
    }

    private void InitStyles()
    {
        if (goldBadgeStyle == null)
        {
            goldBadgeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                richText = true
            };
            goldBadgeStyle.normal.textColor = Color.white;
        }

        if (panelStyle == null)
        {
            panelStyle = new GUIStyle(GUI.skin.box);
            panelStyle.normal.background = mainBarTex;
        }

        if (towerBtnStyle == null)
        {
            towerBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                richText = true,
                padding = new RectOffset(2, 2, 0, 0)
            };
            towerBtnStyle.normal.background = btnNormalTex;
            towerBtnStyle.hover.background = btnHoverTex;
            towerBtnStyle.normal.textColor = Color.white;
            towerBtnStyle.hover.textColor = Color.white;
        }

        if (towerBtnActiveStyle == null)
        {
            towerBtnActiveStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                richText = true,
                padding = new RectOffset(2, 2, 0, 0)
            };
            towerBtnActiveStyle.normal.background = btnActiveTex;
            towerBtnActiveStyle.hover.background = btnActiveTex;
            towerBtnActiveStyle.normal.textColor = Color.white;
        }

        if (towerBtnDisabledStyle == null)
        {
            towerBtnDisabledStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                richText = true,
                padding = new RectOffset(2, 2, 0, 0)
            };
            towerBtnDisabledStyle.normal.background = btnDisabledTex;
            towerBtnDisabledStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);
        }

        if (cancelBtnStyle == null)
        {
            cancelBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                richText = true,
                padding = new RectOffset(1, 1, 0, 0)
            };
            cancelBtnStyle.normal.background = btnCancelTex;
            cancelBtnStyle.normal.textColor = Color.white;
        }

        if (tooltipStyle == null)
        {
            tooltipStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                richText = true
            };
            tooltipStyle.normal.textColor = new Color(0.92f, 0.96f, 1f, 1f);
        }

        if (dayNightTextStyle == null)
        {
            dayNightTextStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                richText = true
            };
            dayNightTextStyle.normal.textColor = Color.white;
        }

        if (dayNightTimerStyle == null)
        {
            dayNightTimerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight,
                richText = true
            };
            dayNightTimerStyle.normal.textColor = Color.white;
        }

        if (endSubtitleStyle == null)
        {
            endSubtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                richText = true
            };
            endSubtitleStyle.normal.textColor = Color.white;
        }

        if (gameOverTitleStyle == null)
        {
            gameOverTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                richText = true
            };
            gameOverTitleStyle.normal.textColor = new Color(1f, 0.35f, 0.35f, 1f);
        }

        if (gameOverBtnStyle == null)
        {
            gameOverBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                richText = true
            };
            gameOverBtnStyle.normal.background = btnActiveTex;
            gameOverBtnStyle.normal.textColor = Color.white;
        }
    }

    private Texture2D CreateSharpBorderTex(int w, int h, Color fill, Color border)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] cols = new Color[w * h];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (x == 0 || x == w - 1 || y == 0 || y == h - 1)
                {
                    cols[y * w + x] = border;
                }
                else
                {
                    cols[y * w + x] = fill;
                }
            }
        }
        tex.SetPixels(cols);
        tex.Apply();
        return tex;
    }

    private Texture2D CreateSharpGradientTex(int w, int h, Color topFill, Color botFill, Color border)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] cols = new Color[w * h];

        for (int y = 0; y < h; y++)
        {
            float t = (float)y / h;
            Color curFill = Color.Lerp(botFill, topFill, t);

            for (int x = 0; x < w; x++)
            {
                if (x == 0 || x == w - 1 || y == 0 || y == h - 1)
                {
                    cols[y * w + x] = border;
                }
                else
                {
                    if (y >= h - 2) curFill = Color.Lerp(curFill, Color.white, 0.3f);
                    cols[y * w + x] = curFill;
                }
            }
        }
        tex.SetPixels(cols);
        tex.Apply();
        return tex;
    }
}
