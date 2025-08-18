using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "Scene1";
    [SerializeField] private string mainMenuSceneName = "0";
    [SerializeField] private string nextLevelSceneName = "Scene2";
    [SerializeField] private string level2SceneName = "Level2";
    [SerializeField] private string level3SceneName = "Level3";
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
    [SerializeField] private bool autoUpdateVictoryScreen = true;

    [Header("Game Over Settings")]
    [SerializeField] private float gameOverScreenDuration = 5f;
    [SerializeField] private bool showGameOverLog = true;
    [SerializeField] private bool autoUpdateGameOverScreen = true;
    [SerializeField] private bool saveScoreOnGameOver = true;

    [Header("Level 2 Settings")]
    [SerializeField] private float level2SpeedMultiplier = 1.3f;
    [SerializeField] private float level2HealthMultiplier = 1.2f;
    [SerializeField] private Color level2EnemyColor = new Color(1f, 0.8f, 0.8f, 1f);

    [Header("Reset System")]
    [SerializeField] private bool debugResetSystem = true;
    [SerializeField] private int maxResetAttempts = 5;
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
    private bool gameEnded = false;
    private string lastLoadedScene = "";
    private int resetCallCount = 0;
    private float lastResetTime = 0f;

    void Awake()
    {
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
    }

    void Start()
    {
        InitializeGame();
        lastKnownScore = PlayerController.currentScore;
    }

    void Update()
    {
        try
        {
            if (autoUpdateUI)
            {
                CheckAndUpdateScore();
            }

            if (enableDebugLogs)
            {
                if (Input.GetKeyDown(KeyCode.F1))
                    DebugPrintGameState();

                if (Input.GetKeyDown(KeyCode.F2))
                    TestAllConnections();
            }

            if (Application.isEditor)
            {
                if (Input.GetKeyDown(KeyCode.T))
                {
                    AddScore(100);
                    Debug.Log("🧪 TEST: Added 100 points via GameManager");
                }

                if (Input.GetKeyDown(KeyCode.H))
                {
                    SetScore(5000);
                    Debug.Log("🧪 TEST: Set high score (5000)");
                }

                // ✅ ENHANCED RESET TEST KEYS
                if (Input.GetKeyDown(KeyCode.R))
                {
                    MasterResetScore();
                    Debug.Log("🧪 TEST: Master Reset via R key");
                }

                if (Input.GetKeyDown(KeyCode.F5))
                {
                    Debug.Log("🧪 F5: Force reset test");
                    MasterResetScore();
                }

                if (Input.GetKeyDown(KeyCode.F6))
                {
                    Debug.Log($"🧪 F6: Current score = {PlayerController.currentScore}");
                }

                if (Input.GetKeyDown(KeyCode.F7))
                {
                    Debug.Log("🧪 F7: Master Reset Test");
                    MasterResetScore();
                }

                if (Input.GetKeyDown(KeyCode.F8))
                {
                    Debug.Log($"🧪 F8: Current score check = {PlayerController.currentScore}");
                }

                if (Input.GetKeyDown(KeyCode.V))
                {
                    TriggerGameWin();
                    Debug.Log("🧪 TEST: Trigger victory screen with current score: " + PlayerController.currentScore);
                }

                if (Input.GetKeyDown(KeyCode.G))
                {
                    TriggerGameOver();
                    Debug.Log("🧪 TEST: Trigger game over screen with current score: " + PlayerController.currentScore);
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
                    foreach (GameObject enemy in enemies)
                    {
                        ModifyEnemyForLevel2(enemy);
                    }
                    Debug.Log("🧪 TEST: Modified all enemies for Level 2");
                }

                // Level switching
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
            GUI.Box(new Rect(10, 10, 400, 380), "");
            GUI.Label(new Rect(15, 15, 390, 20), "GAMEMANAGER DEBUG");
            GUI.Label(new Rect(15, 35, 390, 20), "Current Score: " + PlayerController.currentScore);
            GUI.Label(new Rect(15, 55, 390, 20), "High Score: " + GetHighScore());
            GUI.Label(new Rect(15, 75, 390, 20), "Current Scene: " + SceneManager.GetActiveScene().name);
            GUI.Label(new Rect(15, 95, 390, 20), "Instance Valid: " + (instance != null ? "✅" : "❌"));
            GUI.Label(new Rect(15, 115, 390, 20), "GameStateManager: " + (GameStateManager.instance != null ? "✅" : "❌"));
            GUI.Label(new Rect(15, 135, 390, 20), "Can Progress L2: " + CanProgressToLevel2());
            GUI.Label(new Rect(15, 155, 390, 20), "Reset Count: " + resetCallCount + " | Last: " + lastResetTime.ToString("F1"));
            GUI.Label(new Rect(15, 175, 390, 20), "Game Completed: " + (gameCompleted ? "✅" : "❌") + " | Game Over: " + (gameEnded ? "✅" : "❌"));
            GUI.Label(new Rect(15, 195, 390, 20), "F1: Debug, F2: Test, T: +100, H: 5000");
            GUI.Label(new Rect(15, 215, 390, 20), "R/F5/F7: Master Reset, F6/F8: Check Score");
            GUI.Label(new Rect(15, 235, 390, 20), "V: Victory, G: Game Over, E: Modify Enemies");
            GUI.Label(new Rect(15, 255, 390, 20), "1-3: Levels, B: Boss, M: Menu");
            GUI.Label(new Rect(15, 275, 390, 20), "Level 2 Speed Mult: " + level2SpeedMultiplier);
            GUI.Label(new Rect(15, 295, 390, 20), "Level 2 Color: " + level2EnemyColor);
            GUI.Label(new Rect(15, 315, 390, 20), "Allow Instant Reset: " + (allowInstantReset ? "✅" : "❌"));
            GUI.Label(new Rect(15, 335, 390, 20), "Debug Reset System: " + (debugResetSystem ? "✅" : "❌"));
        }
    }

    // ===== ✅ MASTER RESET SYSTEM =====
    public void MasterResetScore()
    {
        if (debugResetSystem)
            Debug.Log("🔄 === MASTER RESET STARTED ===");

        StartCoroutine(ComprehensiveResetSequence());
    }

    private IEnumerator ComprehensiveResetSequence()
    {
        int initialScore = PlayerController.currentScore;

        if (debugResetSystem)
            Debug.Log($"🔄 Starting reset sequence. Initial score: {initialScore}");

        // Step 1: Reset PlayerController nhiều lần để đảm bảo
        for (int attempt = 0; attempt < maxResetAttempts; attempt++)
        {
            PlayerController.currentScore = 0;
            yield return null; // Wait 1 frame

            if (debugResetSystem)
                Debug.Log($"🔄 Reset attempt {attempt + 1}: Score = {PlayerController.currentScore}");
        }

        // Step 2: Reset local tracking
        lastKnownScore = 0;
        resetCallCount++;
        lastResetTime = Time.time;

        // Step 3: Trigger events immediately
        OnScoreChanged?.Invoke(0);
        OnScoreReset?.Invoke();

        if (debugResetSystem)
            Debug.Log($"🔄 Score reset: {initialScore} → {PlayerController.currentScore}");

        // Step 4: Update UI systems (immediate)
        UpdateAllUISystems();

        yield return new WaitForSeconds(0.1f);

        // Step 5: Update UI systems (delayed for safety)
        UpdateAllUISystems();

        yield return new WaitForSeconds(0.1f);

        // Step 6: Final verification and emergency reset if needed
        if (PlayerController.currentScore != 0)
        {
            if (debugResetSystem)
                Debug.LogWarning("⚠️ Score still not 0 after reset! Emergency reset...");

            for (int i = 0; i < 3; i++)
            {
                PlayerController.currentScore = 0;
                yield return null;
            }

            UpdateAllUISystems();
        }

        if (debugResetSystem)
        {
            Debug.Log($"🔄 Final score after reset: {PlayerController.currentScore}");
            Debug.Log("✅ === MASTER RESET COMPLETED ===");
        }
    }

    private void UpdateAllUISystems()
    {
        try
        {
            if (debugResetSystem)
                Debug.Log("🔄 Updating all UI systems...");

            // Update GameStateManager
            UpdateGameStateManager();

            // Update all Text components
            UpdateAllScoreTexts();

            // Update TMPro texts (if available)
            UpdateTMProTexts();

            // Refresh Canvas components
            RefreshAllCanvas();

            // Trigger events again
            OnScoreChanged?.Invoke(0);

            if (debugResetSystem)
                Debug.Log("✅ All UI systems updated");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ UpdateAllUISystems failed: {e.Message}");
        }
    }

    private void UpdateGameStateManager()
    {
        try
        {
            if (GameStateManager.instance != null)
            {
                GameStateManager.instance.ForceUpdateHUD();
                if (debugResetSystem)
                    Debug.Log("✅ GameStateManager.ForceUpdateHUD() called");
            }
            else
            {
                GameStateManager gsm = FindObjectOfType<GameStateManager>();
                if (gsm != null)
                {
                    gsm.ForceUpdateHUD();
                    if (debugResetSystem)
                        Debug.Log("✅ Found GameStateManager and called ForceUpdateHUD()");
                }
                else
                {
                    if (debugResetSystem)
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
            UnityEngine.UI.Text[] allTexts = Resources.FindObjectsOfTypeAll<UnityEngine.UI.Text>();
            int updatedCount = 0;

            foreach (UnityEngine.UI.Text text in allTexts)
            {
                if (text != null && text.gameObject.scene.isLoaded)
                {
                    string textName = text.name.ToLower();
                    string oldText = text.text;
                    bool wasUpdated = false;

                    if (textName.Contains("score") && !textName.Contains("high"))
                    {
                        text.text = "SCORE: 0";
                        wasUpdated = true;
                    }
                    else if (textName.Contains("point") && !textName.Contains("high"))
                    {
                        text.text = "POINTS: 0";
                        wasUpdated = true;
                    }
                    else if (IsScoreText(oldText))
                    {
                        text.text = ReplaceScoreInText(oldText, 0);
                        wasUpdated = true;
                    }

                    if (wasUpdated)
                    {
                        // Force refresh text component
                        text.enabled = false;
                        text.enabled = true;
                        updatedCount++;

                        if (debugResetSystem)
                            Debug.Log($"🔄 Reset text '{text.name}': '{oldText}' → '{text.text}'");
                    }
                }
            }

            if (debugResetSystem && updatedCount > 0)
                Debug.Log($"🔄 Updated {updatedCount} score text elements to 0");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ UpdateAllScoreTexts failed: {e.Message}");
        }
    }

    private void UpdateTMProTexts()
    {
        try
        {
            // Using reflection to avoid hard dependency on TextMeshPro
            System.Type tmpType = System.Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
            if (tmpType != null)
            {
                UnityEngine.Object[] tmpTexts = Resources.FindObjectsOfTypeAll(tmpType);
                int updatedCount = 0;

                foreach (UnityEngine.Object tmpObj in tmpTexts)
                {
                    if (tmpObj != null)
                    {
                        MonoBehaviour tmpComponent = tmpObj as MonoBehaviour;
                        if (tmpComponent != null && tmpComponent.gameObject.scene.isLoaded)
                        {
                            string textName = tmpComponent.name.ToLower();
                            if (textName.Contains("score") && !textName.Contains("high"))
                            {
                                // Use reflection to set text property
                                var textProperty = tmpType.GetProperty("text");
                                if (textProperty != null)
                                {
                                    textProperty.SetValue(tmpComponent, "SCORE: 0");
                                    tmpComponent.enabled = false;
                                    tmpComponent.enabled = true;
                                    updatedCount++;

                                    if (debugResetSystem)
                                        Debug.Log($"🔄 Updated TMPro text: {tmpComponent.name}");
                                }
                            }
                        }
                    }
                }

                if (debugResetSystem && updatedCount > 0)
                    Debug.Log($"🔄 Updated {updatedCount} TMPro texts");
            }
        }
        catch (System.Exception e)
        {
            if (debugResetSystem)
                Debug.Log("ℹ️ TMPro not available or failed to update: " + e.Message);
        }
    }

    private void RefreshAllCanvas()
    {
        try
        {
            Canvas[] allCanvas = FindObjectsOfType<Canvas>();
            int refreshedCount = 0;

            foreach (Canvas canvas in allCanvas)
            {
                if (canvas != null)
                {
                    canvas.enabled = false;
                    canvas.enabled = true;
                    refreshedCount++;
                }
            }

            if (debugResetSystem && refreshedCount > 0)
                Debug.Log($"🔄 Refreshed {refreshedCount} Canvas components");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ RefreshAllCanvas failed: {e.Message}");
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

    // ✅ PUBLIC METHODS FOR BUTTONS
    public void ResetScoreButton()
    {
        if (debugResetSystem)
            Debug.Log("🎮 Reset Score Button clicked");
        MasterResetScore();
    }

    public void RestartGameButton()
    {
        if (debugResetSystem)
            Debug.Log("🎮 Restart Game Button clicked");
        StartCoroutine(RestartGameSequence());
    }

    private IEnumerator RestartGameSequence()
    {
        MasterResetScore();
        yield return new WaitForSeconds(0.3f);
        RestartCurrentLevel();
    }

    // ===== LEVEL 2 ENEMY ENHANCEMENT =====
    public void ModifyEnemyForLevel2(GameObject enemy)
    {
        if (enemy == null) return;

        try
        {
            MonoBehaviour[] scripts = enemy.GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour script in scripts)
            {
                if (script == null) continue;

                var setSpeedMethod = script.GetType().GetMethod("SetSpeed");
                var getSpeedMethod = script.GetType().GetMethod("GetSpeed");

                if (setSpeedMethod != null && getSpeedMethod != null)
                {
                    try
                    {
                        float currentSpeed = (float)getSpeedMethod.Invoke(script, null);
                        float newSpeed = currentSpeed * level2SpeedMultiplier;
                        setSpeedMethod.Invoke(script, new object[] { newSpeed });

                        if (enableDebugLogs)
                            Debug.Log($"✅ Modified enemy speed: {currentSpeed} → {newSpeed}");
                    }
                    catch (System.Exception e)
                    {
                        if (enableDebugLogs)
                            Debug.LogWarning($"⚠️ Could not modify enemy speed: {e.Message}");
                    }
                    break;
                }
            }

            SpriteRenderer spriteRenderer = enemy.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = level2EnemyColor;

                if (enableDebugLogs)
                    Debug.Log("🎨 Applied Level 2 enemy color: " + level2EnemyColor);
            }

            if (enableDebugLogs)
                Debug.Log("✅ Level 2 enemy modification completed for: " + enemy.name);
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ ModifyEnemyForLevel2 failed: " + e.Message);
        }
    }

    public void EnhanceEnemyForLevel2(GameObject enemy)
    {
        ModifyEnemyForLevel2(enemy);
    }

    // ===== SCORE MANAGEMENT =====
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
                        text.text = "SCORE: " + PlayerController.currentScore.ToString();
                        updatedCount++;

                        if (enableDebugLogs)
                            Debug.Log("✅ Updated score UI: " + text.name + " = " + text.text);
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

    // ===== LEGACY RESET METHODS (for backward compatibility) =====
    public void ResetScore()
    {
        MasterResetScore();
    }

    public void ForceResetScore()
    {
        MasterResetScore();
    }

    // ===== VICTORY AND GAME OVER SYSTEMS =====
    private void ShowGameCompletedScreen()
    {
        if (gameCompleted) return;
        gameCompleted = true;

        if (autoUpdateVictoryScreen)
        {
            UpdateVictoryScreenScore();
        }

        if (showVictoryLog)
        {
            Debug.Log("🏆 ===============================");
            Debug.Log("🏆 CONGRATULATIONS! GAME COMPLETED!");
            Debug.Log("🏆 Final Score: " + PlayerController.currentScore);
            Debug.Log("🏆 ===============================");
        }

        SaveHighScore();
        OnGameWin?.Invoke();

        if (victoryScreenDuration > 0)
        {
            Invoke("ReturnToMainMenuAfterVictory", victoryScreenDuration);
        }
    }

    private void UpdateVictoryScreenScore()
    {
        try
        {
            if (enableDebugLogs)
                Debug.Log("🏆 Updating victory screen score: " + PlayerController.currentScore);

            UnityEngine.UI.Text[] allTexts = Resources.FindObjectsOfTypeAll<UnityEngine.UI.Text>();
            int updatedCount = 0;

            foreach (UnityEngine.UI.Text text in allTexts)
            {
                if (text != null && text.gameObject.scene.isLoaded)
                {
                    string textName = text.name.ToLower();

                    if (textName.Contains("final") || textName.Contains("victory") || textName.Contains("complete"))
                    {
                        if (textName.Contains("score"))
                        {
                            text.text = "FINAL SCORE: " + PlayerController.currentScore.ToString();
                            text.enabled = false;
                            text.enabled = true;
                            updatedCount++;

                            if (enableDebugLogs)
                                Debug.Log("🏆 Updated victory text: " + text.name + " = " + text.text);
                        }
                    }
                }
            }

            OnScoreChanged?.Invoke(PlayerController.currentScore);

            if (enableDebugLogs)
                Debug.Log("🏆 Victory screen score update completed - Updated: " + updatedCount + " texts");
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ UpdateVictoryScreenScore failed: " + e.Message);
        }
    }

    public void TriggerGameWin()
    {
        ShowGameCompletedScreen();
        Invoke("UpdateVictoryScreenScore", 0.2f);
    }

    private void ShowGameOverScreen()
    {
        if (gameEnded) return;
        gameEnded = true;

        if (autoUpdateGameOverScreen)
        {
            UpdateGameOverScreenScore();
        }

        if (showGameOverLog)
        {
            Debug.Log("💀 ===============================");
            Debug.Log("💀 GAME OVER!");
            Debug.Log("💀 Final Score: " + PlayerController.currentScore);
            Debug.Log("💀 ===============================");
        }

        if (saveScoreOnGameOver)
        {
            SaveHighScore();
        }

        OnGameOver?.Invoke();

        if (gameOverScreenDuration > 0)
        {
            Invoke("ReturnToMainMenuAfterGameOver", gameOverScreenDuration);
        }
    }

    private void UpdateGameOverScreenScore()
    {
        try
        {
            if (enableDebugLogs)
                Debug.Log("💀 Updating game over screen score: " + PlayerController.currentScore);

            UnityEngine.UI.Text[] allTexts = Resources.FindObjectsOfTypeAll<UnityEngine.UI.Text>();
            int updatedCount = 0;

            foreach (UnityEngine.UI.Text text in allTexts)
            {
                if (text != null && text.gameObject.scene.isLoaded)
                {
                    string textName = text.name.ToLower();

                    if (textName.Contains("gameover") || textName.Contains("game over") || textName.Contains("dead") || textName.Contains("final"))
                    {
                        if (textName.Contains("score"))
                        {
                            text.text = "FINAL SCORE: " + PlayerController.currentScore.ToString();
                            text.enabled = false;
                            text.enabled = true;
                            updatedCount++;

                            if (enableDebugLogs)
                                Debug.Log("💀 Updated game over text: " + text.name + " = " + text.text);
                        }
                    }
                }
            }

            OnScoreChanged?.Invoke(PlayerController.currentScore);

            if (enableDebugLogs)
                Debug.Log("💀 Game over screen score update completed - Updated: " + updatedCount + " texts");
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ UpdateGameOverScreenScore failed: " + e.Message);
        }
    }

    public void TriggerGameOver()
    {
        ShowGameOverScreen();
        Invoke("UpdateGameOverScreenScore", 0.2f);
    }

    private void ReturnToMainMenuAfterVictory()
    {
        if (enableDebugLogs)
            Debug.Log("🏆 Victory screen timeout - returning to main menu");
        LoadMainMenu();
    }

    private void ReturnToMainMenuAfterGameOver()
    {
        if (enableDebugLogs)
            Debug.Log("💀 Game Over screen timeout - returning to main menu");
        LoadMainMenu();
    }

    // ===== SCENE LOADING METHODS =====
    public void LoadGameScene()
    {
        ResetGameState();
        LoadScene(gameSceneName);
    }

    public void LoadLevel2()
    {
        ResetGameState();
        LoadScene(level2SceneName);
    }

    public void LoadLevel3()
    {
        ResetGameState();
        LoadScene(level3SceneName);
    }

    public void LoadBossLevel()
    {
        ResetGameState();
        LoadScene(bossLevelSceneName);
    }

    public void LoadMainMenu()
    {
        ResetGameState();
        SaveHighScore();
        LoadScene(mainMenuSceneName);
    }

    public void LoadNextLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name.ToLower();
        ResetGameState();

        switch (currentScene)
        {
            case "scene1":
                LoadScene(level2SceneName);
                break;
            case "scene2":
                LoadScene(level3SceneName);
                break;
            case "scene3":
                LoadScene(bossLevelSceneName);
                break;
            default:
                LoadScene(nextLevelSceneName);
                break;
        }
    }

    public void RestartCurrentLevel()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        ResetGameState();
        LoadScene(currentSceneName);
    }

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
            SceneManager.LoadScene(sceneName);
            if (enableDebugLogs)
                Debug.Log("✅ Loading scene: " + sceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Error loading scene '" + sceneName + "': " + e.Message);
        }
    }

    // ===== GAME STATE MANAGEMENT =====
    private void ResetGameState()
    {
        Time.timeScale = 1f;
        PlayerController.isGamePaused = false;
        gameCompleted = false;
        gameEnded = false;

        if (enableDebugLogs)
            Debug.Log("🎮 Game state reset");
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

    private void TestAllConnections()
    {
        Debug.Log("🧪 === CONNECTION TEST ===");
        Debug.Log("GameManager.instance: " + (instance != null ? "✅" : "❌"));
        Debug.Log("GameStateManager.instance: " + (GameStateManager.instance != null ? "✅" : "❌"));
        Debug.Log("PlayerController.currentScore: " + PlayerController.currentScore);
        Debug.Log("========================");
    }

    // ===== HIGH SCORE SYSTEM =====
    private void SaveHighScore()
    {
        int currentHighScore = PlayerPrefs.GetInt("HighScore", 0);
        if (PlayerController.currentScore > currentHighScore)
        {
            PlayerPrefs.SetInt("HighScore", PlayerController.currentScore);
            PlayerPrefs.Save();

            if (enableDebugLogs)
                Debug.Log("🏆 NEW HIGH SCORE: " + PlayerController.currentScore);
        }
    }

    public int GetHighScore()
    {
        return PlayerPrefs.GetInt("HighScore", 0);
    }

    // ===== GETTER METHODS =====
    public int GetCurrentScore()
    {
        return PlayerController.currentScore;
    }

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

    // ===== DEBUG METHODS =====
    private void DebugPrintGameState()
    {
        Debug.Log("🎮 === GAMEMANAGER STATE DEBUG ===");
        Debug.Log("Instance Valid: " + (instance != null));
        Debug.Log("Current Scene: " + SceneManager.GetActiveScene().name);
        Debug.Log("Current Score: " + PlayerController.currentScore);
        Debug.Log("High Score: " + GetHighScore());
        Debug.Log("Game Completed: " + gameCompleted);
        Debug.Log("Game Over: " + gameEnded);
        Debug.Log("Level 2 Speed Multiplier: " + level2SpeedMultiplier);
        Debug.Log("Debug Reset System: " + debugResetSystem);
        Debug.Log("Reset Call Count: " + resetCallCount);
        Debug.Log("===============================");
    }

    public void QuitGame()
    {
        if (enableDebugLogs)
            Debug.Log("🔚 Quitting game...");

        SaveHighScore();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}