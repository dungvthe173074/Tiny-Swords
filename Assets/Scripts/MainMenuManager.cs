using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum MenuState
{
    Main,
    MapSelect,
    Guide,
    Settings
}

public class MainMenuManager : MonoBehaviour
{
    public event Action<float> OnBGMVolumeChanged;
    public event Action<float> OnSFXVolumeChanged;
    public static MainMenuManager Instance { get; private set; }

    [Header("Current View")]
    public MenuState currentState = MenuState.Main;

    [Header("Settings State")]
    public float bgmVolume = 1.0f;
    public float sfxVolume = 1.0f;
    public bool isFullscreen = true;

    // --- PROCEDURAL SHARP TEXTURES (Crisp, High-Resolution, No Atlas Distortion) ---
    private Texture2D texLogoBanner;
    private Texture2D texLogoBackdrop;
    private Texture2D texBtnPlayNormal;
    private Texture2D texBtnPlayHover;
    private Texture2D texBtnRegularNormal;
    private Texture2D texBtnRegularHover;
    private Texture2D texBtnRedNormal;
    private Texture2D texBtnRedHover;
    private Texture2D texModalWood;
    private Texture2D texModalInner;
    private Texture2D texCardUnlocked;
    private Texture2D texCardLocked;
    private Texture2D texBadgeReady;
    private Texture2D texBadgeLocked;
    private Texture2D texDimOverlay;

    // --- GUI STYLES ---
    private GUIStyle logoTitleStyle;
    private GUIStyle logoSubtitleStyle;
    private GUIStyle btnPlayStyle;
    private GUIStyle btnRegularStyle;
    private GUIStyle btnRedStyle;
    private GUIStyle headerStyle;
    private GUIStyle bodyStyle;
    private GUIStyle cardTitleStyle;
    private GUIStyle cardDescStyle;
    private GUIStyle badgeStyle;

    private void Awake()
    {
        Instance = this;
        GenerateSharpTextures();
    }

    private void Start()
    {
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (InputHelper.GetEscapeDown())
        {
            if (currentState != MenuState.Main)
            {
                currentState = MenuState.Main;
            }
        }
    }

    private void OnGUI()
    {
        InitStyles();

        // 0. Atmospheric Dim Background
        if (texDimOverlay != null)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texDimOverlay);
        }

        switch (currentState)
        {
            case MenuState.Main:
                DrawMainMenu();
                break;
            case MenuState.MapSelect:
                DrawMapSelectMenu();
                break;
            case MenuState.Guide:
                DrawGuideModal();
                break;
            case MenuState.Settings:
                DrawSettingsModal();
                break;
        }
    }

    // =========================================================================
    // 1. MAIN MENU SCREEN (STATIC, CLEAN & BEAUTIFULLY CENTERED)
    // =========================================================================
    private void DrawMainMenu()
    {
        float centerX = Screen.width * 0.5f;
        float centerY = Screen.height * 0.5f;

        // Perfect Vertical Centering for entire menu block
        float bannerW = Mathf.Min(520f, Screen.width - 40f);
        float bannerH = 92f;
        float btnW = 280f;
        float btnH = 50f;
        float btnSpacing = 12f;

        float totalMenuH = bannerH + 24f + 4 * (btnH + btnSpacing) - btnSpacing;
        float startY = Mathf.Max(20f, centerY - totalMenuH * 0.5f);

        // --- 1.1 STATIC ROYAL LOGO BANNER (NO BOBBING) ---
        float bannerX = centerX - bannerW * 0.5f;
        float bannerY = startY;

        // Outer Royal Box
        GUI.Box(new Rect(bannerX, bannerY, bannerW, bannerH), "", new GUIStyle { normal = { background = texLogoBanner } });

        // Inner Royal Navy Backdrop
        float innerPad = 4f;
        GUI.Box(new Rect(bannerX + innerPad, bannerY + innerPad, bannerW - innerPad * 2, bannerH - innerPad * 2), "", new GUIStyle { normal = { background = texLogoBackdrop } });

        // Embossed Title: TINY SWORDS
        Rect titleShadowRect = new Rect(bannerX, bannerY + 12f, bannerW, 38f);
        GUI.Label(titleShadowRect, "TINY SWORDS", new GUIStyle(logoTitleStyle) { normal = { textColor = new Color(0.08f, 0.04f, 0.02f, 0.95f) } });

        Rect titleRect = new Rect(bannerX, bannerY + 10f, bannerW, 38f);
        GUI.Label(titleRect, "<color=#FFE066>TINY SWORDS</color>", logoTitleStyle);

        // Subtitle: TOWER DEFENSE LITE
        Rect subRect = new Rect(bannerX, bannerY + 54f, bannerW, 22f);
        GUI.Label(subRect, "TOWER DEFENSE LITE  •  ALPHA v0.3", logoSubtitleStyle);

        // --- 1.2 MAIN ACTION BUTTONS (CLEAN LABELS) ---
        float startBtnY = bannerY + bannerH + 24f;

        // Button 1: BẮT ĐẦU CHƠI (Emerald Green)
        Rect playRect = new Rect(centerX - btnW * 0.5f, startBtnY, btnW, btnH + 4f);
        if (GUI.Button(playRect, "<b>BẮT ĐẦU CHƠI</b>", btnPlayStyle))
        {
            currentState = MenuState.MapSelect;
        }

        // Button 2: HƯỚNG DẪN (Royal Blue)
        Rect guideRect = new Rect(centerX - btnW * 0.5f, startBtnY + (btnH + btnSpacing), btnW, btnH);
        if (GUI.Button(guideRect, "<b>HƯỚNG DẪN</b>", btnRegularStyle))
        {
            currentState = MenuState.Guide;
        }

        // Button 3: CÀI ĐẶT (Slate Blue)
        Rect settingsRect = new Rect(centerX - btnW * 0.5f, startBtnY + (btnH + btnSpacing) * 2, btnW, btnH);
        if (GUI.Button(settingsRect, "<b>CÀI ĐẶT</b>", btnRegularStyle))
        {
            currentState = MenuState.Settings;
        }

        // Button 4: THOÁT GAME (Crimson Red)
        Rect exitRect = new Rect(centerX - btnW * 0.5f, startBtnY + (btnH + btnSpacing) * 3, btnW, btnH);
        if (GUI.Button(exitRect, "<b>THOÁT GAME</b>", btnRedStyle))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // Footer copyright
        GUI.Label(new Rect(10, Screen.height - 26, Screen.width - 20, 22), "© 2026 PRU Group Project  •  Tiny Swords Tower Defense Alpha", new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 11,
            normal = { textColor = new Color(0.75f, 0.85f, 0.95f, 0.75f) }
        });
    }

    // =========================================================================
    // 2. MAP SELECTION SCREEN (CHỌN BẢN ĐỒ CHIẾN TRƯỜNG)
    // =========================================================================
    private void DrawMapSelectMenu()
    {
        float centerX = Screen.width * 0.5f;
        float centerY = Screen.height * 0.5f;

        float panelW = Mathf.Min(840f, Screen.width - 24f);
        float panelH = Mathf.Min(470f, Screen.height - 30f);
        float panelX = centerX - panelW * 0.5f;
        float panelY = centerY - panelH * 0.5f;

        // Outer Royal Frame
        DrawPanel(new Rect(panelX, panelY, panelW, panelH),
            new Color(0.08f, 0.11f, 0.16f, 0.98f),
            new Color(0.85f, 0.65f, 0.25f, 1.0f), 3);

        // Header Title
        GUI.Label(new Rect(panelX, panelY + 14f, panelW, 30f), "⚔️  <b>CHỌN BẢN ĐỒ CHIẾN TRƯỜNG</b>  ⚔️", headerStyle);

        // 3 Map Cards
        float pad = 16f;
        float gap = 12f;
        float cardW = (panelW - pad * 2f - gap * 2f) / 3f;
        float cardH = panelH - 128f;
        float cardStartY = panelY + 54f;

        // --- MAP 1: THUNG LŨNG XANH (READY / UNLOCKED) ---
        float card1X = panelX + pad;
        Rect card1Rect = new Rect(card1X, cardStartY, cardW, cardH);
        DrawPanel(card1Rect, new Color(0.06f, 0.11f, 0.16f, 0.98f), new Color(0.25f, 0.78f, 0.45f, 1.0f), 2);

        GUI.Label(new Rect(card1X + 8, cardStartY + 12, cardW - 16, 24), "🏞️  <b>Map 1: Thung Lũng</b>", cardTitleStyle);

        Rect b1 = new Rect(card1X + cardW * 0.5f - 55, cardStartY + 38, 110, 22);
        DrawPanel(b1, new Color(0.10f, 0.38f, 0.18f, 1f), new Color(0.40f, 0.90f, 0.55f, 1f), 1);
        GUI.Label(b1, "🟢 SẴN SÀNG", badgeStyle);

        string desc1 = "• <b>Địa hình:</b> Thung lũng & Sông suối\n" +
                       "• <b>Số đợt:</b> 5 Waves (Boss Orc)\n" +
                       "• <b>Hiện tượng:</b> Ngày/Đêm (+30% SPD)\n" +
                       "• <b>Độ khó:</b> ⭐⭐ (Bình thường)";
        GUI.Label(new Rect(card1X + 12, cardStartY + 68, cardW - 24, 130), desc1, cardDescStyle);

        float btnActionW = cardW - 24f;
        float btnActionH = 42f;
        Rect fight1Rect = new Rect(card1X + 12, cardStartY + cardH - 52f, btnActionW, btnActionH);
        if (GUI.Button(fight1Rect, "⚔️  <b>VÀO TRẬN</b>", btnPlayStyle))
        {
            SceneManager.LoadScene("Map 1");
        }

        // --- MAP 2: SA MẠC CÁT (LOCKED) ---
        float card2X = card1X + cardW + gap;
        Rect card2Rect = new Rect(card2X, cardStartY, cardW, cardH);
        DrawPanel(card2Rect, new Color(0.05f, 0.07f, 0.10f, 0.94f), new Color(0.28f, 0.32f, 0.38f, 0.8f), 1);

        GUI.Label(new Rect(card2X + 8, cardStartY + 12, cardW - 16, 24), "🏜️  <b>Map 2: Sa Mạc Cát</b>", cardTitleStyle);

        Rect b2 = new Rect(card2X + cardW * 0.5f - 55, cardStartY + 38, 110, 22);
        DrawPanel(b2, new Color(0.18f, 0.20f, 0.24f, 1f), new Color(0.45f, 0.48f, 0.55f, 1f), 1);
        GUI.Label(b2, "🔒 SẮP RA MẮT", badgeStyle);

        string desc2 = "• <b>Địa hình:</b> Cồn cát sa mạc\n" +
                       "• <b>Số đợt:</b> 6 Waves quái sa mạc\n" +
                       "• <b>Hiện tượng:</b> Bão cát rực lửa\n" +
                       "• <b>Độ khó:</b> ⭐⭐⭐⭐ (Khó)";
        GUI.Label(new Rect(card2X + 12, cardStartY + 68, cardW - 24, 130), desc2, cardDescStyle);

        Rect fight2Rect = new Rect(card2X + 12, cardStartY + cardH - 52f, btnActionW, btnActionH);
        if (GUI.Button(fight2Rect, "⚔️  <b>VÀO TRẬN</b>", btnPlayStyle))
        {
            SceneManager.LoadScene("Map 2");
        }

        // --- MAP 3: NÚI LỬA (LOCKED) ---
        float card3X = card2X + cardW + gap;
        Rect card3Rect = new Rect(card3X, cardStartY, cardW, cardH);
        DrawPanel(card3Rect, new Color(0.05f, 0.07f, 0.10f, 0.94f), new Color(0.28f, 0.32f, 0.38f, 0.8f), 1);

        GUI.Label(new Rect(card3X + 8, cardStartY + 12, cardW - 16, 24), "🌋  <b>Map 3: Núi Lửa</b>", cardTitleStyle);

        Rect b3 = new Rect(card3X + cardW * 0.5f - 55, cardStartY + 38, 110, 22);
        DrawPanel(b3, new Color(0.18f, 0.20f, 0.24f, 1f), new Color(0.45f, 0.48f, 0.55f, 1f), 1);
        GUI.Label(b3, "🔒 SẮP RA MẮT", badgeStyle);

        string desc3 = "• <b>Địa hình:</b> Pháo đài dung nham\n" +
                       "• <b>Số đợt:</b> 8 Waves (Boss Rồng)\n" +
                       "• <b>Hiện tượng:</b> Mưa nham thạch\n" +
                       "• <b>Độ khó:</b> ⭐⭐⭐⭐⭐ (Ác mộng)";
        GUI.Label(new Rect(card3X + 12, cardStartY + 68, cardW - 24, 130), desc3, cardDescStyle);

        Rect fight3Rect = new Rect(card3X + 12, cardStartY + cardH - 52f, btnActionW, btnActionH);
        if (GUI.Button(fight3Rect, "⚔️  <b>VÀO TRẬN</b>", btnPlayStyle))
        {
            SceneManager.LoadScene("Map 3");
        }

        // Back Button
        float backW = 160f;
        float backH = 40f;
        Rect backRect = new Rect(centerX - backW * 0.5f, panelY + panelH - 52f, backW, backH);
        if (GUI.Button(backRect, "⬅  <b>QUAY LẠI</b>", btnRegularStyle))
        {
            currentState = MenuState.Main;
        }
    }

    // =========================================================================
    // 3. HOW TO PLAY / GUIDE MODAL
    // =========================================================================
    private void DrawGuideModal()
    {
        float centerX = Screen.width * 0.5f;
        float centerY = Screen.height * 0.5f;

        float modalW = Mathf.Min(760f, Screen.width - 24f);
        float modalH = Mathf.Min(470f, Screen.height - 30f);
        float modalX = centerX - modalW * 0.5f;
        float modalY = centerY - modalH * 0.5f;

        // Outer Royal Frame
        DrawPanel(new Rect(modalX, modalY, modalW, modalH),
            new Color(0.08f, 0.11f, 0.16f, 0.98f),
            new Color(0.85f, 0.65f, 0.25f, 1.0f), 3);

        GUI.Label(new Rect(modalX, modalY + 14f, modalW, 30f), "📖  <b>CẨM NANG CHIẾN THUẬT TINY SWORDS</b>  📖", headerStyle);

        // Inner Box
        Rect contentRect = new Rect(modalX + 16f, modalY + 48f, modalW - 32f, modalH - 110f);
        DrawPanel(contentRect,
            new Color(0.05f, 0.07f, 0.11f, 0.95f),
            new Color(0.25f, 0.35f, 0.48f, 0.8f), 1);

        string guideContent =
            "<b>1. NHIỆM VỤ BẢO VỆ NHÀ CHÍNH (CASTLE):</b>\n" +
            "• Ngăn chặn quái vật tiếp cận Lâu Đài Xanh. Máu nhà chính khởi đầu là <b>15 HP</b>.\n\n" +
            "<b>2. BINH PHÁP 3 LOẠI THÁP PHÒNG THỦ:</b>\n" +
            "• 🏹 <b>Tháp Tên (50G):</b> Tầm 4.5 • Tốc độ 1.6/s • Sát thương 25. Diệt quái cơ động.\n" +
            "• 🧪 <b>Tháp Độc (75G):</b> Tầm 3.8 • Tốc độ 1.2/s • Sát thương 42 • <b>Gây Làm Chậm 45%</b>.\n" +
            "• 🔥 <b>Tháp Lửa (100G):</b> Tầm 3.2 • Tốc độ 0.85/s • Sát thương 75 • <b>Gây Thiêu Đốt Rút Máu</b>.\n\n" +
            "<b>3. HIỆN TƯỢNG NGÀY / ĐÊM & TRÙM CUỐI (BOSS ORC):</b>\n" +
            "• ☀️ <b>Ban Ngày:</b> Ánh sáng 100%, quái vật di chuyển tốc độ chuẩn.\n" +
            "• 🌙 <b>Ban Đêm:</b> Tầm nhìn giảm 80%, quái vật nhận hiệu ứng <b>Cuồng Nộ (+30% Tốc độ)</b>.\n" +
            "• 👹 <b>Trùm Cuối:</b> Xuất hiện ở Wave 5, máu siêu trâu và cuồng nộ khi máu dưới 40%!\n\n" +
            "<b>4. THAO TÁC ĐIỀU KHIỂN:</b>\n" +
            "• <b>Chuột Trái:</b> Chọn loại tháp và bấm vào ô đất để xây.\n" +
            "• <b>Chuột Phải / Phím ✕:</b> Hủy chọn tháp  •  <b>Phím ESC:</b> Tạm dừng trận đấu.";

        GUI.Label(new Rect(contentRect.x + 14f, contentRect.y + 10f, contentRect.width - 28f, contentRect.height - 20f), guideContent, bodyStyle);

        // Close Button
        float closeW = 160f;
        float closeH = 40f;
        Rect closeRect = new Rect(centerX - closeW * 0.5f, modalY + modalH - 50f, closeW, closeH);
        if (GUI.Button(closeRect, "✓  <b>ĐÃ HIỂU</b>", btnPlayStyle))
        {
            currentState = MenuState.Main;
        }
    }

    // =========================================================================
    // 4. SETTINGS MODAL
    // =========================================================================
    private void DrawSettingsModal()
    {
        float centerX = Screen.width * 0.5f;
        float centerY = Screen.height * 0.5f;

        float modalW = Mathf.Min(540f, Screen.width - 24f);
        float modalH = Mathf.Min(370f, Screen.height - 30f);
        float modalX = centerX - modalW * 0.5f;
        float modalY = centerY - modalH * 0.5f;

        // Outer Royal Frame
        DrawPanel(new Rect(modalX, modalY, modalW, modalH),
            new Color(0.08f, 0.11f, 0.16f, 0.98f),
            new Color(0.85f, 0.65f, 0.25f, 1.0f), 3);

        GUI.Label(new Rect(modalX, modalY + 14f, modalW, 30f), "⚙️  <b>CÀI ĐẶT TRÒ CHƠI</b>  ⚙️", headerStyle);

        // Inner Box
        Rect contentRect = new Rect(modalX + 16f, modalY + 48f, modalW - 32f, modalH - 110f);
        DrawPanel(contentRect,
            new Color(0.05f, 0.07f, 0.11f, 0.95f),
            new Color(0.25f, 0.35f, 0.48f, 0.8f), 1);

        float optX = contentRect.x + 20f;
        float optY = contentRect.y + 16f;
        float optW = contentRect.width - 40f;

        // Music Volume
        GUI.Label(new Rect(optX, optY, optW, 22f), $"🎵  <b>Nhạc Nền (BGM):</b>  {(int)(bgmVolume * 100)}%", bodyStyle);
        bgmVolume = GUI.HorizontalSlider(new Rect(optX, optY + 24f, optW, 20f), bgmVolume, 0f, 1f);
        OnBGMVolumeChanged?.Invoke(bgmVolume);

        // SFX Volume
        GUI.Label(new Rect(optX, optY + 52f, optW, 22f), $"🔊  <b>Hiệu Ứng Âm Thanh (SFX):</b>  {(int)(sfxVolume * 100)}%", bodyStyle);
        sfxVolume = GUI.HorizontalSlider(new Rect(optX, optY + 76f, optW, 20f), sfxVolume, 0f, 1f);
        OnSFXVolumeChanged?.Invoke(sfxVolume);

        // Fullscreen Toggle
        bool newFullscreen = GUI.Toggle(new Rect(optX, optY + 112f, optW, 24f), isFullscreen, "  🖥️  <b>Chế Độ Toàn Màn Hình (Fullscreen)</b>", bodyStyle);
        if (newFullscreen != isFullscreen)
        {
            isFullscreen = newFullscreen;
            Screen.fullScreen = isFullscreen;
        }

        // Close Button
        float closeW = 160f;
        float closeH = 40f;
        Rect closeRect = new Rect(centerX - closeW * 0.5f, modalY + modalH - 50f, closeW, closeH);
        if (GUI.Button(closeRect, "✓  <b>LƯU & ĐÓNG</b>", btnPlayStyle))
        {
            currentState = MenuState.Main;
        }
    }

    /// <summary>
    /// Draws a crisp pixel-perfect bordered panel with 0% texture distortion.
    /// </summary>
    private void DrawPanel(Rect r, Color fillColor, Color borderColor, int borderWidth = 2)
    {
        // 1. Outer Border
        GUI.color = borderColor;
        GUI.DrawTexture(r, Texture2D.whiteTexture);

        // 2. Inner Fill
        GUI.color = fillColor;
        GUI.DrawTexture(new Rect(r.x + borderWidth, r.y + borderWidth, r.width - borderWidth * 2, r.height - borderWidth * 2), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    // =========================================================================
    // TEXTURE GENERATOR (CRISP, SHARP & GORGEOUS)
    // =========================================================================
    private void GenerateSharpTextures()
    {
        // 1. Royal Gold Title Banner (Double-bordered Gold & Navy)
        texLogoBanner = CreateSharpBorderTex(64, 64,
            new Color(0.95f, 0.78f, 0.22f, 1.0f),
            new Color(0.55f, 0.40f, 0.08f, 1.0f), 2);

        texLogoBackdrop = CreateSharpGradientTex(64, 64,
            new Color(0.12f, 0.18f, 0.28f, 0.98f),
            new Color(0.06f, 0.09f, 0.15f, 0.98f),
            new Color(0.95f, 0.78f, 0.22f, 1.0f), 2);

        // 2. Play Button (Emerald Green Gradient)
        texBtnPlayNormal = CreateSharpGradientTex(64, 64,
            new Color(0.18f, 0.68f, 0.35f, 1f),
            new Color(0.08f, 0.44f, 0.20f, 1f),
            new Color(0.55f, 0.95f, 0.70f, 1f), 2);

        texBtnPlayHover = CreateSharpGradientTex(64, 64,
            new Color(0.24f, 0.82f, 0.44f, 1f),
            new Color(0.12f, 0.56f, 0.26f, 1f),
            new Color(0.85f, 1.0f, 0.90f, 1f), 2);

        // 3. Regular Button (Knight Royal Blue Gradient)
        texBtnRegularNormal = CreateSharpGradientTex(64, 64,
            new Color(0.20f, 0.28f, 0.42f, 1f),
            new Color(0.11f, 0.16f, 0.26f, 1f),
            new Color(0.45f, 0.62f, 0.82f, 1f), 2);

        texBtnRegularHover = CreateSharpGradientTex(64, 64,
            new Color(0.28f, 0.38f, 0.58f, 1f),
            new Color(0.16f, 0.24f, 0.38f, 1f),
            new Color(0.72f, 0.85f, 1.0f, 1f), 2);

        // 4. Red Button (Crimson Gradient)
        texBtnRedNormal = CreateSharpGradientTex(64, 64,
            new Color(0.75f, 0.20f, 0.20f, 1f),
            new Color(0.45f, 0.10f, 0.10f, 1f),
            new Color(1.0f, 0.55f, 0.55f, 1f), 2);

        texBtnRedHover = CreateSharpGradientTex(64, 64,
            new Color(0.88f, 0.28f, 0.28f, 1f),
            new Color(0.56f, 0.12f, 0.12f, 1f),
            new Color(1.0f, 0.75f, 0.75f, 1f), 2);

        // 5. Modal Panels
        texModalWood = CreateSharpBorderTex(64, 64,
            new Color(0.08f, 0.11f, 0.16f, 0.96f),
            new Color(0.72f, 0.52f, 0.22f, 1.0f), 3);

        texModalInner = CreateSharpBorderTex(64, 64,
            new Color(0.05f, 0.07f, 0.11f, 0.95f),
            new Color(0.25f, 0.35f, 0.48f, 0.8f), 1);

        // 6. Map Cards
        texCardUnlocked = CreateSharpBorderTex(64, 64,
            new Color(0.06f, 0.10f, 0.15f, 0.96f),
            new Color(0.25f, 0.75f, 0.45f, 1f), 2);

        texCardLocked = CreateSharpBorderTex(64, 64,
            new Color(0.05f, 0.06f, 0.08f, 0.90f),
            new Color(0.25f, 0.28f, 0.34f, 0.7f), 1);

        // 7. Badges
        texBadgeReady = CreateSharpBorderTex(64, 64,
            new Color(0.10f, 0.35f, 0.18f, 1f),
            new Color(0.40f, 0.90f, 0.55f, 1f), 1);

        texBadgeLocked = CreateSharpBorderTex(64, 64,
            new Color(0.18f, 0.20f, 0.24f, 1f),
            new Color(0.45f, 0.48f, 0.55f, 1f), 1);

        // 8. Fullscreen Dim
        texDimOverlay = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texDimOverlay.SetPixel(0, 0, new Color(0.02f, 0.04f, 0.07f, 0.75f));
        texDimOverlay.Apply();
    }

    private Texture2D CreateSharpBorderTex(int w, int h, Color fill, Color border, int bWidth = 1)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] cols = new Color[w * h];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (x < bWidth || x >= w - bWidth || y < bWidth || y >= h - bWidth)
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

    private Texture2D CreateSharpGradientTex(int w, int h, Color topFill, Color botFill, Color border, int bWidth = 1)
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
                if (x < bWidth || x >= w - bWidth || y < bWidth || y >= h - bWidth)
                {
                    cols[y * w + x] = border;
                }
                else
                {
                    if (y >= h - 2) curFill = Color.Lerp(curFill, Color.white, 0.35f);
                    cols[y * w + x] = curFill;
                }
            }
        }
        tex.SetPixels(cols);
        tex.Apply();
        return tex;
    }

    private void InitStyles()
    {
        if (logoTitleStyle == null)
        {
            logoTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                richText = true
            };
            logoTitleStyle.normal.textColor = new Color(1f, 0.90f, 0.45f, 1f);
        }

        if (logoSubtitleStyle == null)
        {
            logoSubtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                richText = true
            };
            logoSubtitleStyle.normal.textColor = new Color(0.75f, 0.85f, 1f, 0.85f);
        }

        if (btnPlayStyle == null)
        {
            btnPlayStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                richText = true
            };
            btnPlayStyle.normal.background = texBtnPlayNormal;
            btnPlayStyle.hover.background = texBtnPlayHover;
            btnPlayStyle.normal.textColor = Color.white;
            btnPlayStyle.hover.textColor = new Color(1f, 0.95f, 0.7f, 1f);
        }

        if (btnRegularStyle == null)
        {
            btnRegularStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                richText = true
            };
            btnRegularStyle.normal.background = texBtnRegularNormal;
            btnRegularStyle.hover.background = texBtnRegularHover;
            btnRegularStyle.normal.textColor = new Color(0.92f, 0.96f, 1f, 1f);
            btnRegularStyle.hover.textColor = Color.white;
        }

        if (btnRedStyle == null)
        {
            btnRedStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                richText = true
            };
            btnRedStyle.normal.background = texBtnRedNormal;
            btnRedStyle.hover.background = texBtnRedHover;
            btnRedStyle.normal.textColor = new Color(1f, 0.9f, 0.9f, 1f);
            btnRedStyle.hover.textColor = Color.white;
        }

        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 19,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                richText = true
            };
            headerStyle.normal.textColor = new Color(1f, 0.88f, 0.35f, 1f);
        }

        if (bodyStyle == null)
        {
            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.UpperLeft,
                richText = true,
                wordWrap = true
            };
            bodyStyle.normal.textColor = new Color(0.90f, 0.94f, 1f, 1f);
        }

        if (cardTitleStyle == null)
        {
            cardTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                richText = true
            };
            cardTitleStyle.normal.textColor = Color.white;
        }

        if (cardDescStyle == null)
        {
            cardDescStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.UpperLeft,
                richText = true,
                wordWrap = true
            };
            cardDescStyle.normal.textColor = new Color(0.85f, 0.90f, 1f, 1f);
        }

        if (badgeStyle == null)
        {
            badgeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                richText = true
            };
            badgeStyle.normal.textColor = Color.white;
        }
    }
}

