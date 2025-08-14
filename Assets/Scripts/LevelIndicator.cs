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

    void Start()
    {
        level2Spawner = FindObjectOfType<EnemySpawnerLevel2>();
        UpdateLevelDisplay();

        // Hide unused UI elements
        if (waveText != null && !showWaveInfo) waveText.gameObject.SetActive(false);
        if (timeText != null && !showTimeInfo) timeText.gameObject.SetActive(false);
        if (enemyCountText != null && !showEnemyCount) enemyCountText.gameObject.SetActive(false);
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
        if (waveText != null && level2Spawner != null)
        {
            waveText.text = "Wave: " + level2Spawner.GetCurrentWave();
        }
    }

    private void UpdateTimeDisplay()
    {
        if (timeText != null)
        {
            float gameTime = level2Spawner != null ? level2Spawner.GetGameTime() : Time.time;
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

    // Public methods để update từ bên ngoài
    public void SetLevelName(string levelName)
    {
        if (levelText != null)
        {
            levelText.text = "Level: " + levelName;
        }
    }
}