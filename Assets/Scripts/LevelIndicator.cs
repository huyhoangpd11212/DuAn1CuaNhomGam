using UnityEngine;
using UnityEngine.UI;

public class LevelIndicator : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Text levelText;
    [SerializeField] private Text waveText;
    [SerializeField] private Text timeText;
    [SerializeField] private Text enemyCountText;

    [Header("Settings")]
    [SerializeField] private bool showWaveInfo = true;
    [SerializeField] private bool showTimeInfo = true;
    [SerializeField] private bool showEnemyCount = true;

    private EnemySpawnerLevel2 level2Spawner;
    private float gameStartTime;
    private int currentWave = 1; // ✅ LOCAL TRACKING

    void Start()
    {
        level2Spawner = FindObjectOfType<EnemySpawnerLevel2>();
        gameStartTime = Time.time; // ✅ TRACK TIME LOCALLY
        UpdateLevelDisplay();

        // Hide unused UI elements
        if (waveText != null && !showWaveInfo) waveText.gameObject.SetActive(false);
        if (timeText != null && !showTimeInfo) timeText.gameObject.SetActive(false);
        if (enemyCountText != null && !showEnemyCount) enemyCountText.gameObject.SetActive(false);

        Debug.Log("📊 LevelIndicator initialized for " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    void Update()
    {
        if (showWaveInfo) UpdateWaveDisplay();
        if (showTimeInfo) UpdateTimeDisplay();
        if (showEnemyCount) UpdateEnemyCountDisplay();
    }

    private void UpdateLevelDisplay()
    {
        if (levelText != null)
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            levelText.text = "Level: " + GetLevelNumber(sceneName);
        }
    }

    private void UpdateWaveDisplay()
    {
        if (waveText != null)
        {
            // ✅ SỬA: Sử dụng safe method call hoặc fallback
            if (level2Spawner != null && HasMethod(level2Spawner, "GetCurrentWave"))
            {
                try
                {
                    var method = level2Spawner.GetType().GetMethod("GetCurrentWave");
                    if (method != null)
                    {
                        int wave = (int)method.Invoke(level2Spawner, null);
                        waveText.text = "Wave: " + wave;
                    }
                    else
                    {
                        waveText.text = "Wave: " + currentWave; // Fallback
                    }
                }
                catch
                {
                    waveText.text = "Wave: " + currentWave; // Fallback
                }
            }
            else
            {
                // ✅ FALLBACK: Estimate wave based on time
                int estimatedWave = Mathf.FloorToInt((Time.time - gameStartTime) / 30f) + 1;
                waveText.text = "Wave: " + estimatedWave;
            }
        }
    }

    private void UpdateTimeDisplay()
    {
        if (timeText != null)
        {
            // ✅ SỬA: Sử dụng local time tracking thay vì method call
            float gameTime = Time.time - gameStartTime;

            if (level2Spawner != null && HasMethod(level2Spawner, "GetGameTime"))
            {
                try
                {
                    var method = level2Spawner.GetType().GetMethod("GetGameTime");
                    if (method != null)
                    {
                        gameTime = (float)method.Invoke(level2Spawner, null);
                    }
                }
                catch
                {
                    // Use local time as fallback
                }
            }

            int minutes = Mathf.FloorToInt(gameTime / 60);
            int seconds = Mathf.FloorToInt(gameTime % 60);
            timeText.text = string.Format("Time: {0:00}:{1:00}", minutes, seconds);
        }
    }

    private void UpdateEnemyCountDisplay()
    {
        if (enemyCountText != null)
        {
            int currentEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
            enemyCountText.text = "Enemies: " + currentEnemies;
        }
    }

    // ✅ HELPER METHOD: Check if method exists
    private bool HasMethod(object obj, string methodName)
    {
        if (obj == null) return false;
        return obj.GetType().GetMethod(methodName) != null;
    }

    private string GetLevelNumber(string sceneName)
    {
        switch (sceneName.ToLower())
        {
            case "scene1": case "level1": case "menuscene": return "1";
            case "scene2": case "level2": return "2";
            case "scene3": case "level3": return "3";
            case "bosslevel": case "finallevel": return "BOSS";
            default: return "?";
        }
    }

    // ✅ Public methods để update từ bên ngoài
    public void SetLevelName(string levelName)
    {
        if (levelText != null)
        {
            levelText.text = "Level: " + levelName;
        }
    }

    // ✅ NEW: Manual wave update
    public void SetCurrentWave(int wave)
    {
        currentWave = wave;
        Debug.Log("📊 LevelIndicator: Set current wave to " + wave);
    }

    // ✅ NEW: Force refresh all displays
    public void RefreshAllDisplays()
    {
        UpdateLevelDisplay();
        UpdateWaveDisplay();
        UpdateTimeDisplay();
        UpdateEnemyCountDisplay();
        Debug.Log("📊 LevelIndicator: Refreshed all displays");
    }
}