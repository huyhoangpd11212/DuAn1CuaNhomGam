using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SimpleLevelNotifier : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private Text warningText;
    [SerializeField] private Image backgroundImage;

    [Header("Settings")]
    [SerializeField] private float displayTime = 4f;
    [SerializeField] private float fadeSpeed = 2f;
    [SerializeField] private Color warningColor = Color.yellow;

    public static SimpleLevelNotifier instance;

    // Warning messages for each level
    private string[] level2Warnings = {
        "⚠️ X2 MÁU CHO ENEMY!",
        "🔥 THÊM QUÁI MỚI!",
        "⚡ TỐC ĐỘ TĂNG!",
        "💥 SÁT THƯƠNG TĂNG!"
    };

    private string[] level3Warnings = {
        "⚠️ BOSS X2 MÁU!",
        "🌟 THIÊN THẠCH RƠI XUỐNG!",
        "🚀 QUÁI BAY NHANH HỠN!",
        "💀 CHẾ ĐỘ KHÓ!"
    };

    private string[] bossWarnings = {
        "💀 BOSS CUỐI CÙNG!",
        "⚡ SIÊU BOSS X3 MÁU!",
        "🔥 ĐẠN BOSS SIÊU NHANH!",
        "🌟 THÁCH THỨC CUỐI CÙNG!"
    };

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        HideNotification();
    }

    // ✅ MAIN METHOD: Show warning for specific level
    public void ShowLevelWarning(int level)
    {
        StartCoroutine(DisplayWarning(level));
    }

    private IEnumerator DisplayWarning(int level)
    {
        // Get warnings for this level
        string[] warnings = GetWarningsForLevel(level);

        // Show panel
        notificationPanel.SetActive(true);

        // Display each warning
        for (int i = 0; i < warnings.Length; i++)
        {
            // Set warning text
            warningText.text = warnings[i];
            warningText.color = warningColor;

            // Fade in
            yield return StartCoroutine(FadeIn());

            // Wait
            yield return new WaitForSeconds(displayTime / warnings.Length);

            // Fade out (except for last warning)
            if (i < warnings.Length - 1)
            {
                yield return StartCoroutine(FadeOut());
            }
        }

        // Final fade out
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(FadeOut());

        // Hide panel
        HideNotification();
    }

    private string[] GetWarningsForLevel(int level)
    {
        switch (level)
        {
            case 2:
                return level2Warnings;
            case 3:
                return level3Warnings;
            case 4: // Boss level
                return bossWarnings;
            default:
                return new string[] { "⚠️ LEVEL MỚI!", "🔥 CHUẨN BỊ CHIẾN ĐẤU!" };
        }
    }

    private IEnumerator FadeIn()
    {
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;
            SetAlpha(alpha);
            yield return null;
        }
        SetAlpha(1f);
    }

    private IEnumerator FadeOut()
    {
        float alpha = 1f;
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            SetAlpha(alpha);
            yield return null;
        }
        SetAlpha(0f);
    }

    private void SetAlpha(float alpha)
    {
        if (warningText != null)
        {
            Color textColor = warningText.color;
            textColor.a = alpha;
            warningText.color = textColor;
        }

        if (backgroundImage != null)
        {
            Color bgColor = backgroundImage.color;
            bgColor.a = alpha * 0.8f; // Semi-transparent background
            backgroundImage.color = bgColor;
        }
    }

    private void HideNotification()
    {
        if (notificationPanel != null)
            notificationPanel.SetActive(false);
    }

    // ✅ PUBLIC METHODS - Gọi từ GameManager
    public void ShowLevel2Warning()
    {
        ShowLevelWarning(2);
    }

    public void ShowLevel3Warning()
    {
        ShowLevelWarning(3);
    }

    public void ShowBossWarning()
    {
        ShowLevelWarning(4);
    }

    // ✅ TEST KEYS
    void Update()
    {
        if (Application.isEditor)
        {
            if (Input.GetKeyDown(KeyCode.F10))
            {
                ShowLevel2Warning();
                Debug.Log("🧪 Test Level 2 warning");
            }

            if (Input.GetKeyDown(KeyCode.F11))
            {
                ShowLevel3Warning();
                Debug.Log("🧪 Test Level 3 warning");
            }

            if (Input.GetKeyDown(KeyCode.F12))
            {
                ShowBossWarning();
                Debug.Log("🧪 Test Boss warning");
            }
        }
    }
}