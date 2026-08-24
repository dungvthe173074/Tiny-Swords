using System;
using UnityEngine;
#if UNITY_URP_2D || true
using UnityEngine.Rendering.Universal;
#endif

public enum TimeOfDayPhase
{
    Day,
    Sunset,
    Night,
    Dawn
}

public class DayNightCycle : MonoBehaviour
{
    public static DayNightCycle Instance { get; private set; }

    [Header("Cycle Settings (Chu Kỳ Thời Gian)")]
    [Tooltip("Tổng thời gian 1 chu kỳ ngày đêm (tăng lên 30s để chuyển cảnh tự nhiên)")]
    public float cycleDuration = 30.0f;

    [Tooltip("Tỷ lệ thời gian ban ngày (0.5 = 50% ngày, 50% đêm)")]
    [Range(0.2f, 0.8f)]
    public float dayRatio = 0.5f;

    [Header("Lighting & Visibility (Độ Sáng & Tầm Nhìn)")]
    [Tooltip("Độ sáng ban ngày (1.0 = 100% ánh sáng)")]
    public float maxLightIntensity = 1.0f;

    [Tooltip("Độ sáng ban đêm (0.2 = giảm 80% tầm nhìn theo yêu cầu)")]
    public float minLightIntensity = 0.2f;

    [Header("Color Palette (Bảng Màu Ánh Sáng Tự Nhiên)")]
    public Color middayColor = new Color(1.0f, 0.98f, 0.92f, 1.0f);     // Nắng trưa rực rỡ
    public Color afternoonColor = new Color(1.0f, 0.88f, 0.70f, 1.0f);  // Nắng chiều vàng dịu
    public Color sunsetColor = new Color(1.0f, 0.55f, 0.32f, 1.0f);     // Hoàng hôn cam đỏ
    public Color duskColor = new Color(0.50f, 0.40f, 0.75f, 1.0f);       // Chập tối tím thẫm
    public Color midnightColor = new Color(0.32f, 0.42f, 0.85f, 1.0f);   // Nửa đêm trăng xanh mờ
    public Color dawnColor = new Color(0.95f, 0.72f, 0.55f, 1.0f);       // Bình minh ửng hồng

    [Header("Enemy Multipliers (Hệ Số Quái Vật)")]
    [Tooltip("Tốc độ quái vật ban đêm (+30% tốc độ = 1.3x)")]
    public float nightSpeedMultiplier = 1.30f;

    [Tooltip("Sát thương quái vật lên nhà chính ban đêm")]
    public int nightCastleDamage = 1;

    [Tooltip("Tốc độ quái vật ban ngày (1.0x = Chuẩn)")]
    public float daySpeedMultiplier = 1.0f;

    [Tooltip("Sát thương quái vật lên nhà chính ban ngày")]
    public int dayCastleDamage = 1;

    [Header("Light References")]
    public Light2D globalLight2D;

    [Header("Runtime Status (Read-Only)")]
    [SerializeField] private float currentTimer = 0f;
    [SerializeField] private bool isNight = false;
    [SerializeField] private float darknessFactor = 0f; // 0.0 = Midday, 1.0 = Peak Midnight
    [SerializeField] private TimeOfDayPhase currentPhase = TimeOfDayPhase.Day;

    // Public Getters
    public float CurrentTimer => currentTimer;
    public float CycleDuration => cycleDuration;
    public bool IsNight => isNight;
    public float DarknessFactor => darknessFactor;
    public TimeOfDayPhase CurrentPhase => currentPhase;
    public float CurrentSpeedMultiplier => Mathf.Lerp(daySpeedMultiplier, nightSpeedMultiplier, darknessFactor);
    public int CurrentCastleDamage => isNight ? nightCastleDamage : dayCastleDamage;

    // Events
    public event Action<bool> OnDayNightChanged;
    public event Action<float> OnDarknessChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        FindGlobalLightIfNeeded();
    }

    private void Start()
    {
        FindGlobalLightIfNeeded();
        UpdateDayNightState(true);
    }

    public void FindGlobalLightIfNeeded()
    {
        if (globalLight2D == null)
        {
            GameObject lightObj = GameObject.Find("Global Light 2D");
            if (lightObj != null)
            {
                globalLight2D = lightObj.GetComponent<Light2D>();
            }
            if (globalLight2D == null)
            {
#if UNITY_2023_1_OR_NEWER
                globalLight2D = FindAnyObjectByType<Light2D>();
#else
                globalLight2D = FindObjectOfType<Light2D>();
#endif
            }
        }
    }

    private void Update()
    {
        if (cycleDuration <= 0f) return;

        // Vòng lặp ngày đêm diễn ra liên tục theo thời gian thực (không phụ thuộc vào quái vật)
        currentTimer += Time.deltaTime;
        if (currentTimer >= cycleDuration)
        {
            currentTimer -= cycleDuration;
        }

        UpdateDayNightState(false);
    }

    /// <summary>
    /// Chuyển đổi màu sắc và độ sáng cực kỳ mượt mà từng frame qua đường cong liên tục 24h.
    /// </summary>
    private void UpdateDayNightState(bool forceUpdate)
    {
        float t = currentTimer / cycleDuration; // 0.0 -> 1.0
        bool wasNight = isNight;

        Color targetColor;
        float targetIntensity;

        // Timeline 24 giờ siêu mượt:
        // [0.00 - 0.20]: Ban ngày (12h - 15h) -> Nắng ấm rực rỡ
        // [0.20 - 0.42]: Chiều tà -> Hoàng hôn (15h - 18h) -> Nắng vàng dịu chuyển dần sang cam ráng chiều
        // [0.42 - 0.55]: Chập tối -> Đêm (18h - 21h) -> Cam ráng chiều chuyển dần sang tím tối rồi xanh đêm mờ
        // [0.55 - 0.75]: Nửa đêm (21h - 03h) -> Đêm tối mờ 80% (Visibility 0.2), xanh trăng sâu thẳm
        // [0.75 - 0.88]: Rạng đông -> Bình minh (03h - 07h) -> Xanh đêm chuyển dần sang ửng hồng bình minh
        // [0.88 - 1.00]: Sáng sớm -> Giữa trưa (07h - 12h) -> Bình minh sáng dần đều trở lại nắng ấm

        if (t < 0.20f)
        {
            // Ban ngày
            currentPhase = TimeOfDayPhase.Day;
            float segT = t / 0.20f;
            targetColor = Color.Lerp(middayColor, afternoonColor, segT * 0.3f);
            targetIntensity = maxLightIntensity;
            darknessFactor = 0f;
        }
        else if (t < 0.42f)
        {
            // Chiều chuyển dần sang hoàng hôn (Tối dần mềm mại)
            currentPhase = TimeOfDayPhase.Sunset;
            float segT = (t - 0.20f) / (0.42f - 0.20f);
            targetColor = Color.Lerp(afternoonColor, sunsetColor, segT);
            targetIntensity = Mathf.Lerp(maxLightIntensity, 0.60f, segT);
            darknessFactor = Mathf.Lerp(0f, 0.45f, segT);
        }
        else if (t < 0.55f)
        {
            // Hoàng hôn chuyển vào đêm tối (Chuyển sắc mượt mà không giật)
            currentPhase = TimeOfDayPhase.Sunset;
            float segT = (t - 0.42f) / (0.55f - 0.42f);
            targetColor = Color.Lerp(sunsetColor, duskColor, segT);
            targetIntensity = Mathf.Lerp(0.60f, minLightIntensity, segT);
            darknessFactor = Mathf.Lerp(0.45f, 1.0f, segT);
        }
        else if (t < 0.75f)
        {
            // Nửa đêm tối nhất (Giảm 80% tầm nhìn theo yêu cầu)
            currentPhase = TimeOfDayPhase.Night;
            float segT = (t - 0.55f) / (0.75f - 0.55f);
            targetColor = Color.Lerp(duskColor, midnightColor, Mathf.Min(segT * 2f, 1f));
            targetIntensity = minLightIntensity; // 0.2f
            darknessFactor = 1.0f;
        }
        else if (t < 0.88f)
        {
            // Rạng đông (Sáng dần mềm mại)
            currentPhase = TimeOfDayPhase.Dawn;
            float segT = (t - 0.75f) / (0.88f - 0.75f);
            targetColor = Color.Lerp(midnightColor, dawnColor, segT);
            targetIntensity = Mathf.Lerp(minLightIntensity, 0.65f, segT);
            darknessFactor = Mathf.Lerp(1.0f, 0.35f, segT);
        }
        else
        {
            // Bình minh sáng rực về trưa
            currentPhase = TimeOfDayPhase.Dawn;
            float segT = (t - 0.88f) / (1.0f - 0.88f);
            targetColor = Color.Lerp(dawnColor, middayColor, segT);
            targetIntensity = Mathf.Lerp(0.65f, maxLightIntensity, segT);
            darknessFactor = Mathf.Lerp(0.35f, 0.0f, segT);
        }

        isNight = darknessFactor >= 0.5f;

        // Cập nhật mượt mà trực tiếp lên Global Light 2D
        if (globalLight2D != null)
        {
            globalLight2D.intensity = targetIntensity;
            globalLight2D.color = targetColor;
        }

        // Kích hoạt sự kiện
        if (wasNight != isNight || forceUpdate)
        {
            OnDayNightChanged?.Invoke(isNight);
        }

        OnDarknessChanged?.Invoke(darknessFactor);
    }
}

