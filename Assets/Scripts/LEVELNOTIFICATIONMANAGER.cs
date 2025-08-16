using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelNotificationManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private Text levelTitleText;
    [SerializeField] private Text bonusText;
    [SerializeField] private Text motivationText;
    [SerializeField] private Image levelIcon;
    [SerializeField] private Button continueButton;
    [SerializeField] private Image backgroundImage;

    [Header("Animation Settings")]
    [SerializeField] private float displayDuration = 4f;
    [SerializeField] private float fadeInDuration = 0.8f;
    [SerializeField] private float fadeOutDuration = 0.6f;
    [SerializeField] private bool pauseGameDuringNotification = true;
    [SerializeField] private bool autoCloseAfterDuration = true;

    [Header("Visual Effects")]
    [SerializeField] private bool enableScaleAnimation = true;
    [SerializeField] private bool enableGlowEffect = true;
    [SerializeField] private float scaleAnimationIntensity = 0.1f;
    [SerializeField] private float glowSpeed = 2f;

    [Header("Audio")]
    [SerializeField] private AudioClip levelUpSound;
    [SerializeField] private AudioClip bonusSound;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private float soundVolume = 0.7f;

    [Header("Level Configurations")]
    [SerializeField] private LevelConfig[] levelConfigs;

    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool showTestKeysInBuild = false;

    public static LevelNotificationManager instance;
    private AudioSource audioSource;
    private CanvasGroup canvasGroup;
    private bool isShowing = false;
    private Vector3 originalScale;
    private Color originalBackgroundColor;
    private Coroutine glowCoroutine;

    [System.Serializable]
    public class LevelConfig
    {
        [Header("Basic Info")]
        public string levelName = "LEVEL 2";
        public string sceneName = "Scene2";
        public Color levelColor = Color.cyan;
        public Sprite levelIcon;

        [Header("Bonuses")]
        public string[] bonuses = { "🔥 x2 HEALTH", "⚡ FASTER RELOAD", "💪 +50% DAMAGE" };

        [Header("Motivation")]
        public string motivationText = "GET READY FOR MORE CHALLENGE!";
        public Color motivationColor = Color.white;

        [Header("Visual")]
        public Color backgroundColor = new Color(0, 0, 0, 0.8f);
        public bool useCustomIcon = false;
    }

    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeComponents();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        SetupDefaultConfigurations();
        HideNotificationImmediate();

        if (enableDebugLogs)
            Debug.Log("✅ LevelNotificationManager initialized successfully");
    }

    void Update()
    {
        // Debug keys (only in editor or if enabled for build)
        if (Application.isEditor || showTestKeysInBuild)
        {
            if (Input.GetKeyDown(KeyCode.F10))
            {
                ShowLevel2Notification();
                if (enableDebugLogs) Debug.Log("🧪 F10: Test Level 2 notification");
            }

            if (Input.GetKeyDown(KeyCode.F11))
            {
                ShowLevel3Notification();
                if (enableDebugLogs) Debug.Log("🧪 F11: Test Level 3 notification");
            }

            if (Input.GetKeyDown(KeyCode.F12))
            {
                ShowBossLevelNotification();
                if (enableDebugLogs) Debug.Log("🧪 F12: Test Boss notification");
            }

            if (Input.GetKeyDown(KeyCode.Insert))
            {
                ShowVictoryNotification();
                if (enableDebugLogs) Debug.Log("🧪 Insert: Test Victory notification");
            }

            if (Input.GetKeyDown(KeyCode.Delete))
            {
                CloseNotification();
                if (enableDebugLogs) Debug.Log("🧪 Delete: Force close notification");
            }
        }

        // Handle ESC key to close notification
        if (isShowing && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseNotification();
        }
    }

    private void InitializeComponents()
    {
        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.volume = soundVolume;
        audioSource.playOnAwake = false;

        // Setup canvas group for fade effects
        if (notificationPanel != null)
        {
            canvasGroup = notificationPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = notificationPanel.AddComponent<CanvasGroup>();

            // Store original scale for animations
            originalScale = notificationPanel.transform.localScale;

            // Store original background color
            if (backgroundImage != null)
                originalBackgroundColor = backgroundImage.color;
        }

        // Setup continue button
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueButtonClicked);
    }

    private void SetupDefaultConfigurations()
    {
        if (levelConfigs == null || levelConfigs.Length == 0)
        {
            levelConfigs = new LevelConfig[5];

            // Level 2 Configuration
            levelConfigs[0] = new LevelConfig
            {
                levelName = "🌟 LEVEL 2 🌟",
                sceneName = "Scene2",
                bonuses = new string[] {
                    "🔥 DOUBLE HEALTH BOOST",
                    "⚡ LIGHTNING FAST RELOAD",
                    "💪 +50% DAMAGE POWER",
                    "🛡️ IMPROVED DEFENSE"
                },
                levelColor = new Color(0f, 0.8f, 1f), // Cyan
                motivationText = "ENEMIES ARE GETTING STRONGER!",
                motivationColor = Color.yellow,
                backgroundColor = new Color(0.1f, 0.3f, 0.6f, 0.9f)
            };

            // Level 3 Configuration
            levelConfigs[1] = new LevelConfig
            {
                levelName = "⚡ LEVEL 3 ⚡",
                sceneName = "Scene3",
                bonuses = new string[] {
                    "🛡️ ULTIMATE SHIELD BOOST",
                    "🚀 HYPERSPEED x2.0",
                    "💎 DOUBLE POINT MULTIPLIER",
                    "🔫 ENHANCED WEAPON SYSTEMS"
                },
                levelColor = new Color(1f, 0f, 1f), // Magenta
                motivationText = "PREPARE FOR INTENSE WARFARE!",
                motivationColor = Color.red,
                backgroundColor = new Color(0.6f, 0.1f, 0.6f, 0.9f)
            };

            // Boss Level Configuration
            levelConfigs[2] = new LevelConfig
            {
                levelName = "💀 BOSS LEVEL 💀",
                sceneName = "BossLevel",
                bonuses = new string[] {
                    "⭐ ALL UPGRADES MAXED",
                    "🔫 UNLIMITED AMMUNITION",
                    "💀 BOSS SLAYER MODE",
                    "🌟 LEGENDARY POWER"
                },
                levelColor = new Color(1f, 0.2f, 0f), // Red-Orange
                motivationText = "FACE THE ULTIMATE CHALLENGE!",
                motivationColor = Color.white,
                backgroundColor = new Color(0.8f, 0.1f, 0.1f, 0.95f)
            };

            // Victory Configuration
            levelConfigs[3] = new LevelConfig
            {
                levelName = "🏆 VICTORY! 🏆",
                sceneName = "Victory",
                bonuses = new string[] {
                    "🎉 GAME COMPLETED SUCCESSFULLY!",
                    "👑 YOU ARE THE CHAMPION!",
                    "⭐ LEGENDARY SCORE ACHIEVED",
                    "🌟 MASTER OF THE GALAXY"
                },
                levelColor = new Color(1f, 0.8f, 0f), // Gold
                motivationText = "CONGRATULATIONS ON YOUR EPIC VICTORY!",
                motivationColor = Color.white,
                backgroundColor = new Color(0.8f, 0.6f, 0f, 0.95f)
            };

            // Game Over Configuration
            levelConfigs[4] = new LevelConfig
            {
                levelName = "💥 GAME OVER 💥",
                sceneName = "GameOver",
                bonuses = new string[] {
                    "🔄 READY FOR ANOTHER TRY?",
                    "💪 LEARN FROM THE CHALLENGE",
                    "🎯 AIM FOR HIGHER SCORES",
                    "🚀 COME BACK STRONGER"
                },
                levelColor = Color.red,
                motivationText = "DON'T GIVE UP! TRY AGAIN!",
                motivationColor = Color.white,
                backgroundColor = new Color(0.6f, 0.1f, 0.1f, 0.9f)
            };

            if (enableDebugLogs)
                Debug.Log("✅ Default level configurations created");
        }
    }

    // ===== PUBLIC METHODS =====

    public void ShowLevelNotification(string sceneName)
    {
        if (isShowing)
        {
            if (enableDebugLogs)
                Debug.LogWarning("⚠️ Notification already showing, ignoring new request");
            return;
        }

        LevelConfig config = GetLevelConfig(sceneName);
        if (config != null)
        {
            StartCoroutine(DisplayLevelNotification(config));
        }
        else
        {
            if (enableDebugLogs)
                Debug.LogWarning($"⚠️ No level config found for scene: {sceneName}");
        }
    }

    public void ShowCustomNotification(string title, string[] bonuses, Color color, string motivation = "", float duration = 0f)
    {
        if (isShowing) return;

        LevelConfig customConfig = new LevelConfig
        {
            levelName = title,
            bonuses = bonuses,
            levelColor = color,
            motivationText = motivation,
            backgroundColor = new Color(color.r * 0.3f, color.g * 0.3f, color.b * 0.3f, 0.9f)
        };

        if (duration > 0f)
        {
            float originalDuration = displayDuration;
            displayDuration = duration;
            StartCoroutine(DisplayLevelNotification(customConfig));
            displayDuration = originalDuration;
        }
        else
        {
            StartCoroutine(DisplayLevelNotification(customConfig));
        }
    }

    public void ShowLevel2Notification()
    {
        ShowLevelNotification("Scene2");
    }

    public void ShowLevel3Notification()
    {
        ShowLevelNotification("Scene3");
    }

    public void ShowBossLevelNotification()
    {
        ShowLevelNotification("BossLevel");
    }

    public void ShowVictoryNotification()
    {
        ShowLevelNotification("Victory");
    }

    public void ShowGameOverNotification()
    {
        ShowLevelNotification("GameOver");
    }

    public void CloseNotification()
    {
        if (isShowing)
        {
            StopAllCoroutines(); // Stop any running animations
            StartCoroutine(CloseNotificationCoroutine());
        }
    }

    public bool IsShowing()
    {
        return isShowing;
    }

    // ===== PRIVATE METHODS =====

    private LevelConfig GetLevelConfig(string sceneName)
    {
        foreach (LevelConfig config in levelConfigs)
        {
            if (config.sceneName.Equals(sceneName, System.StringComparison.OrdinalIgnoreCase))
            {
                return config;
            }
        }

        // Fallback: try to match by level name
        foreach (LevelConfig config in levelConfigs)
        {
            if (config.levelName.ToLower().Contains(sceneName.ToLower()))
            {
                return config;
            }
        }

        return null;
    }

    private IEnumerator DisplayLevelNotification(LevelConfig config)
    {
        isShowing = true;

        if (enableDebugLogs)
            Debug.Log($"🌟 Showing level notification: {config.levelName}");

        // Pause game if setting enabled
        if (pauseGameDuringNotification)
        {
            Time.timeScale = 0f;
            if (enableDebugLogs)
                Debug.Log("⏸️ Game paused for notification");
        }

        // Setup UI elements
        SetupNotificationUI(config);

        // Show notification panel
        notificationPanel.SetActive(true);

        // Play notification sound
        PlayNotificationSound();

        // Start visual effects
        if (enableScaleAnimation)
            StartCoroutine(ScaleAnimation());

        if (enableGlowEffect)
            glowCoroutine = StartCoroutine(GlowEffect(config.levelColor));

        // Fade in animation
        yield return StartCoroutine(FadeIn());

        // Wait for display duration or manual close
        if (autoCloseAfterDuration)
        {
            float timeElapsed = 0f;
            while (timeElapsed < displayDuration && isShowing)
            {
                timeElapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            // Auto close if still showing
            if (isShowing)
            {
                if (enableDebugLogs)
                    Debug.Log("⏰ Auto-closing notification after duration");
                yield return StartCoroutine(CloseNotificationCoroutine());
            }
        }
    }

    private void SetupNotificationUI(LevelConfig config)
    {
        // Set level title
        if (levelTitleText != null)
        {
            levelTitleText.text = config.levelName;
            levelTitleText.color = config.levelColor;
        }

        // Set bonuses text
        if (bonusText != null)
        {
            string bonusString = "";
            for (int i = 0; i < config.bonuses.Length; i++)
            {
                bonusString += config.bonuses[i];
                if (i < config.bonuses.Length - 1)
                    bonusString += "\n";
            }
            bonusText.text = bonusString;
        }

        // Set motivation text
        if (motivationText != null)
        {
            motivationText.text = config.motivationText;
            motivationText.color = config.motivationColor;
        }

        // Set level icon
        if (levelIcon != null)
        {
            if (config.levelIcon != null && config.useCustomIcon)
            {
                levelIcon.sprite = config.levelIcon;
            }
            levelIcon.color = config.levelColor;
        }

        // Set background
        if (backgroundImage != null)
        {
            backgroundImage.color = config.backgroundColor;
        }
    }

    private void PlayNotificationSound()
    {
        if (audioSource != null)
        {
            if (levelUpSound != null)
            {
                audioSource.PlayOneShot(levelUpSound, soundVolume);
                if (enableDebugLogs)
                    Debug.Log("🔊 Played level up sound");
            }

            if (bonusSound != null)
            {
                StartCoroutine(PlayBonusSoundDelayed());
            }
        }
    }

    private IEnumerator PlayBonusSoundDelayed()
    {
        yield return new WaitForSecondsRealtime(0.6f);
        if (bonusSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(bonusSound, soundVolume * 0.8f);
            if (enableDebugLogs)
                Debug.Log("🔊 Played bonus sound");
        }
    }

    private IEnumerator FadeIn()
    {
        float timeElapsed = 0f;
        canvasGroup.alpha = 0f;

        while (timeElapsed < fadeInDuration)
        {
            timeElapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, timeElapsed / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        float timeElapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while (timeElapsed < fadeOutDuration)
        {
            timeElapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, timeElapsed / fadeOutDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }

    private IEnumerator ScaleAnimation()
    {
        Vector3 targetScale = originalScale * (1f + scaleAnimationIntensity);
        float animationTime = fadeInDuration * 0.5f;

        // Scale up
        float timeElapsed = 0f;
        while (timeElapsed < animationTime && isShowing)
        {
            timeElapsed += Time.unscaledDeltaTime;
            float progress = timeElapsed / animationTime;
            notificationPanel.transform.localScale = Vector3.Lerp(originalScale, targetScale, progress);
            yield return null;
        }

        // Scale down
        timeElapsed = 0f;
        while (timeElapsed < animationTime && isShowing)
        {
            timeElapsed += Time.unscaledDeltaTime;
            float progress = timeElapsed / animationTime;
            notificationPanel.transform.localScale = Vector3.Lerp(targetScale, originalScale, progress);
            yield return null;
        }

        notificationPanel.transform.localScale = originalScale;
    }

    private IEnumerator GlowEffect(Color glowColor)
    {
        if (levelTitleText == null) yield break;

        Color originalColor = levelTitleText.color;
        Color glowColorBright = glowColor;
        glowColorBright.a = 1f;

        while (isShowing)
        {
            // Glow brighter
            float timeElapsed = 0f;
            float glowDuration = 1f / glowSpeed;

            while (timeElapsed < glowDuration && isShowing)
            {
                timeElapsed += Time.unscaledDeltaTime;
                float progress = timeElapsed / glowDuration;
                levelTitleText.color = Color.Lerp(originalColor, glowColorBright, Mathf.Sin(progress * Mathf.PI));
                yield return null;
            }

            yield return null;
        }

        // Reset to original color
        if (levelTitleText != null)
            levelTitleText.color = originalColor;
    }

    private IEnumerator CloseNotificationCoroutine()
    {
        if (enableDebugLogs)
            Debug.Log("🌟 Closing level notification");

        // Stop glow effect
        if (glowCoroutine != null)
        {
            StopCoroutine(glowCoroutine);
            glowCoroutine = null;
        }

        // Fade out
        yield return StartCoroutine(FadeOut());

        // Hide and cleanup
        HideNotificationImmediate();

        // Resume game
        if (pauseGameDuringNotification)
        {
            Time.timeScale = 1f;
            if (enableDebugLogs)
                Debug.Log("▶️ Game resumed");
        }

        isShowing = false;

        if (enableDebugLogs)
            Debug.Log("✅ Level notification closed successfully");
    }

    private void HideNotificationImmediate()
    {
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
            notificationPanel.transform.localScale = originalScale;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = originalBackgroundColor;
        }
    }

    private void OnContinueButtonClicked()
    {
        if (enableDebugLogs)
            Debug.Log("🎮 Continue button clicked");

        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound, soundVolume);
        }

        CloseNotification();
    }

    // ===== DEBUG METHODS =====

    public void DebugShowRandomNotification()
    {
        if (levelConfigs != null && levelConfigs.Length > 0)
        {
            int randomIndex = Random.Range(0, levelConfigs.Length);
            ShowLevelNotification(levelConfigs[randomIndex].sceneName);
        }
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugLogConfigurations()
    {
        Debug.Log("🔍 === LEVEL NOTIFICATION CONFIGURATIONS ===");

        if (levelConfigs != null)
        {
            for (int i = 0; i < levelConfigs.Length; i++)
            {
                LevelConfig config = levelConfigs[i];
                Debug.Log($"Config {i}: {config.levelName} ({config.sceneName})");
                Debug.Log($"  - Bonuses: {config.bonuses.Length}");
                Debug.Log($"  - Color: {config.levelColor}");
                Debug.Log($"  - Motivation: {config.motivationText}");
            }
        }
        else
        {
            Debug.LogWarning("❌ No level configurations found!");
        }

        Debug.Log("================================================");
    }

    void OnDestroy()
    {
        if (enableDebugLogs)
            Debug.Log("🗑️ LevelNotificationManager destroyed");
    }

    // ===== UTILITY METHODS =====

    public void SetVolume(float volume)
    {
        soundVolume = Mathf.Clamp01(volume);
        if (audioSource != null)
            audioSource.volume = soundVolume;
    }

    public void SetDisplayDuration(float duration)
    {
        displayDuration = Mathf.Max(0.5f, duration);
    }

    public void EnablePauseOnShow(bool enable)
    {
        pauseGameDuringNotification = enable;
    }

    public void EnableAutoClose(bool enable)
    {
        autoCloseAfterDuration = enable;
    }
}