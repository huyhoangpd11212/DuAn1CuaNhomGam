using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameStateManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject gameWinPanel;
    
    [Header("Game Over Elements")]
    [SerializeField] private Text gameOverFinalScoreText;
    [SerializeField] private Text gameOverHighScoreText;
    [SerializeField] private Button gameOverRestartButton;
    [SerializeField] private Button gameOverMainMenuButton;
    [SerializeField] private Button gameOverQuitButton;
    
    [Header("Game Win Elements")]
    [SerializeField] private Text gameWinFinalScoreText;
    [SerializeField] private Text gameWinHighScoreText;
    [SerializeField] private Text levelCompleteText;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button gameWinRestartButton;
    [SerializeField] private Button gameWinMainMenuButton;
    [SerializeField] private Button gameWinQuitButton;
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private CanvasGroup gameOverCanvasGroup;
    [SerializeField] private CanvasGroup gameWinCanvasGroup;
    
    [Header("Game Settings")]
    [SerializeField] private int winScore = 1000;
    [SerializeField] private int bonusScore = 500;
    [SerializeField] private string nextSceneName = "Level2";
    
    [Header("Win Conditions")]
    [Tooltip("Thắng khi đạt đủ điểm số")]
    [SerializeField] private bool winByScore = true;
    [Tooltip("Thắng khi tiêu diệt hết enemy")]
    [SerializeField] private bool winByEnemyCount = true;
    [Tooltip("Thắng khi sống đủ thời gian")]
    [SerializeField] private bool winByTime = false;
    [SerializeField] private float gameTimeLimit = 180f; // 3 phút
    
    private float gameStartTime;
    
    [Header("Sound Effects")]
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip gameWinSound;
    
    // Static instance
    public static GameStateManager instance;
    
    // Game state
    private bool isGameOver = false;
    private bool isGameWin = false;
    
    // Prevent immediate win condition check after restart
    private float gameStartDelay = 3f; // 3 giây delay
    private bool canCheckWinCondition = false;
    
    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        InitializePanels();
        SetupButtons();
        gameStartTime = Time.time; // Record thời gian bắt đầu game
        
        // Bắt đầu coroutine để enable win condition check sau delay
        StartCoroutine(EnableWinConditionCheckAfterDelay());
        
        Debug.Log($"🎮 GameStateManager initialized!");
        Debug.Log($"📊 Win Conditions - Score: {(winByScore ? winScore.ToString() : "OFF")} | Enemy: {(winByEnemyCount ? "ON" : "OFF")} | Time: {(winByTime ? gameTimeLimit + "s" : "OFF")}");
        Debug.Log($"⏰ Win condition check will be enabled after {gameStartDelay} seconds");
    }
    
    private System.Collections.IEnumerator EnableWinConditionCheckAfterDelay()
    {
        canCheckWinCondition = false;
        yield return new WaitForSeconds(gameStartDelay);
        canCheckWinCondition = true;
        Debug.Log("✅ Win condition check is now ENABLED!");
    }
    
    private void Update()
    {
        CheckWinCondition();
        CheckGameOverCondition();
        HandleTestKeys();
    }
    
    private void InitializePanels()
    {
        // Ẩn tất cả panels khi bắt đầu
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        if (gameWinPanel != null)
        {
            gameWinPanel.SetActive(false);
        }
    }
    
    private void SetupButtons()
    {
        // Game Over buttons
        if (gameOverRestartButton != null)
            gameOverRestartButton.onClick.AddListener(() => RestartGame("Game Over"));
            
        if (gameOverMainMenuButton != null)
            gameOverMainMenuButton.onClick.AddListener(() => GoToMainMenu("Game Over"));
            
        if (gameOverQuitButton != null)
            gameOverQuitButton.onClick.AddListener(QuitGame);
        
        // Game Win buttons
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(LoadNextLevel);
            
        if (gameWinRestartButton != null)
            gameWinRestartButton.onClick.AddListener(() => RestartGame("Game Win"));
            
        if (gameWinMainMenuButton != null)
            gameWinMainMenuButton.onClick.AddListener(() => GoToMainMenu("Game Win"));
            
        if (gameWinQuitButton != null)
            gameWinQuitButton.onClick.AddListener(QuitGame);
    }
    
    private void CheckWinCondition()
    {
        // Không check win condition nếu:
        // - Game đã over hoặc win
        // - Game đang pause
        // - Chưa đến thời gian được check (sau restart)
        if (isGameOver || isGameWin || PlayerController.isGamePaused || !canCheckWinCondition) 
            return;
        
        bool shouldWin = false;
        string winReason = "";
        
        // Điều kiện 1: Thắng bằng điểm số
        if (winByScore && PlayerController.currentScore >= winScore)
        {
            shouldWin = true;
            winReason = $"Reached target score: {PlayerController.currentScore}/{winScore}";
        }
        
        // Điều kiện 2: Thắng bằng tiêu diệt hết enemy
        if (winByEnemyCount && !shouldWin)
        {
            int enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
            if (enemyCount <= 0)
            {
                shouldWin = true;
                winReason = "All enemies defeated!";
            }
        }
        
        // Điều kiện 3: Thắng bằng thời gian sống sót
        if (winByTime && !shouldWin)
        {
            float gameTime = Time.time - gameStartTime;
            if (gameTime >= gameTimeLimit)
            {
                shouldWin = true;
                winReason = $"Survived {gameTimeLimit} seconds!";
            }
        }
        
        if (shouldWin)
        {
            Debug.Log($"🎉 WIN CONDITION REACHED! Reason: {winReason}");
            TriggerGameWin();
        }
    }
    
    private void CheckGameOverCondition()
    {
        if (isGameOver || isGameWin) return;
        
        // Tự động trigger game over nếu không có player trong scene
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player == null)
        {
            Debug.Log("💀 AUTO GAME OVER: No player found in scene");
            TriggerGameOver();
            return;
        }
        
        // Có thể thêm các điều kiện Game Over khác ở đây:
        // - Hết thời gian (countdown timer)
        // - Không còn lives
        // - etc.
    }
    
    private void HandleTestKeys()
    {
        // Test keys
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("🔥 Testing Game Over");
            TriggerGameOver();
        }
        
        if (Input.GetKeyDown(KeyCode.V))
        {
            Debug.Log("🎉 Testing Game Win");
            TriggerGameWin();
        }
        
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("🔄 Hiding all UI panels");
            HideAllPanels();
        }
        
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("➕ Adding 100 points for testing");
            PlayerController.AddScore(100);
        }
        
        // THÊM: Test kill all enemies
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("💀 Killing all enemies for testing");
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemies)
            {
                Destroy(enemy);
            }
        }
    }
    
    // Method để lấy thông tin trạng thái game
    public string GetGameStatusInfo()
    {
        int enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        float gameTime = Time.time - gameStartTime;
        
        string info = $"Score: {PlayerController.currentScore}";
        
        if (winByScore) info += $" (Target: {winScore})";
        if (winByEnemyCount) info += $" | Enemies: {enemyCount}";
        if (winByTime) info += $" | Time: {gameTime:F1}s/{gameTimeLimit}s";
        
        return info;
    }
    
    // Public Methods
    public void TriggerGameOver()
    {
        if (isGameOver || isGameWin) return;
        
        isGameOver = true;
        Debug.Log("💀 GAME OVER TRIGGERED");
        
        // Phát âm thanh Game Over bằng AudioManager
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayGameOverSound();
        }
        
        ShowPanel(gameOverPanel, gameOverCanvasGroup, "GAME OVER", Color.red, 
                 gameOverFinalScoreText, gameOverHighScoreText, gameOverSound);
    }
    
    public void TriggerGameWin()
    {
        if (isGameOver || isGameWin) return;
        
        isGameWin = true;
        Debug.Log("🎉 GAME WIN TRIGGERED");
        
        // Phát âm thanh Victory bằng AudioManager
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayVictorySound();
        }
        
        // Thêm bonus score
        PlayerController.AddScore(bonusScore);
        
        ShowPanel(gameWinPanel, gameWinCanvasGroup, "🎉 LEVEL COMPLETE! 🎉", Color.yellow,
                 gameWinFinalScoreText, gameWinHighScoreText, gameWinSound);
                 
        // Cập nhật level complete text
        if (levelCompleteText != null)
        {
            levelCompleteText.text = "EXCELLENT WORK!";
        }
    }
    
    private void ShowPanel(GameObject panel, CanvasGroup canvasGroup, string titleText, Color titleColor,
                          Text scoreText, Text highScoreText, AudioClip soundEffect)
    {
        if (panel == null) 
        {
            Debug.LogError($"Panel is null!");
            return;
        }
        
        // Hiển thị panel
        panel.SetActive(true);
        
        // Pause game
        Time.timeScale = 0f;
        PlayerController.isGamePaused = true;
        
        // Force update UI
        Canvas.ForceUpdateCanvases();
        
        // Cập nhật điểm số
        UpdateScoreDisplay(scoreText, titleText, titleColor);
        
        // Cập nhật high score
        UpdateHighScore(highScoreText);
        
        // Animation fade in
        if (canvasGroup != null)
        {
            StartCoroutine(FadeInPanel(canvasGroup));
        }
        
        // Phát âm thanh
        PlaySound(soundEffect);
        
        Debug.Log($"Panel displayed: {panel.name}");
    }
    
    private void UpdateScoreDisplay(Text scoreText, string titleText, Color titleColor)
    {
        if (scoreText != null)
        {
            scoreText.text = $"{titleText}\n\nFinal Score: {PlayerController.currentScore:N0}";
            scoreText.color = titleColor;
        }
    }
    
    private void UpdateHighScore(Text highScoreText)
    {
        if (highScoreText == null) return;
        
        int currentHighScore = PlayerPrefs.GetInt("HighScore", 0);
        
        if (PlayerController.currentScore > currentHighScore)
        {
            PlayerPrefs.SetInt("HighScore", PlayerController.currentScore);
            PlayerPrefs.Save();
            currentHighScore = PlayerController.currentScore;
            
            highScoreText.text = $"🏆 NEW HIGH SCORE: {currentHighScore:N0} 🏆";
            highScoreText.color = Color.yellow;
            
            Debug.Log($"🏆 New High Score: {currentHighScore}");
        }
        else
        {
            highScoreText.text = $"High Score: {currentHighScore:N0}";
            highScoreText.color = Color.white;
        }
    }
    
    private IEnumerator FadeInPanel(CanvasGroup canvasGroup)
    {
        if (canvasGroup == null) yield break;
        
        canvasGroup.alpha = 0f;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            // Ưu tiên dùng AudioManager nếu có
            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlaySFX(clip);
            }
            else
            {
                // Fallback: dùng AudioSource tạm thời
                GameObject tempAudio = new GameObject("TempAudio");
                AudioSource audioSource = tempAudio.AddComponent<AudioSource>();
                audioSource.clip = clip;
                audioSource.Play();
                Destroy(tempAudio, clip.length);
            }
        }
    }
    
    public void HideAllPanels()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameWinPanel != null) gameWinPanel.SetActive(false);
        
        // Resume game
        Time.timeScale = 1f;
        PlayerController.isGamePaused = false;
        
        // Reset states
        isGameOver = false;
        isGameWin = false;
    }
    
    // Button Event Handlers
    public void LoadNextLevel()
    {
        Debug.Log($"🚀 Loading next level: {nextSceneName}");
        ResetGameState();
        SceneManager.LoadScene(nextSceneName);
    }
    
    public void RestartGame(string source = "")
    {
        Debug.Log($"🔄 Restarting game from: {source}");
        ResetGameState();
        PlayerController.currentScore = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void GoToMainMenu(string source = "")
    {
        Debug.Log($"🏠 Going to main menu from: {source}");
        ResetGameState();
        PlayerController.currentScore = 0;
        SceneManager.LoadScene(1); // Main menu scene
    }
    
    public void QuitGame()
    {
        Debug.Log("🚪 Quitting game");
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    
    private void ResetGameState()
    {
        Time.timeScale = 1f;
        PlayerController.isGamePaused = false;
        isGameOver = false;
        isGameWin = false;
        canCheckWinCondition = false; // Tắt win condition check để tránh trigger ngay lập tức
        
        Debug.Log("🔄 Game state reset - Win condition check disabled temporarily");
    }
    
    // Static Methods để gọi từ bên ngoài
    public static void DisplayGameOver()
    {
        if (instance != null)
        {
            instance.TriggerGameOver();
        }
        else
        {
            Debug.LogError("❌ GameStateManager instance not found!");
        }
    }
    
    public static void DisplayGameWin()
    {
        if (instance != null)
        {
            instance.TriggerGameWin();
        }
        else
        {
            Debug.LogError("❌ GameStateManager instance not found!");
        }
    }
    
    // Utility Methods
    public void SetWinScore(int newWinScore)
    {
        winScore = newWinScore;
        Debug.Log($"🎯 Win score changed to: {winScore}");
    }
    
    public int GetWinScore() => winScore;
    public bool IsGameOver() => isGameOver;
    public bool IsGameWin() => isGameWin;
    public bool CanCheckWinCondition() => canCheckWinCondition;
    
    // Method để manual enable win condition check (nếu cần)
    public void EnableWinConditionCheck()
    {
        canCheckWinCondition = true;
        Debug.Log("✅ Win condition check manually ENABLED!");
    }
    
    public void DisableWinConditionCheck()
    {
        canCheckWinCondition = false;
        Debug.Log("❌ Win condition check manually DISABLED!");
    }
    
    // Events khi player chết (gọi từ PlayerController)
    public void OnPlayerDeath()
    {
        Debug.Log("💀 Player died - triggering game over");
        TriggerGameOver();
    }
}
