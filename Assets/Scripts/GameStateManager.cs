using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    [Header("Game Win UI")]
    [SerializeField] private GameObject gameWinPanel;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button winMainMenuButton;
    [SerializeField] private Button winQuitButton;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button gameOverRestartButton;
    [SerializeField] private Button gameOverMainMenuButton;
    [SerializeField] private Button gameOverQuitButton;
    [SerializeField] private Text gameOverScoreText;
    [SerializeField] private Text gameOverHighScoreText;
    [SerializeField] private Text gameOverTitleText;

    [Header("Pause Menu UI")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseMainMenuButton;
    [SerializeField] private Button pauseQuitButton;

    [Header("HUD Elements")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text highScoreText;
    [SerializeField] private Text livesText;
    [SerializeField] private Text levelText;
    [SerializeField] private Text timeText;

    [Header("Win Conditions")]
    [SerializeField] private bool winByScore = false;
    [SerializeField] private int winScore = 5000;
    [SerializeField] private bool winByTime = false;
    [SerializeField] private int gameTimeLimit = 180;
    [SerializeField] private bool winByEnemyCount = false;
    [SerializeField] private int bonusScore = 100;

    [Header("Level Progression")]
    [SerializeField] private int level1RequiredScore = 1000;
    [SerializeField] private int level2RequiredScore = 2000;
    [SerializeField] private int bossLevelRequiredScore = 3000;

    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool showDebugInfo = true;

    // Private variables
    private bool gameWon = false;
    private bool gameOver = false;
    private bool gamePaused = false;
    private float gameStartTime;
    private string currentLevelName = "Level 1";
    private int previousScore = 0;

    public static GameStateManager instance;

    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        gameStartTime = Time.time;
        currentLevelName = GetCurrentLevelDisplayName();

        // Check GameManager connection first
        CheckGameManagerConnection();

        // Auto-find UI elements
        AutoFindUIElements();

        // Setup button listeners
        SetupButtonListeners();

        // Initial UI update
        ForceUpdateHUD();

        if (enableDebugLogs)
            Debug.Log("✅ GameStateManager initialized for: " + currentLevelName);
    }

    void Update()
    {
        if (gameWon || gamePaused) return;

        try
        {
            // Update HUD
            UpdateHUD();

            // Check win conditions only if game not over
            if (!gameOver) CheckWinConditions();

            // Handle pause input
            if (Input.GetKeyDown(KeyCode.Escape) && !gameOver)
            {
                if (gamePaused)
                    ResumeGame();
                else
                    PauseGame();
            }

            // Debug controls
            if (enableDebugLogs && Application.isEditor)
            {
                if (Input.GetKeyDown(KeyCode.K))
                    TriggerGameOver();
                if (Input.GetKeyDown(KeyCode.L))
                    TriggerGameWin();
                if (Input.GetKeyDown(KeyCode.F4))
                    TestAllButtons();

                // Manual button tests when game over
                if (gameOver)
                {
                    if (Input.GetKeyDown(KeyCode.R))
                        ManualRestartGame();
                    if (Input.GetKeyDown(KeyCode.M))
                        ManualReturnToMainMenu();
                    if (Input.GetKeyDown(KeyCode.Q))
                        ManualQuitApplication();
                }
            }
        }
        catch (System.Exception e)
        {
            if (enableDebugLogs)
                Debug.LogError("❌ GameStateManager Update error: " + e.Message);
        }
    }

    void OnGUI()
    {
        if (showDebugInfo && Application.isEditor)
        {
            GUI.Box(new Rect(Screen.width - 320, 10, 310, 200), "");
            GUI.Label(new Rect(Screen.width - 315, 15, 300, 20), "GAME STATE DEBUG");
            GUI.Label(new Rect(Screen.width - 315, 35, 300, 20), "Score: " + PlayerController.currentScore);
            GUI.Label(new Rect(Screen.width - 315, 55, 300, 20), "Game Won: " + gameWon);
            GUI.Label(new Rect(Screen.width - 315, 75, 300, 20), "Game Over: " + gameOver);
            GUI.Label(new Rect(Screen.width - 315, 95, 300, 20),
                      "GameManager: " + (GameManager.instance != null ? "✅" : "❌"));
            GUI.Label(new Rect(Screen.width - 315, 115, 300, 20),
                      "Restart Btn: " + (gameOverRestartButton != null ? "✅" : "❌"));
            GUI.Label(new Rect(Screen.width - 315, 135, 300, 20),
                      "Menu Btn: " + (gameOverMainMenuButton != null ? "✅" : "❌"));
            GUI.Label(new Rect(Screen.width - 315, 155, 300, 20),
                      "Quit Btn: " + (gameOverQuitButton != null ? "✅" : "❌"));
            GUI.Label(new Rect(Screen.width - 315, 175, 300, 20),
                      "F4: Test, K: GameOver, L: Win");
            GUI.Label(new Rect(Screen.width - 315, 195, 300, 20),
                      "R: Restart, M: Menu, Q: Quit");
        }
    }

    // ===== INITIALIZATION METHODS =====
    private void CheckGameManagerConnection()
    {
        if (GameManager.instance == null)
        {
            if (enableDebugLogs)
                Debug.LogWarning("⚠️ GameManager.instance is null! Looking for GameManager...");

            GameManager gameManagerInScene = FindObjectOfType<GameManager>();
            if (gameManagerInScene != null)
            {
                if (enableDebugLogs)
                    Debug.Log("✅ Found GameManager in scene");
            }
            else
            {
                if (enableDebugLogs)
                    Debug.LogWarning("⚠️ No GameManager found! Some features may not work.");
            }
        }
        else
        {
            if (enableDebugLogs)
                Debug.Log("✅ GameManager.instance connected");
        }
    }

    private void AutoFindUIElements()
    {
        if (scoreText == null)
        {
            GameObject scoreObj = GameObject.Find("Score");
            if (scoreObj != null)
            {
                scoreText = scoreObj.GetComponent<Text>();
                if (scoreText != null && enableDebugLogs)
                    Debug.Log("✅ Auto-found Score Text: " + scoreObj.name);
            }

            if (scoreText == null)
            {
                Text[] allTexts = FindObjectsOfType<Text>();
                foreach (Text text in allTexts)
                {
                    if (text.name.ToLower().Contains("score") && !text.name.ToLower().Contains("high"))
                    {
                        scoreText = text;
                        if (enableDebugLogs)
                            Debug.Log("✅ Auto-found Score Text by search: " + text.name);
                        break;
                    }
                }
            }
        }

        if (highScoreText == null)
        {
            Text[] allTexts = FindObjectsOfType<Text>();
            foreach (Text text in allTexts)
            {
                if (text.name.ToLower().Contains("high") && text.name.ToLower().Contains("score"))
                {
                    highScoreText = text;
                    if (enableDebugLogs)
                        Debug.Log("✅ Auto-found High Score Text: " + text.name);
                    break;
                }
            }
        }
    }

    // ===== HUD UPDATE METHODS =====
    private void UpdateHUD()
    {
        UpdateScoreDisplay();
        UpdateOtherHUDElements();
    }

    public void ForceUpdateHUD()
    {
        UpdateScoreDisplay();
        UpdateOtherHUDElements();
        if (enableDebugLogs && PlayerController.currentScore != previousScore)
            Debug.Log("💰 HUD Force Updated - Score: " + PlayerController.currentScore);
    }

    private void UpdateScoreDisplay()
    {
        int currentScore = PlayerController.currentScore;

        if (scoreText != null)
        {
            scoreText.text = "SCORE: " + currentScore.ToString();

            if (currentScore != previousScore && enableDebugLogs)
            {
                Debug.Log("💰 Score UI updated: " + previousScore + " → " + currentScore);
                previousScore = currentScore;
            }
        }
        else
        {
            TryFindScoreText();
        }

        if (highScoreText != null)
        {
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            highScoreText.text = "HIGH: " + highScore.ToString();
        }
    }

    private void TryFindScoreText()
    {
        GameObject scoreObj = GameObject.Find("Score");
        if (scoreObj != null)
        {
            Text foundText = scoreObj.GetComponent<Text>();
            if (foundText != null)
            {
                scoreText = foundText;
                scoreText.text = "SCORE: " + PlayerController.currentScore.ToString();
                if (enableDebugLogs)
                    Debug.Log("✅ Found Score Text: " + scoreObj.name);
                return;
            }
        }

        Canvas[] allCanvas = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in allCanvas)
        {
            Text[] textsInCanvas = canvas.GetComponentsInChildren<Text>();
            foreach (Text text in textsInCanvas)
            {
                if (text.name.ToLower().Contains("score") && !text.name.ToLower().Contains("high"))
                {
                    scoreText = text;
                    scoreText.text = "SCORE: " + PlayerController.currentScore.ToString();
                    if (enableDebugLogs)
                        Debug.Log("✅ Found Score Text in Canvas: " + text.name);
                    return;
                }
            }
        }

        if (enableDebugLogs)
            Debug.LogWarning("⚠️ Score Text not found!");
    }

    private void UpdateOtherHUDElements()
    {
        if (livesText != null)
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                livesText.text = "HEALTH: " + player.GetCurrentHealth() + "/" + player.GetMaxHealth();
            }
        }

        if (levelText != null)
            levelText.text = "LEVEL: " + currentLevelName;

        if (timeText != null)
        {
            float gameTime = Time.time - gameStartTime;
            int minutes = Mathf.FloorToInt(gameTime / 60);
            int seconds = Mathf.FloorToInt(gameTime % 60);
            timeText.text = string.Format("TIME: {0:00}:{1:00}", minutes, seconds);
        }
    }

    // ===== SCORE MANAGEMENT =====
    public void AddScoreAndUpdateUI(int points)
    {
        PlayerController.currentScore += points;
        ForceUpdateHUD();
        if (enableDebugLogs)
            Debug.Log("💰 Score added via GameStateManager: +" + points + " | Total: " + PlayerController.currentScore);
    }

    public void UpdateScore(int newScore)
    {
        PlayerController.currentScore = newScore;
        ForceUpdateHUD();
    }

    // ===== WIN CONDITIONS =====
    private void CheckWinConditions()
    {
        if (gameWon || gameOver) return;

        bool shouldWin = false;

        if (winByScore && PlayerController.currentScore >= winScore)
        {
            shouldWin = true;
            if (enableDebugLogs)
                Debug.Log("🏆 Win by Score: " + PlayerController.currentScore + " >= " + winScore);
        }

        if (winByTime)
        {
            float gameTime = Time.time - gameStartTime;
            if (gameTime >= gameTimeLimit)
            {
                shouldWin = true;
                if (enableDebugLogs)
                    Debug.Log("🏆 Win by Time: " + gameTime + " >= " + gameTimeLimit);
            }
        }

        if (winByEnemyCount)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            if (enemies.Length == 0)
            {
                Invoke("CheckEnemiesAgain", 5f);
            }
        }

        if (shouldWin)
        {
            TriggerGameWin();
        }
    }

    private void CheckEnemiesAgain()
    {
        if (gameWon || gameOver) return;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0)
        {
            if (enableDebugLogs)
                Debug.Log("🏆 Win by Enemy Count: All enemies eliminated");
            TriggerGameWin();
        }
    }

    // ===== GAME WIN SYSTEM =====
    public void TriggerGameWin()
    {
        if (gameWon || gameOver) return;

        gameWon = true;

        if (bonusScore > 0)
        {
            PlayerController.currentScore += bonusScore;
            ForceUpdateHUD();
            if (enableDebugLogs)
                Debug.Log("💰 Bonus score added: +" + bonusScore);
        }

        SaveHighScore();

        if (gameWinPanel != null)
        {
            gameWinPanel.SetActive(true);
        }

        Time.timeScale = 0f;

        if (enableDebugLogs)
            Debug.Log("🏆 LEVEL COMPLETED! Final Score: " + PlayerController.currentScore);
    }

    // ===== GAME OVER SYSTEM =====
    public void TriggerGameOver()
    {
        if (gameOver || gameWon) return;

        gameOver = true;

        if (enableDebugLogs)
            Debug.Log("💀 GAME OVER! Final Score: " + PlayerController.currentScore);

        SaveHighScore();
        UpdateGameOverUI();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (enableDebugLogs)
                Debug.Log("✅ Game Over Panel activated");
        }
        else
        {
            if (enableDebugLogs)
                Debug.LogWarning("⚠️ Game Over Panel not assigned! Using fallback.");
            ShowFallbackGameOver();
        }

        Time.timeScale = 0f;
    }

    private void UpdateGameOverUI()
    {
        if (gameOverTitleText != null)
        {
            gameOverTitleText.text = "GAME OVER";
        }

        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = "FINAL SCORE: " + PlayerController.currentScore.ToString();
        }

        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        bool isNewHighScore = PlayerController.currentScore > highScore;

        if (gameOverHighScoreText != null)
        {
            if (isNewHighScore)
            {
                gameOverHighScoreText.text = "NEW HIGH SCORE: " + PlayerController.currentScore.ToString();
                gameOverHighScoreText.color = Color.yellow;
            }
            else
            {
                gameOverHighScoreText.text = "HIGH SCORE: " + highScore.ToString();
                gameOverHighScoreText.color = Color.white;
            }
        }
    }

    private void ShowFallbackGameOver()
    {
        if (enableDebugLogs)
        {
            Debug.Log("💀 === GAME OVER ===");
            Debug.Log("Final Score: " + PlayerController.currentScore);
            Debug.Log("High Score: " + PlayerPrefs.GetInt("HighScore", 0));
            Debug.Log("Press R: restart, M: menu, Q: quit");
            Debug.Log("==================");
        }

        StartCoroutine(HandleFallbackInput());
    }

    private System.Collections.IEnumerator HandleFallbackInput()
    {
        while (gameOver && !gameWon)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                RestartGame();
                yield break;
            }
            else if (Input.GetKeyDown(KeyCode.M))
            {
                ReturnToMainMenu();
                yield break;
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                QuitApplication();
                yield break;
            }

            yield return null;
        }
    }

    // ===== BUTTON SYSTEM =====
    private void SetupButtonListeners()
    {
        if (enableDebugLogs)
            Debug.Log("🎮 Setting up button listeners...");

        try
        {
            // Game Over buttons
            if (gameOverRestartButton != null)
            {
                gameOverRestartButton.onClick.RemoveAllListeners();
                gameOverRestartButton.onClick.AddListener(RestartGame);
                if (enableDebugLogs)
                    Debug.Log("✅ Restart button listener added");
            }

            if (gameOverMainMenuButton != null)
            {
                gameOverMainMenuButton.onClick.RemoveAllListeners();
                gameOverMainMenuButton.onClick.AddListener(ReturnToMainMenu);
                if (enableDebugLogs)
                    Debug.Log("✅ Main Menu button listener added");
            }

            if (gameOverQuitButton != null)
            {
                gameOverQuitButton.onClick.RemoveAllListeners();
                gameOverQuitButton.onClick.AddListener(QuitApplication);
                if (enableDebugLogs)
                    Debug.Log("✅ Quit button listener added");
            }

            // Win panel buttons
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveAllListeners();
                mainMenuButton.onClick.AddListener(SafeLoadMainMenuFromWin);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveAllListeners();
                quitButton.onClick.AddListener(SafeQuitFromWin);
            }

            if (nextLevelButton != null)
            {
                nextLevelButton.onClick.RemoveAllListeners();
                nextLevelButton.onClick.AddListener(SafeLoadNextLevel);
            }

            if (winMainMenuButton != null)
            {
                winMainMenuButton.onClick.RemoveAllListeners();
                winMainMenuButton.onClick.AddListener(SafeLoadMainMenuFromWin);
            }

            if (winQuitButton != null)
            {
                winQuitButton.onClick.RemoveAllListeners();
                winQuitButton.onClick.AddListener(SafeQuitFromWin);
            }

            // Pause menu buttons
            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveAllListeners();
                resumeButton.onClick.AddListener(ResumeGame);
            }

            if (pauseMainMenuButton != null)
            {
                pauseMainMenuButton.onClick.RemoveAllListeners();
                pauseMainMenuButton.onClick.AddListener(SafeLoadMainMenuFromPause);
            }

            if (pauseQuitButton != null)
            {
                pauseQuitButton.onClick.RemoveAllListeners();
                pauseQuitButton.onClick.AddListener(SafeQuitFromPause);
            }

            if (enableDebugLogs)
                Debug.Log("✅ All button listeners setup completed");
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Button listener setup failed: " + e.Message);
        }
    }

    // ===== GAME OVER BUTTON METHODS =====
    public void RestartGame()
    {
        if (enableDebugLogs)
            Debug.Log("🔄 Restarting game...");

        gameOver = false;
        gameWon = false;
        Time.timeScale = 1f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        SafeRestartLevel();
    }

    public void ReturnToMainMenu()
    {
        if (enableDebugLogs)
            Debug.Log("🏠 Returning to main menu...");

        gameOver = false;
        gameWon = false;
        Time.timeScale = 1f;

        SafeReturnToMainMenu();
    }

    public void QuitApplication()
    {
        if (enableDebugLogs)
            Debug.Log("🔚 Quitting application...");

        SafeQuitApplication();
    }

    // ===== SAFE METHODS =====
    private void SafeRestartLevel()
    {
        try
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.RestartCurrentLevel();
                if (enableDebugLogs)
                    Debug.Log("✅ Restart via GameManager successful");
            }
            else
            {
                string currentScene = SceneManager.GetActiveScene().name;
                SceneManager.LoadScene(currentScene);
                if (enableDebugLogs)
                    Debug.Log("✅ Fallback restart successful");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Restart failed: " + e.Message);
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentSceneIndex);
        }
    }

    private void SafeReturnToMainMenu()
    {
        try
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.LoadMainMenu();
                if (enableDebugLogs)
                    Debug.Log("✅ Main Menu via GameManager successful");
            }
            else
            {
                LoadMainMenuFallback();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Main Menu load failed: " + e.Message);
            LoadMainMenuFallback();
        }
    }

    private void LoadMainMenuFallback()
    {
        string[] possibleMainMenuNames = { "MainMenu", "Menu", "StartMenu", "Main" };

        foreach (string sceneName in possibleMainMenuNames)
        {
            try
            {
                if (Application.CanStreamedLevelBeLoaded(sceneName))
                {
                    SceneManager.LoadScene(sceneName);
                    if (enableDebugLogs)
                        Debug.Log("✅ Fallback main menu loaded: " + sceneName);
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
            SceneManager.LoadScene(0);
            if (enableDebugLogs)
                Debug.Log("✅ Loaded first scene as main menu");
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ All main menu fallbacks failed: " + e.Message);
        }
    }

    private void SafeQuitApplication()
    {
        try
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.QuitGame();
                if (enableDebugLogs)
                    Debug.Log("✅ Quit via GameManager successful");
            }
            else
            {
                QuitGameDirect();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Quit via GameManager failed: " + e.Message);
            QuitGameDirect();
        }
    }

    private void QuitGameDirect()
    {
        if (enableDebugLogs)
            Debug.Log("🔚 Direct application quit");

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

    // ===== CONTEXT-SPECIFIC SAFE METHODS =====
    private void SafeLoadMainMenuFromWin()
    {
        gameWon = false;
        Time.timeScale = 1f;
        SafeReturnToMainMenu();
    }

    private void SafeLoadMainMenuFromPause()
    {
        gamePaused = false;
        Time.timeScale = 1f;
        SafeReturnToMainMenu();
    }

    private void SafeQuitFromWin()
    {
        gameWon = false;
        Time.timeScale = 1f;
        SafeQuitApplication();
    }

    private void SafeQuitFromPause()
    {
        gamePaused = false;
        Time.timeScale = 1f;
        SafeQuitApplication();
    }

    private void SafeLoadNextLevel()
    {
        gameWon = false;
        Time.timeScale = 1f;

        try
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.LoadNextLevel();
            }
            else
            {
                SafeRestartLevel();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Next level load failed: " + e.Message);
            SafeRestartLevel();
        }
    }

    // ===== PAUSE/RESUME SYSTEM =====
    public void PauseGame()
    {
        gamePaused = true;
        Time.timeScale = 0f;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        if (enableDebugLogs)
            Debug.Log("⏸️ Game Paused");
    }

    public void ResumeGame()
    {
        gamePaused = false;
        Time.timeScale = 1f;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (enableDebugLogs)
            Debug.Log("▶️ Game Resumed");
    }

    // ===== UTILITY METHODS =====
    private string GetCurrentLevelDisplayName()
    {
        string sceneName = SceneManager.GetActiveScene().name.ToLower();

        switch (sceneName)
        {
            case "scene1": case "level1": case "menuscene": return "Level 1";
            case "scene2": case "level2": return "Level 2";
            case "scene3": case "level3": return "Level 3";
            case "bosslevel": case "finallevel": return "Boss Level";
            default: return "Unknown Level";
        }
    }

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

    // ===== PUBLIC GETTERS =====
    public bool IsGameWon() { return gameWon; }
    public bool IsGameOver() { return gameOver; }
    public bool IsGamePaused() { return gamePaused; }
    public int GetCurrentScore() { return PlayerController.currentScore; }

    // ===== TEST METHODS =====
    public void TestScoreIncrease(int points = 50)
    {
        AddScoreAndUpdateUI(points);
    }

    public void TestAllButtons()
    {
        if (enableDebugLogs)
        {
            Debug.Log("🧪 === BUTTON TEST ===");
            Debug.Log("Restart Button: " + (gameOverRestartButton != null ? "✅" : "❌"));
            Debug.Log("Main Menu Button: " + (gameOverMainMenuButton != null ? "✅" : "❌"));
            Debug.Log("Quit Button: " + (gameOverQuitButton != null ? "✅" : "❌"));
            Debug.Log("GameManager.instance: " + (GameManager.instance != null ? "✅" : "❌"));
            Debug.Log("==================");
        }
    }

    public void ManualRestartGame()
    {
        Debug.Log("🧪 Manual Restart triggered");
        RestartGame();
    }

    public void ManualReturnToMainMenu()
    {
        Debug.Log("🧪 Manual Main Menu triggered");
        ReturnToMainMenu();
    }

    public void ManualQuitApplication()
    {
        Debug.Log("🧪 Manual Quit triggered");
        QuitApplication();
    }

    public void EnableDebugMode(bool enable)
    {
        enableDebugLogs = enable;
        showDebugInfo = enable;
        if (enableDebugLogs)
            Debug.Log("🎮 GameStateManager Debug mode " + (enable ? "enabled" : "disabled"));
    }

    public void SetWinByEnemyCount(bool enable)
    {
        winByEnemyCount = enable;
        if (enableDebugLogs)
            Debug.Log("🎮 Win by Enemy Count: " + (enable ? "Enabled" : "Disabled"));
    }
}