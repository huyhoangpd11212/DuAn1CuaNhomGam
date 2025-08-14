using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "Scene1";
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string nextLevelSceneName = "Scene2";
    [SerializeField] private string level2SceneName = "Scene2";
    [SerializeField] private string level3SceneName = "Scene3";
    [SerializeField] private string bossLevelSceneName = "BossLevel";

    [Header("Game Settings")]
    [SerializeField] private bool resetScoreOnLoad = false;
    [SerializeField] private bool carryScoreBetweenLevels = true;

    [Header("Level Progression")]
    [SerializeField] private int level1RequiredScore = 1000;
    [SerializeField] private int level2RequiredScore = 2000;
    [SerializeField] private int bossLevelRequiredScore = 3000;

    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool showScoreInGUI = true;

    [Header("Score Management")]
    [SerializeField] private bool autoUpdateUI = true;
    [SerializeField] private float uiUpdateRate = 0.1f;

    [Header("Victory Settings")]
    [SerializeField] private float victoryScreenDuration = 5f;
    [SerializeField] private bool showVictoryLog = true;
    [SerializeField] private bool autoUpdateVictoryScreen = true; // ✅ NEW

    [Header("Reset Settings")]
    [SerializeField] private bool confirmResetScore = false;
    [SerializeField] private bool resetScoreKeepsUI = true;
    [SerializeField] private bool forceUIRefreshOnReset = true;
    [SerializeField] private bool allowInstantReset = true;

    public static GameManager instance;

    // Events
    public static System.Action<string> OnSceneLoading;
    public static System.Action<int> OnScoreChanged;
    public static System.Action OnGameWin;
    public static System.Action OnGameOver;
    public static System.Action OnScoreReset;

    // Score tracking
    private int lastKnownScore = 0;
    private float lastUIUpdateTime = 0f;

    // Enhanced state tracking
    private bool gameCompleted = false;
    private string lastLoadedScene = "";
    private int resetCallCount = 0;
    private float lastResetTime = 0f;

    void Awake()
    {
        // Enhanced Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            if (enableDebugLogs)
                Debug.Log("🎮 GameManager initialized as singleton - Instance: " + (instance != null));
        }
        else if (instance != this)
        {
            if (enableDebugLogs)
                Debug.Log("🎮 Duplicate GameManager found - Destroying: " + gameObject.name);
            Destroy(gameObject);
            return;
        }

        // Verify instance is properly set
        if (instance == null)
        {
            Debug.LogError("❌ CRITICAL: GameManager.instance failed to initialize!");
            instance = this;
        }
        else
        {
            if (enableDebugLogs)
                Debug.Log("✅ GameManager.instance verified: " + instance.name);
        }
    }

    void Start()
    {
        InitializeGame();
        lastKnownScore = PlayerController.currentScore;
        ValidateGameManagerInstance();
    }

    void Update()
    {
        try
        {
            // Auto update UI when score changes
            if (autoUpdateUI)
            {
                CheckAndUpdateScore();
            }

            // Debug controls
            if (enableDebugLogs)
            {
                if (Input.GetKeyDown(KeyCode.F1))
                    DebugPrintGameState();

                if (Input.GetKeyDown(KeyCode.F2))
                    TestAllConnections();
            }

            // Score testing in editor
            if (Application.isEditor)
            {
                if (Input.GetKeyDown(KeyCode.T))
                {
                    AddScore(100);
                    Debug.Log("🧪 TEST: Added 100 points via GameManager");
                }

                if (Input.GetKeyDown(KeyCode.Y))
                {
                    SetScore(0);
                    Debug.Log("🧪 TEST: Reset score to 0");
                }

                if (Input.GetKeyDown(KeyCode.H))
                {
                    SetScore(5000);
                    Debug.Log("🧪 TEST: Set high score (5000)");
                }

                // Enhanced reset score test keys
                if (Input.GetKeyDown(KeyCode.R))
                {
                    ResetScore();
                    Debug.Log("🧪 TEST: Reset score via R key");
                }

                if (Input.GetKeyDown(KeyCode.Z))
                {
                    ResetScoreAndRestart();
                    Debug.Log("🧪 TEST: Reset score and restart via Z key");
                }

                if (Input.GetKeyDown(KeyCode.X))
                {
                    ResetScoreOnly();
                    Debug.Log("🧪 TEST: Reset score only via X key");
                }

                // Multiple reset testing
                if (Input.GetKeyDown(KeyCode.U))
                {
                    TestMultipleResets();
                    Debug.Log("🧪 TEST: Multiple reset sequence started");
                }

                if (Input.GetKeyDown(KeyCode.I))
                {
                    QuickResetTest();
                }

                // Reset test
                if (Input.GetKeyDown(KeyCode.P))
                {
                    ForceResetScore();
                    Debug.Log("🧪 TEST: Force reset score (no restrictions)");
                }

                // ✅ NEW: Victory screen test keys
                if (Input.GetKeyDown(KeyCode.V))
                {
                    TriggerGameWin();
                    Debug.Log("🧪 TEST: Trigger victory screen with current score: " + PlayerController.currentScore);
                }

                if (Input.GetKeyDown(KeyCode.N))
                {
                    UpdateVictoryScreen();
                    Debug.Log("🧪 TEST: Manually update victory screen score");
                }

                // Quick level switching
                if (Input.GetKeyDown(KeyCode.Alpha1)) LoadGameScene();
                if (Input.GetKeyDown(KeyCode.Alpha2)) LoadLevel2();
                if (Input.GetKeyDown(KeyCode.Alpha3)) LoadLevel3();
                if (Input.GetKeyDown(KeyCode.B)) LoadBossLevel();
                if (Input.GetKeyDown(KeyCode.M)) LoadMainMenu();
            }
        }
        catch (System.Exception e)
        {
            if (enableDebugLogs)
                Debug.LogError("❌ GameManager Update error: " + e.Message);
        }
    }

    void OnGUI()
    {
        if (showScoreInGUI && Application.isEditor)
        {
            GUI.Box(new Rect(10, 10, 350, 320), ""); // ✅ Increased height for victory info
            GUI.Label(new Rect(15, 15, 340, 20), "GAMEMANAGER DEBUG");
            GUI.Label(new Rect(15, 35, 340, 20), "Current Score: " + PlayerController.currentScore);
            GUI.Label(new Rect(15, 55, 340, 20), "High Score: " + GetHighScore());
            GUI.Label(new Rect(15, 75, 340, 20), "Current Scene: " + SceneManager.GetActiveScene().name);
            GUI.Label(new Rect(15, 95, 340, 20), "Instance Valid: " + (instance != null ? "✅" : "❌"));
            GUI.Label(new Rect(15, 115, 340, 20), "GameStateManager: " + (GameStateManager.instance != null ? "✅" : "❌"));
            GUI.Label(new Rect(15, 135, 340, 20), "Can Progress L2: " + CanProgressToLevel2());
            GUI.Label(new Rect(15, 155, 340, 20), "Reset Count: " + resetCallCount + " | Last: " + lastResetTime.ToString("F1"));
            GUI.Label(new Rect(15, 175, 340, 20), "Game Completed: " + (gameCompleted ? "✅" : "❌")); // ✅ NEW
            GUI.Label(new Rect(15, 195, 340, 20), "F1: Debug, F2: Test, T: +100, Y: Reset, H: 5000");
            GUI.Label(new Rect(15, 215, 340, 20), "R: Reset, Z: Reset+Restart, X: Reset Only");
            GUI.Label(new Rect(15, 235, 340, 20), "U: Multi Reset, I: Quick Reset, V: Victory"); // ✅ NEW
            GUI.Label(new Rect(15, 255, 340, 20), "P: Force Reset, N: Update Victory, 1-3: Levels"); // ✅ NEW
            GUI.Label(new Rect(15, 275, 340, 20), "B: Boss, M: Menu");
            GUI.Label(new Rect(15, 295, 340, 20), "Allow Instant Reset: " + (allowInstantReset ? "✅" : "❌"));
        }
    }

    // ===== VALIDATION METHODS =====
    private void ValidateGameManagerInstance()
    {
        if (instance != this)
        {
            Debug.LogError("❌ GameManager instance mismatch! Expected: " + this.name + ", Got: " + (instance ? instance.name : "null"));
            instance = this;
        }

        if (enableDebugLogs)
            Debug.Log("✅ GameManager validation complete. Instance: " + (instance != null));
    }

    private void TestAllConnections()
    {
        Debug.Log("🧪 === CONNECTION TEST ===");
        Debug.Log("GameManager.instance: " + (instance != null ? "✅" : "❌"));
        Debug.Log("GameStateManager.instance: " + (GameStateManager.instance != null ? "✅" : "❌"));
        Debug.Log("PlayerController.currentScore: " + PlayerController.currentScore);

        if (GameStateManager.instance != null)
        {
            GameStateManager.instance.ForceUpdateHUD();
            Debug.Log("✅ GameStateManager UI force updated");
        }

        Debug.Log("========================");
    }

    // ===== ENHANCED SCORE MANAGEMENT =====
    private void CheckAndUpdateScore()
    {
        if (Time.time - lastUIUpdateTime >= uiUpdateRate)
        {
            if (PlayerController.currentScore != lastKnownScore)
            {
                lastKnownScore = PlayerController.currentScore;
                OnScoreChanged?.Invoke(PlayerController.currentScore);

                ForceUpdateAllUI();

                if (enableDebugLogs)
                    Debug.Log("💰 Score changed detected: " + PlayerController.currentScore);
            }

            lastUIUpdateTime = Time.time;
        }
    }

    public void ForceUpdateAllUI()
    {
        try
        {
            if (GameStateManager.instance != null)
            {
                GameStateManager.instance.ForceUpdateHUD();
                if (enableDebugLogs)
                    Debug.Log("✅ GameStateManager UI updated");
            }
            else
            {
                GameStateManager gsm = FindObjectOfType<GameStateManager>();
                if (gsm != null)
                {
                    gsm.ForceUpdateHUD();
                    if (enableDebugLogs)
                        Debug.Log("✅ Found and updated GameStateManager UI");
                }
            }

            UpdateScoreUI();
            OnScoreChanged?.Invoke(PlayerController.currentScore);
        }
        catch (System.Exception e)
        {
            if (enableDebugLogs)
                Debug.LogError("❌ ForceUpdateAllUI error: " + e.Message);
        }
    }

    private void UpdateScoreUI()
    {
        try
        {
            UnityEngine.UI.Text[] allTexts = FindObjectsOfType<UnityEngine.UI.Text>();
            int updatedCount = 0;

            foreach (UnityEngine.UI.Text text in allTexts)
            {
                if (text != null && text.gameObject.activeInHierarchy)
                {
                    string textName = text.name.ToLower();

                    if (textName.Contains("score") && !textName.Contains("high"))
                    {
                        string oldText = text.text;
                        text.text = "SCORE: " + PlayerController.currentScore.ToString();
                        updatedCount++;

                        if (enableDebugLogs)
                            Debug.Log("✅ Updated score UI: " + text.name + " = " + text.text);
                    }

                    if (textName.Contains("final") && textName.Contains("score"))
                    {
                        text.text = "Final Score: " + PlayerController.currentScore.ToString();
                        updatedCount++;
                    }
                }
            }

            if (enableDebugLogs && updatedCount > 0)
                Debug.Log("✅ Updated " + updatedCount + " score text elements");
        }
        catch (System.Exception e)
        {
            if (enableDebugLogs)
                Debug.LogError("❌ UpdateScoreUI error: " + e.Message);
        }
    }

    public void AddScore(int points)
    {
        PlayerController.AddScore(points);
        ForceUpdateAllUI();

        if (enableDebugLogs)
            Debug.Log("💰 GameManager: Added " + points + " points. Total: " + PlayerController.currentScore);
    }

    public void SetScore(int newScore)
    {
        PlayerController.currentScore = newScore;
        lastKnownScore = newScore;
        ForceUpdateAllUI();

        if (enableDebugLogs)
            Debug.Log("💰 GameManager: Set score to " + newScore);
    }

    // ===== FIXED RESET SCORE METHODS =====
    public void ResetScore()
    {
        if (!allowInstantReset && Time.time - lastResetTime < 0.1f)
        {
            if (enableDebugLogs)
                Debug.Log("⚠️ Reset spam protection active - use ForceResetScore() to bypass");
            return;
        }

        ExecuteResetScore();
    }

    public void ForceResetScore()
    {
        ExecuteResetScore();
    }

    private void ExecuteResetScore()
    {
        lastResetTime = Time.time;
        resetCallCount++;

        if (enableDebugLogs)
            Debug.Log("🔄 === RESET SCORE START === (Call #" + resetCallCount + " at " + Time.time.ToString("F2") + "s)");

        int oldScore = PlayerController.currentScore;

        if (enableDebugLogs)
            Debug.Log("🔄 Score trước khi reset: " + oldScore);

        try
        {
            // Triple reset method để đảm bảo reset thành công
            PlayerController.currentScore = 0;
            lastKnownScore = 0;

            // Verify and force if needed
            if (PlayerController.currentScore != 0)
            {
                if (enableDebugLogs)
                    Debug.LogWarning("⚠️ First reset failed, forcing again...");
                PlayerController.currentScore = 0;
            }

            // Final verification
            if (PlayerController.currentScore != 0)
            {
                Debug.LogError("❌ Score reset failed twice! Attempting emergency reset...");
                for (int i = 0; i < 5; i++)
                {
                    PlayerController.currentScore = 0;
                }
            }

            if (enableDebugLogs)
                Debug.Log("🔄 Score sau khi reset: " + PlayerController.currentScore);

            // Trigger events immediately
            OnScoreChanged?.Invoke(0);
            OnScoreReset?.Invoke();

            if (enableDebugLogs)
                Debug.Log("🔄 Events triggered: OnScoreChanged(0), OnScoreReset()");

            // Comprehensive UI update
            if (resetScoreKeepsUI)
            {
                ForceUpdateAllUISystems();
                StartCoroutine(DelayedResetUIUpdate());
            }

            if (enableDebugLogs)
            {
                Debug.Log("🔄 Final verification - PlayerController.currentScore: " + PlayerController.currentScore);
                Debug.Log("🔄 Final verification - lastKnownScore: " + lastKnownScore);
                Debug.Log("✅ === RESET SCORE COMPLETE ===");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Reset score failed: " + e.Message);

            // Enhanced emergency fallback
            for (int i = 0; i < 3; i++)
            {
                PlayerController.currentScore = 0;
                lastKnownScore = 0;
            }

            ForceUpdateAllUISystems();
            StartCoroutine(DelayedResetUIUpdate());
        }
    }

    private System.Collections.IEnumerator DelayedResetUIUpdate()
    {
        // Update immediately
        ForceUpdateAllUISystems();

        // Wait and update again
        yield return new WaitForSeconds(0.1f);

        // Final verification and update
        if (PlayerController.currentScore != 0)
        {
            Debug.LogWarning("⚠️ Score still not 0 after delay! Final emergency reset...");
            PlayerController.currentScore = 0;
            lastKnownScore = 0;
        }

        ForceUpdateAllUISystems();
        OnScoreChanged?.Invoke(PlayerController.currentScore);

        // One more delayed update for good measure
        yield return new WaitForSeconds(0.1f);
        ForceUpdateAllUISystems();

        if (enableDebugLogs)
            Debug.Log("🔄 Delayed reset UI update completed - Final Score: " + PlayerController.currentScore);
    }

    // ===== ENHANCED: Comprehensive UI system update =====
    private void ForceUpdateAllUISystems()
    {
        try
        {
            if (enableDebugLogs)
                Debug.Log("🔄 ForceUpdateAllUISystems started... (Score: " + PlayerController.currentScore + ")");

            UpdateGameStateManager();
            UpdateAllScoreTexts();
            OnScoreChanged?.Invoke(PlayerController.currentScore);

            if (forceUIRefreshOnReset)
            {
                RefreshScoreGameObjects();
            }

            if (enableDebugLogs)
                Debug.Log("✅ ForceUpdateAllUISystems completed");
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ ForceUpdateAllUISystems failed: " + e.Message);

            // Minimal fallback
            try
            {
                UpdateGameStateManager();
                OnScoreChanged?.Invoke(PlayerController.currentScore);
            }
            catch
            {
                Debug.LogError("❌ Even minimal UI update failed!");
            }
        }
    }

    private void UpdateGameStateManager()
    {
        try
        {
            if (GameStateManager.instance != null)
            {
                GameStateManager.instance.ForceUpdateHUD();
                if (enableDebugLogs)
                    Debug.Log("✅ GameStateManager.ForceUpdateHUD() called");
            }
            else
            {
                GameStateManager gsm = FindObjectOfType<GameStateManager>();
                if (gsm != null)
                {
                    gsm.ForceUpdateHUD();
                    if (enableDebugLogs)
                        Debug.Log("✅ Found GameStateManager and called ForceUpdateHUD()");
                }
                else
                {
                    if (enableDebugLogs)
                        Debug.LogWarning("⚠️ No GameStateManager found for UI update");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ UpdateGameStateManager failed: " + e.Message);
        }
    }

    private void UpdateAllScoreTexts()
    {
        try
        {
            // Enhanced: Include inactive objects and use more comprehensive search
            UnityEngine.UI.Text[] allTexts = Resources.FindObjectsOfTypeAll<UnityEngine.UI.Text>();
            int updatedCount = 0;

            foreach (UnityEngine.UI.Text text in allTexts)
            {
                if (text != null && text.gameObject.scene.isLoaded) // Only scene objects
                {
                    string textName = text.name.ToLower();
                    string oldText = text.text;
                    bool wasUpdated = false;

                    if (textName.Contains("score") && !textName.Contains("high"))
                    {
                        text.text = "SCORE: " + PlayerController.currentScore.ToString();
                        wasUpdated = true;
                    }
                    else if (textName.Contains("final") && textName.Contains("score"))
                    {
                        text.text = "Final Score: " + PlayerController.currentScore.ToString();
                        wasUpdated = true;
                    }
                    else if (textName.Contains("point") && !textName.Contains("high"))
                    {
                        text.text = "Points: " + PlayerController.currentScore.ToString();
                        wasUpdated = true;
                    }
                    else if (IsScoreText(oldText))
                    {
                        text.text = ReplaceScoreInText(oldText, PlayerController.currentScore);
                        wasUpdated = true;
                    }

                    if (wasUpdated)
                    {
                        updatedCount++;

                        // Force refresh text component
                        text.enabled = false;
                        text.enabled = true;

                        if (enableDebugLogs)
                            Debug.Log("✅ Updated text '" + text.name + "': '" + oldText + "' → '" + text.text + "'");
                    }
                }
            }

            if (enableDebugLogs)
                Debug.Log("✅ Updated " + updatedCount + " score text elements");
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ UpdateAllScoreTexts failed: " + e.Message);
        }
    }

    private bool IsScoreText(string text)
    {
        if (string.IsNullOrEmpty(text)) return false;

        return text.Contains("SCORE") ||
               text.Contains("Score") ||
               text.Contains("Points") ||
               text.Contains("POINTS") ||
               (text.Contains(":") && System.Text.RegularExpressions.Regex.IsMatch(text, @"\d+"));
    }

    private string ReplaceScoreInText(string originalText, int newScore)
    {
        string result = originalText;

        result = System.Text.RegularExpressions.Regex.Replace(result, @"(SCORE:\s*)\d+", "$1" + newScore);
        result = System.Text.RegularExpressions.Regex.Replace(result, @"(Score:\s*)\d+", "$1" + newScore);
        result = System.Text.RegularExpressions.Regex.Replace(result, @"(POINTS:\s*)\d+", "$1" + newScore);
        result = System.Text.RegularExpressions.Regex.Replace(result, @"(Points:\s*)\d+", "$1" + newScore);

        if (result == originalText)
        {
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\d+", newScore.ToString());
        }

        return result;
    }

    private void RefreshScoreGameObjects()
    {
        try
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            int refreshedCount = 0;

            foreach (GameObject obj in allObjects)
            {
                if (obj != null && obj.name.ToLower().Contains("score"))
                {
                    bool wasActive = obj.activeInHierarchy;
                    if (wasActive)
                    {
                        obj.SetActive(false);
                        obj.SetActive(true);
                        refreshedCount++;

                        if (enableDebugLogs)
                            Debug.Log("🔄 Refreshed GameObject: " + obj.name);
                    }
                }
            }

            if (enableDebugLogs && refreshedCount > 0)
                Debug.Log("🔄 Refreshed " + refreshedCount + " score GameObjects");
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ RefreshScoreGameObjects failed: " + e.Message);
        }
    }

    // ===== ENHANCED: Reset methods for different use cases =====
    public void ResetScoreAndRestart()
    {
        if (enableDebugLogs)
            Debug.Log("🔄 Reset score và restart level...");

        ForceResetScore();
        RestartCurrentLevel();

        if (enableDebugLogs)
            Debug.Log("🔄 Score reset và restart level hoàn thành");
    }

    public void ResetScoreOnly()
    {
        ForceResetScore();

        if (enableDebugLogs)
            Debug.Log("🔄 Chỉ reset score, không restart level");
    }

    public void ResetScoreWithConfirmation()
    {
        if (confirmResetScore)
        {
            if (enableDebugLogs)
                Debug.Log("🤔 Reset score cần xác nhận (trong thực tế nên có UI)");
        }

        ForceResetScore();
    }

    public void ResetScoreAndLoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("❌ Scene name không hợp lệ cho ResetScoreAndLoadScene!");
            return;
        }

        if (enableDebugLogs)
            Debug.Log("🔄 Reset score và load scene: " + sceneName);

        ForceResetScore();
        LoadScene(sceneName);
    }

    public void ResetScoreForButton()
    {
        if (enableDebugLogs)
            Debug.Log("🔄 ResetScoreForButton called from UI button (Time: " + Time.time.ToString("F2") + "s)");

        ForceResetScore();
        StartCoroutine(ButtonResetUIRefresh());
    }

    private System.Collections.IEnumerator ButtonResetUIRefresh()
    {
        // Immediate update
        ForceUpdateAllUISystems();

        // Wait a bit
        yield return new WaitForSeconds(0.05f);

        // Second update
        ForceUpdateAllUISystems();
        OnScoreChanged?.Invoke(PlayerController.currentScore);

        // Final update after a bit more time
        yield return new WaitForSeconds(0.1f);

        // Final verification
        if (PlayerController.currentScore != 0)
        {
            Debug.LogWarning("⚠️ Button reset: Score still not 0, final attempt!");
            PlayerController.currentScore = 0;
            lastKnownScore = 0;
        }

        ForceUpdateAllUISystems();
        OnScoreChanged?.Invoke(PlayerController.currentScore);

        if (enableDebugLogs)
            Debug.Log("🔄 Button reset UI refresh completed - Score: " + PlayerController.currentScore);
    }

    public void OnResetButtonClick()
    {
        if (enableDebugLogs)
            Debug.Log("🔄 Reset button được nhấn từ UI");

        ForceResetScore();
    }

    // ===== INITIALIZATION =====
    private void InitializeGame()
    {
        if (enableDebugLogs)
            Debug.Log("🎮 Game initialized. Current scene: " + SceneManager.GetActiveScene().name);

        Invoke("DelayedInitialUpdate", 0.2f);
    }

    private void DelayedInitialUpdate()
    {
        ForceUpdateAllUI();
        if (enableDebugLogs)
            Debug.Log("🎮 Delayed initial UI update completed");
    }

    // ===== SCENE LOADING METHODS =====
    public void LoadGameScene()
    {
        if (resetScoreOnLoad)
        {
            PlayerController.currentScore = 0;
            OnScoreChanged?.Invoke(PlayerController.currentScore);
        }

        ResetGameState();
        LoadScene(gameSceneName);

        if (enableDebugLogs)
            Debug.Log("🎮 Loading Game Scene: " + gameSceneName + " | Score: " + PlayerController.currentScore);
    }

    public void LoadLevel2()
    {
        if (!carryScoreBetweenLevels)
        {
            PlayerController.currentScore = 0;
            OnScoreChanged?.Invoke(PlayerController.currentScore);
        }

        ResetGameState();
        LoadScene(level2SceneName);

        if (enableDebugLogs)
            Debug.Log("🎮 Loading Level 2 with score: " + PlayerController.currentScore);
    }

    public void LoadLevel3()
    {
        if (!carryScoreBetweenLevels)
        {
            PlayerController.currentScore = 0;
            OnScoreChanged?.Invoke(PlayerController.currentScore);
        }

        ResetGameState();
        LoadScene(level3SceneName);

        if (enableDebugLogs)
            Debug.Log("🎮 Loading Level 3 with score: " + PlayerController.currentScore);
    }

    public void LoadBossLevel()
    {
        ResetGameState();
        LoadScene(bossLevelSceneName);

        if (enableDebugLogs)
            Debug.Log("🎮 Loading Boss Level with score: " + PlayerController.currentScore);
    }

    public void LoadNextLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name.ToLower();

        if (enableDebugLogs)
            Debug.Log("🎮 LoadNextLevel called from: " + currentScene + " | Score: " + PlayerController.currentScore);

        if (!carryScoreBetweenLevels)
        {
            PlayerController.currentScore = 0;
            OnScoreChanged?.Invoke(PlayerController.currentScore);
        }

        ResetGameState();

        switch (currentScene)
        {
            case "scene1":
            case "level1":
            case "menuscene":
                LoadScene(level2SceneName);
                if (enableDebugLogs)
                    Debug.Log("🎮 Progressing: Level 1 → Level 2");
                break;

            case "scene2":
            case "level2":
                if (PlayerController.currentScore >= bossLevelRequiredScore)
                {
                    LoadScene(bossLevelSceneName);
                    if (enableDebugLogs)
                        Debug.Log("🎮 Progressing: Level 2 → Boss Level (High Score Bonus!)");
                }
                else
                {
                    LoadScene(level3SceneName);
                    if (enableDebugLogs)
                        Debug.Log("🎮 Progressing: Level 2 → Level 3");
                }
                break;

            case "scene3":
            case "level3":
                LoadScene(bossLevelSceneName);
                if (enableDebugLogs)
                    Debug.Log("🎮 Progressing: Level 3 → Boss Level");
                break;

            case "bosslevel":
            case "finallevel":
                ShowGameCompletedScreen();
                break;

            default:
                if (!string.IsNullOrEmpty(nextLevelSceneName))
                {
                    LoadScene(nextLevelSceneName);
                    if (enableDebugLogs)
                        Debug.Log("🎮 Fallback progression to: " + nextLevelSceneName);
                }
                else
                {
                    RestartCurrentLevel();
                }
                break;
        }
    }

    public void LoadMainMenu()
    {
        ResetGameState();
        SaveHighScore();
        LoadScene(mainMenuSceneName);

        if (enableDebugLogs)
            Debug.Log("🎮 Loading Main Menu | Final Score: " + PlayerController.currentScore);
    }

    public void RestartCurrentLevel()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (resetScoreOnLoad && !carryScoreBetweenLevels)
        {
            int scoreBeforeReset = PlayerController.currentScore;
            PlayerController.currentScore = 0;
            OnScoreChanged?.Invoke(PlayerController.currentScore);

            if (enableDebugLogs)
                Debug.Log("🎮 Score reset for restart: " + scoreBeforeReset + " → 0");
        }

        ResetGameState();
        LoadScene(currentSceneName);

        if (enableDebugLogs)
            Debug.Log("🎮 Restarting current level: " + currentSceneName + " | Score: " + PlayerController.currentScore);
    }

    public void LoadSpecificScene(string sceneName, bool keepScore = true)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("❌ Scene name không hợp lệ!");
            return;
        }

        if (enableDebugLogs)
            Debug.Log("🎮 Loading specific scene: " + sceneName + " | Keep Score: " + keepScore);

        if (!keepScore)
        {
            PlayerController.currentScore = 0;
            OnScoreChanged?.Invoke(PlayerController.currentScore);
        }

        ResetGameState();
        LoadScene(sceneName);
    }

    public void LoadSceneViaButton(string sceneName)
    {
        LoadSpecificScene(sceneName, carryScoreBetweenLevels);
    }

    // ===== GAME STATE MANAGEMENT =====
    private void ResetGameState()
    {
        Time.timeScale = 1f;
        PlayerController.isGamePaused = false;
        gameCompleted = false;

        if (enableDebugLogs)
            Debug.Log("🎮 Game state reset - Time: 1.0, Paused: false");
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        PlayerController.isGamePaused = true;

        if (enableDebugLogs)
            Debug.Log("⏸️ Game Paused");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        PlayerController.isGamePaused = false;

        if (enableDebugLogs)
            Debug.Log("▶️ Game Resumed");
    }

    // ===== ✅ VICTORY AND GAME COMPLETION WITH SCORE DISPLAY =====
    private void ShowGameCompletedScreen()
    {
        if (gameCompleted) return;

        gameCompleted = true;

        // ✅ CẬP NHẬT VICTORY SCREEN SCORE
        if (autoUpdateVictoryScreen)
        {
            UpdateVictoryScreenScore();
        }

        if (showVictoryLog)
        {
            Debug.Log("🏆 ===============================");
            Debug.Log("🏆 CONGRATULATIONS! GAME COMPLETED!");
            Debug.Log("🏆 Final Score: " + PlayerController.currentScore);
            Debug.Log("🏆 Previous High Score: " + GetHighScore());
            Debug.Log("🏆 ===============================");
        }

        SaveHighScore();
        OnGameWin?.Invoke();

        if (victoryScreenDuration > 0)
        {
            Invoke("ReturnToMainMenuAfterVictory", victoryScreenDuration);
        }

        if (enableDebugLogs)
            Debug.Log("🏆 Victory screen will last " + victoryScreenDuration + " seconds");
    }

    // ✅ NEW METHOD: Update Victory Screen Score
    private void UpdateVictoryScreenScore()
    {
        try
        {
            if (enableDebugLogs)
                Debug.Log("🏆 Updating victory screen score: " + PlayerController.currentScore);

            // Tìm tất cả Text components trong scene
            UnityEngine.UI.Text[] allTexts = Resources.FindObjectsOfTypeAll<UnityEngine.UI.Text>();
            int updatedCount = 0;

            foreach (UnityEngine.UI.Text text in allTexts)
            {
                if (text != null && text.gameObject.scene.isLoaded)
                {
                    string textName = text.name.ToLower();
                    string parentName = "";

                    // Kiểm tra parent objects để tìm Victory Panel
                    Transform parent = text.transform.parent;
                    while (parent != null)
                    {
                        parentName += parent.name.ToLower() + " ";
                        if (parentName.Contains("victory") || parentName.Contains("win") ||
                            parentName.Contains("complete") || parentName.Contains("end"))
                        {
                            break;
                        }
                        parent = parent.parent;
                    }

                    // Cập nhật score text trong Victory Panel hoặc có tên liên quan
                    bool isVictoryText = parentName.Contains("victory") || parentName.Contains("win") ||
                                       parentName.Contains("complete") || parentName.Contains("end") ||
                                       textName.Contains("victory") || textName.Contains("final") ||
                                       textName.Contains("complete");

                    if (isVictoryText || textName.Contains("score"))
                    {
                        string oldText = text.text;

                        // Update based on text name and content
                        if (textName.Contains("score") && !textName.Contains("high"))
                        {
                            text.text = "FINAL SCORE: " + PlayerController.currentScore.ToString();
                            updatedCount++;
                        }
                        else if (textName.Contains("point"))
                        {
                            text.text = "POINTS: " + PlayerController.currentScore.ToString();
                            updatedCount++;
                        }
                        else if (textName.Contains("final"))
                        {
                            text.text = "Final Score: " + PlayerController.currentScore.ToString();
                            updatedCount++;
                        }
                        // Cập nhật text có chứa số
                        else if (System.Text.RegularExpressions.Regex.IsMatch(text.text, @"\d+"))
                        {
                            text.text = System.Text.RegularExpressions.Regex.Replace(text.text, @"\d+", PlayerController.currentScore.ToString());
                            updatedCount++;
                        }
                        // Fallback: nếu text trống hoặc placeholder
                        else if (string.IsNullOrEmpty(text.text) || text.text.Contains("000") || text.text.Contains("Score"))
                        {
                            text.text = "SCORE: " + PlayerController.currentScore.ToString();
                            updatedCount++;
                        }

                        // Force refresh text component
                        if (oldText != text.text)
                        {
                            text.enabled = false;
                            text.enabled = true;

                            if (enableDebugLogs)
                                Debug.Log("🏆 Updated victory text '" + text.name + "': '" + oldText + "' → '" + text.text + "'");
                        }
                    }

                    // Cập nhật high score text nếu có
                    if ((textName.Contains("high") && textName.Contains("score")) || textName.Contains("best"))
                    {
                        int currentHighScore = GetHighScore();
                        int newHighScore = Mathf.Max(currentHighScore, PlayerController.currentScore);

                        text.text = "HIGH SCORE: " + newHighScore.ToString();
                        text.enabled = false;
                        text.enabled = true;
                        updatedCount++;

                        if (enableDebugLogs)
                            Debug.Log("🏆 Updated high score text: " + text.name + " = " + text.text);
                    }
                }
            }

            // ✅ ADDITIONAL: Tìm và update bất kỳ text nào có "0000" hoặc placeholder
            if (updatedCount == 0)
            {
                UpdatePlaceholderScoreTexts();
            }

            if (enableDebugLogs)
                Debug.Log("🏆 Victory screen score update completed - Updated: " + updatedCount + " texts");

            // ✅ TRIGGER UI EVENTS
            OnScoreChanged?.Invoke(PlayerController.currentScore);
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ UpdateVictoryScreenScore failed: " + e.Message);
        }
    }

    // ✅ FALLBACK METHOD: Update placeholder texts
    private void UpdatePlaceholderScoreTexts()
    {
        try
        {
            UnityEngine.UI.Text[] allTexts = FindObjectsOfType<UnityEngine.UI.Text>();

            foreach (UnityEngine.UI.Text text in allTexts)
            {
                if (text != null && text.gameObject.activeInHierarchy)
                {
                    string textContent = text.text;

                    // Tìm text có placeholder values
                    if (textContent.Contains("0000") || textContent.Contains("000") ||
                        textContent.Contains("9999") || textContent.Contains("XXX") ||
                        string.IsNullOrEmpty(textContent) || textContent == "Score")
                    {
                        text.text = "SCORE: " + PlayerController.currentScore.ToString();
                        text.enabled = false;
                        text.enabled = true;

                        if (enableDebugLogs)
                            Debug.Log("🏆 Updated placeholder text: " + text.name + " = " + text.text);
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ UpdatePlaceholderScoreTexts failed: " + e.Message);
        }
    }

    // ✅ PUBLIC METHOD: Manually trigger victory screen update
    public void UpdateVictoryScreen()
    {
        UpdateVictoryScreenScore();
    }

    // ✅ ENHANCED: Trigger game win with score update
    public void TriggerGameWin()
    {
        ShowGameCompletedScreen();

        // ✅ DELAYED UPDATE để đảm bảo UI đã load
        Invoke("UpdateVictoryScreenScore", 0.2f);
    }

    // ✅ METHOD for external scripts to trigger victory with score
    public void ShowVictoryWithScore(int finalScore)
    {
        PlayerController.currentScore = finalScore;
        lastKnownScore = finalScore;
        ShowGameCompletedScreen();
    }

    private void ReturnToMainMenuAfterVictory()
    {
        if (enableDebugLogs)
            Debug.Log("🏆 Victory screen timeout - returning to main menu");
        LoadMainMenu();
    }

    public void TriggerGameOver()
    {
        SaveHighScore();
        OnGameOver?.Invoke();

        if (enableDebugLogs)
            Debug.Log("💀 Game Over triggered | Final Score: " + PlayerController.currentScore);
    }

    // ===== SCENE LOADING WITH ERROR HANDLING =====
    private void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("❌ Scene name is null or empty!");
            return;
        }

        lastLoadedScene = sceneName;
        OnSceneLoading?.Invoke(sceneName);

        try
        {
            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SceneManager.LoadScene(sceneName);
                if (enableDebugLogs)
                    Debug.Log("✅ Loading scene: " + sceneName);
            }
            else
            {
                Debug.LogError("❌ Scene '" + sceneName + "' not found in Build Settings!");
                HandleSceneLoadError(sceneName);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Error loading scene '" + sceneName + "': " + e.Message);
            HandleSceneLoadError(sceneName);
        }
    }

    private void HandleSceneLoadError(string failedSceneName)
    {
        Debug.LogWarning("⚠️ Attempting fallback scene loading for: " + failedSceneName);

        string[] fallbackNames = GetFallbackSceneNames(failedSceneName);

        foreach (string fallback in fallbackNames)
        {
            try
            {
                if (Application.CanStreamedLevelBeLoaded(fallback))
                {
                    SceneManager.LoadScene(fallback);
                    if (enableDebugLogs)
                        Debug.Log("✅ Fallback scene loaded: " + fallback);
                    return;
                }
            }
            catch
            {
                continue;
            }
        }

        try
        {
            if (SceneManager.sceneCountInBuildSettings > 0)
            {
                SceneManager.LoadScene(0);
                if (enableDebugLogs)
                    Debug.Log("✅ Ultimate fallback: Loading first scene in Build Settings");
            }
            else
            {
                Debug.LogError("❌ No scenes found in Build Settings!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Ultimate fallback scene loading failed: " + e.Message);
        }
    }

    private string[] GetFallbackSceneNames(string originalName)
    {
        string lowerName = originalName.ToLower();

        if (lowerName.Contains("main") || lowerName.Contains("menu"))
        {
            return new string[] { "MainMenu", "Menu", "StartMenu", "Main" };
        }
        else if (lowerName.Contains("level") || lowerName.Contains("scene"))
        {
            return new string[] { "Scene1", "Level1", "Game", "GameScene" };
        }

        return new string[] { "Scene1", "MainMenu", "Game" };
    }

    // ===== HIGH SCORE SYSTEM =====
    private void SaveHighScore()
    {
        int currentHighScore = PlayerPrefs.GetInt("HighScore", 0);
        if (PlayerController.currentScore > currentHighScore)
        {
            PlayerPrefs.SetInt("HighScore", PlayerController.currentScore);
            PlayerPrefs.SetString("HighScoreDate", System.DateTime.Now.ToString());
            PlayerPrefs.Save();

            if (enableDebugLogs)
                Debug.Log("🏆 NEW HIGH SCORE: " + PlayerController.currentScore + " (Previous: " + currentHighScore + ")");
        }
        else
        {
            if (enableDebugLogs)
                Debug.Log("📊 Final Score: " + PlayerController.currentScore + " (High Score: " + currentHighScore + ")");
        }
    }

    public void QuitGame()
    {
        if (enableDebugLogs)
            Debug.Log("🔚 Quitting game...");

        SaveHighScore();
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        if (enableDebugLogs)
            Debug.Log("✅ Editor play mode stopped");
#else
        Application.Quit();
        if (enableDebugLogs)
            Debug.Log("✅ Application quit requested");
#endif
    }

    // ===== GETTER METHODS =====
    public string GetGameSceneName() { return gameSceneName; }
    public string GetMainMenuSceneName() { return mainMenuSceneName; }
    public string GetNextLevelSceneName() { return nextLevelSceneName; }
    public string GetLevel2SceneName() { return level2SceneName; }
    public string GetLevel3SceneName() { return level3SceneName; }
    public string GetBossLevelSceneName() { return bossLevelSceneName; }
    public string GetLastLoadedScene() { return lastLoadedScene; }

    public int GetCurrentScore()
    {
        return PlayerController.currentScore;
    }

    public int GetHighScore()
    {
        return PlayerPrefs.GetInt("HighScore", 0);
    }

    public string GetHighScoreDate()
    {
        return PlayerPrefs.GetString("HighScoreDate", "Never");
    }

    // ===== PROGRESSION CHECKERS =====
    public bool CanProgressToLevel2()
    {
        return PlayerController.currentScore >= level1RequiredScore;
    }

    public bool CanProgressToLevel3()
    {
        return PlayerController.currentScore >= level2RequiredScore;
    }

    public bool CanProgressToBossLevel()
    {
        return PlayerController.currentScore >= bossLevelRequiredScore;
    }

    public bool IsGameCompleted()
    {
        return gameCompleted;
    }

    // ===== UTILITY METHODS =====
    public string GetCurrentLevelName()
    {
        string sceneName = SceneManager.GetActiveScene().name.ToLower();

        switch (sceneName)
        {
            case "scene1":
            case "level1":
            case "menuscene":
                return "Level 1";

            case "scene2":
            case "level2":
                return "Level 2";

            case "scene3":
            case "level3":
                return "Level 3";

            case "bosslevel":
            case "finallevel":
                return "Boss Level";

            case "mainmenu":
            case "menu":
                return "Main Menu";

            default:
                return "Unknown Level";
        }
    }

    public int GetRequiredScoreForNextLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name.ToLower();

        switch (currentScene)
        {
            case "scene1":
            case "level1":
                return level1RequiredScore;

            case "scene2":
            case "level2":
                return level2RequiredScore;

            case "scene3":
            case "level3":
                return bossLevelRequiredScore;

            default:
                return 0;
        }
    }

    public float GetLevelProgress()
    {
        int required = GetRequiredScoreForNextLevel();
        if (required <= 0) return 1f;

        return Mathf.Clamp01((float)PlayerController.currentScore / required);
    }

    // ===== DEBUG METHODS =====
    private void DebugPrintGameState()
    {
        Debug.Log("🎮 === GAMEMANAGER STATE DEBUG ===");
        Debug.Log("Instance Valid: " + (instance != null));
        Debug.Log("Current Scene: " + SceneManager.GetActiveScene().name);
        Debug.Log("Current Score: " + PlayerController.currentScore);
        Debug.Log("High Score: " + GetHighScore());
        Debug.Log("High Score Date: " + GetHighScoreDate());
        Debug.Log("Game Paused: " + PlayerController.isGamePaused);
        Debug.Log("Time Scale: " + Time.timeScale);
        Debug.Log("Game Completed: " + gameCompleted);
        Debug.Log("Can Progress L2: " + CanProgressToLevel2() + " (Need: " + level1RequiredScore + ")");
        Debug.Log("Can Progress L3: " + CanProgressToLevel3() + " (Need: " + level2RequiredScore + ")");
        Debug.Log("Can Progress Boss: " + CanProgressToBossLevel() + " (Need: " + bossLevelRequiredScore + ")");
        Debug.Log("Level Progress: " + (GetLevelProgress() * 100f).ToString("F1") + "%");
        Debug.Log("Auto Update UI: " + autoUpdateUI);
        Debug.Log("Last UI Update: " + lastUIUpdateTime.ToString("F2"));
        Debug.Log("GameStateManager: " + (GameStateManager.instance != null ? "Connected" : "Disconnected"));
        Debug.Log("Reset Count: " + resetCallCount + " | Last Reset: " + lastResetTime.ToString("F2"));
        Debug.Log("Allow Instant Reset: " + allowInstantReset);
        Debug.Log("Auto Update Victory Screen: " + autoUpdateVictoryScreen); // ✅ NEW
        Debug.Log("===============================");
    }

    // ===== ENHANCED RESET TESTING METHODS =====
    public void TestMultipleResets()
    {
        StartCoroutine(TestResetSequence());
    }

    private System.Collections.IEnumerator TestResetSequence()
    {
        Debug.Log("🧪 === MULTIPLE RESET TEST START ===");

        SetScore(1000);
        Debug.Log("🧪 Set initial score: " + PlayerController.currentScore);
        yield return new WaitForSeconds(0.5f);

        Debug.Log("🧪 Reset #1...");
        ForceResetScore();
        yield return new WaitForSeconds(0.2f);
        Debug.Log("🧪 After Reset #1: " + PlayerController.currentScore);

        AddScore(500);
        Debug.Log("🧪 Added 500 points: " + PlayerController.currentScore);
        yield return new WaitForSeconds(0.2f);

        Debug.Log("🧪 Reset #2 (immediate)...");
        ForceResetScore();
        yield return new WaitForSeconds(0.2f);
        Debug.Log("🧪 After Reset #2: " + PlayerController.currentScore);

        AddScore(750);
        Debug.Log("🧪 Added 750 points: " + PlayerController.currentScore);
        yield return new WaitForSeconds(0.2f);

        Debug.Log("🧪 Reset #3 (immediate)...");
        ForceResetScore();
        yield return new WaitForSeconds(0.2f);
        Debug.Log("🧪 After Reset #3: " + PlayerController.currentScore);

        Debug.Log("🧪 === MULTIPLE RESET TEST COMPLETE ===");
    }

    public void QuickResetTest()
    {
        Debug.Log("🧪 === QUICK RESET TEST ===");
        Debug.Log("Before Reset: " + PlayerController.currentScore);
        Debug.Log("Last Reset Time: " + lastResetTime.ToString("F2"));
        Debug.Log("Current Time: " + Time.time.ToString("F2"));
        Debug.Log("Time Difference: " + (Time.time - lastResetTime).ToString("F2"));
        Debug.Log("Allow Instant Reset: " + allowInstantReset);

        ForceResetScore();

        Debug.Log("After Reset: " + PlayerController.currentScore);
        Debug.Log("========================");
    }

    public void EnableDebugMode(bool enable)
    {
        enableDebugLogs = enable;
        showScoreInGUI = enable;

        Debug.Log("🎮 GameManager Debug mode " + (enable ? "enabled" : "disabled"));
    }

    public void SetVictoryScreenDuration(float duration)
    {
        victoryScreenDuration = Mathf.Max(0f, duration);
        if (enableDebugLogs)
            Debug.Log("🏆 Victory screen duration set to: " + victoryScreenDuration + "s");
    }

    public void SetAllowInstantReset(bool allow)
    {
        allowInstantReset = allow;
        if (enableDebugLogs)
            Debug.Log("🔄 Allow instant reset: " + (allow ? "Enabled" : "Disabled"));
    }

    // ✅ NEW: Toggle auto victory screen update
    public void SetAutoUpdateVictoryScreen(bool enable)
    {
        autoUpdateVictoryScreen = enable;
        if (enableDebugLogs)
            Debug.Log("🏆 Auto update victory screen: " + (enable ? "Enabled" : "Disabled"));
    }

    // ===== EVENTS FOR OTHER SYSTEMS =====
    public void NotifyScoreChanged(int newScore)
    {
        OnScoreChanged?.Invoke(newScore);
        ForceUpdateAllUI();
    }

    public void NotifyGameWin()
    {
        OnGameWin?.Invoke();
        TriggerGameWin();
    }

    public void NotifyGameOver()
    {
        OnGameOver?.Invoke();
        TriggerGameOver();
    }

    // ===== BACKWARDS COMPATIBILITY =====
    public void LoadNextLevelScene()
    {
        LoadNextLevel();
    }

    public string GetNextLevelName()
    {
        return GetCurrentLevelName();
    }

    public void EnableAutoUIUpdate(bool enable)
    {
        autoUpdateUI = enable;
        if (enableDebugLogs)
            Debug.Log("🎮 Auto UI Update: " + (enable ? "Enabled" : "Disabled"));
    }

    public void SetUIUpdateRate(float rate)
    {
        uiUpdateRate = Mathf.Max(0.05f, rate);
        if (enableDebugLogs)
            Debug.Log("🎮 UI Update Rate set to: " + uiUpdateRate + "s");
    }

    // ===== SCENE EVENTS =====
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Invoke("DelayedUIUpdate", 0.5f);

        if (enableDebugLogs)
            Debug.Log("🎮 Scene loaded: " + scene.name + " | Score: " + PlayerController.currentScore);
    }

    private void DelayedUIUpdate()
    {
        ForceUpdateAllUI();
        if (enableDebugLogs)
            Debug.Log("🎮 Delayed UI update completed after scene load");
    }

    // ===== PUBLIC TESTING METHODS =====
    public void TestSceneProgression()
    {
        if (enableDebugLogs)
        {
            Debug.Log("🧪 === SCENE PROGRESSION TEST ===");
            Debug.Log("Current: " + GetCurrentLevelName());
            Debug.Log("Next Level Logic:");

            string currentScene = SceneManager.GetActiveScene().name.ToLower();
            switch (currentScene)
            {
                case "scene1":
                    Debug.Log("  Scene1 → Scene2");
                    break;
                case "scene2":
                    Debug.Log("  Scene2 → " + (CanProgressToBossLevel() ? "BossLevel" : "Scene3"));
                    break;
                case "scene3":
                    Debug.Log("  Scene3 → BossLevel");
                    break;
                case "bosslevel":
                    Debug.Log("  BossLevel → Game Complete");
                    break;
                default:
                    Debug.Log("  Unknown → " + nextLevelSceneName);
                    break;
            }
            Debug.Log("============================");
        }
    }

    public void TestAllSceneLoads()
    {
        StartCoroutine(TestSceneLoadSequence());
    }

    private System.Collections.IEnumerator TestSceneLoadSequence()
    {
        Debug.Log("🧪 Starting scene load test sequence...");

        yield return new WaitForSeconds(1f);
        Debug.Log("🧪 Testing Scene1 load...");
        LoadGameScene();

        yield return new WaitForSeconds(2f);
        Debug.Log("🧪 Testing Scene2 load...");
        LoadLevel2();

        yield return new WaitForSeconds(2f);
        Debug.Log("🧪 Testing Scene3 load...");
        LoadLevel3();

        yield return new WaitForSeconds(2f);
        Debug.Log("🧪 Testing Boss Level load...");
        LoadBossLevel();

        yield return new WaitForSeconds(2f);
        Debug.Log("🧪 Testing Main Menu load...");
        LoadMainMenu();

        Debug.Log("🧪 Scene load test sequence completed!");
    }

    public void TestResetScore()
    {
        Debug.Log("🧪 === RESET SCORE TEST ===");
        Debug.Log("Before Reset: " + PlayerController.currentScore);
        ForceResetScore();
        Debug.Log("After Reset: " + PlayerController.currentScore);
        Debug.Log("========================");
    }

    public void TestResetAndRestart()
    {
        Debug.Log("🧪 Testing Reset Score + Restart...");
        ResetScoreAndRestart();
    }

    // ===== PUBLIC RESET METHODS FOR UI BUTTONS =====
    public void ResetScoreForButtonClick()
    {
        ForceResetScore();
    }

    public void ResetScoreAndRestartForButton()
    {
        ResetScoreAndRestart();
    }

    public void ResetScoreButtonSafe()
    {
        ResetScore();
    }

    public void ResetScoreButtonForce()
    {
        ForceResetScore();
    }
}