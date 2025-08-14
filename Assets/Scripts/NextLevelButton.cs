using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NextLevelButton : MonoBehaviour
{
    [Header("Cấu hình Scene")]
    [SerializeField] private string targetSceneName = "Scene2"; // Scene bạn muốn chuyển đến
    [SerializeField] private bool keepScore = true; // Có giữ điểm số không
    [SerializeField] private bool hideWinPanel = true; // Có ẩn win panel không

    [Header("Tên Win Panel (tự động tìm)")]
    [SerializeField]
    private string[] possibleWinPanelNames = {
        "WinPanel",
        "GameWinPanel",
        "VictoryPanel",
        "Win Panel",
        "Panel_Win"
    };

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private Button nextButton;
    private GameObject foundWinPanel;

    void Start()
    {
        // Tự động lấy Button component từ GameObject này
        nextButton = GetComponent<Button>();
        if (nextButton != null)
        {
            // Xóa tất cả listeners cũ để tránh conflict
            nextButton.onClick.RemoveAllListeners();

            // Thêm listener mới
            nextButton.onClick.AddListener(LoadTargetScene);

            if (enableDebugLogs)
                Debug.Log("✅ Next Level Button đã được cấu hình cho scene: " + targetSceneName);
        }
        else
        {
            Debug.LogError("❌ Không tìm thấy Button component trên GameObject: " + gameObject.name);
        }

        // Tự động tìm Win Panel
        FindWinPanel();
    }

    // Tự động tìm Win Panel
    private void FindWinPanel()
    {
        foreach (string panelName in possibleWinPanelNames)
        {
            GameObject panel = GameObject.Find(panelName);
            if (panel != null)
            {
                foundWinPanel = panel;
                if (enableDebugLogs)
                    Debug.Log("✅ Tìm thấy Win Panel: " + panel.name);
                return;
            }
        }

        if (enableDebugLogs)
            Debug.LogWarning("⚠️ Không tìm thấy Win Panel. Sẽ skip việc ẩn panel.");
    }

    // Method chính được gọi khi nhấn nút
    public void LoadTargetScene()
    {
        if (enableDebugLogs)
            Debug.Log("🎮 Next Level Button được nhấn - Đang tải: " + targetSceneName);

        // Reset game state
        Time.timeScale = 1f; // Khôi phục thời gian game

        // Xử lý điểm số
        if (!keepScore)
        {
            int oldScore = PlayerController.currentScore;
            PlayerController.currentScore = 0;
            if (enableDebugLogs)
                Debug.Log("🔄 Điểm số đã reset: " + oldScore + " → 0");
        }
        else
        {
            if (enableDebugLogs)
                Debug.Log("💰 Giữ điểm số: " + PlayerController.currentScore);
        }

        // Ẩn Win Panel nếu tìm thấy
        if (hideWinPanel && foundWinPanel != null)
        {
            foundWinPanel.SetActive(false);
            if (enableDebugLogs)
                Debug.Log("✅ Win Panel đã được ẩn: " + foundWinPanel.name);
        }

        // Reset GameStateManager state nếu có
        ResetGameStateManager();

        // Load scene mong muốn
        LoadSceneWithErrorHandling();
    }

    // Reset GameStateManager để tránh conflict
    private void ResetGameStateManager()
    {
        try
        {
            if (GameStateManager.instance != null)
            {
                // Reset các flags trong GameStateManager
                var gameStateType = typeof(GameStateManager);
                var gameWonField = gameStateType.GetField("gameWon",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);
                var gameOverField = gameStateType.GetField("gameOver",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

                if (gameWonField != null)
                    gameWonField.SetValue(GameStateManager.instance, false);

                if (gameOverField != null)
                    gameOverField.SetValue(GameStateManager.instance, false);

                if (enableDebugLogs)
                    Debug.Log("✅ GameStateManager state đã được reset");
            }
        }
        catch (System.Exception e)
        {
            if (enableDebugLogs)
                Debug.LogWarning("⚠️ Không thể reset GameStateManager: " + e.Message);
        }
    }

    // Load scene với xử lý lỗi đầy đủ
    private void LoadSceneWithErrorHandling()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("❌ Target Scene Name trống! Vui lòng set trong Inspector.");
            return;
        }

        try
        {
            // Kiểm tra scene có tồn tại trong Build Settings không
            if (Application.CanStreamedLevelBeLoaded(targetSceneName))
            {
                SceneManager.LoadScene(targetSceneName);
                if (enableDebugLogs)
                    Debug.Log("✅ Scene đã tải thành công: " + targetSceneName);
            }
            else
            {
                Debug.LogError("❌ Scene không tìm thấy trong Build Settings: " + targetSceneName);

                // Thử các scene fallback
                TryLoadFallbackScenes();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Lỗi khi tải scene: " + e.Message);
            TryLoadFallbackScenes();
        }
    }

    // Thử load các scene dự phòng
    private void TryLoadFallbackScenes()
    {
        string[] fallbackScenes = { "Scene1", "Scene2", "Scene3", "MainMenu", "Main", "Menu" };

        foreach (string fallback in fallbackScenes)
        {
            try
            {
                if (Application.CanStreamedLevelBeLoaded(fallback))
                {
                    SceneManager.LoadScene(fallback);
                    if (enableDebugLogs)
                        Debug.Log("✅ Fallback scene đã tải: " + fallback);
                    return;
                }
            }
            catch
            {
                continue; // Thử scene tiếp theo
            }
        }

        // Fallback cuối cùng - load scene đầu tiên trong Build Settings
        try
        {
            if (SceneManager.sceneCountInBuildSettings > 0)
            {
                SceneManager.LoadScene(0);
                if (enableDebugLogs)
                    Debug.Log("✅ Ultimate fallback: Đã tải scene đầu tiên");
            }
            else
            {
                Debug.LogError("❌ Không có scene nào trong Build Settings!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Tất cả fallbacks thất bại: " + e.Message);
        }
    }

    // ===== METHODS CÔNG KHAI ĐỂ SỬ DỤNG TỪ INSPECTOR/CODE KHÁC =====

    /// <summary>
    /// Thay đổi scene đích từ code khác
    /// </summary>
    public void SetTargetScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            targetSceneName = sceneName;
            if (enableDebugLogs)
                Debug.Log("🎮 Target scene đã thay đổi thành: " + targetSceneName);
        }
        else
        {
            Debug.LogWarning("⚠️ Scene name không hợp lệ!");
        }
    }

    /// <summary>
    /// Bật/tắt việc giữ điểm số
    /// </summary>
    public void SetKeepScore(bool keep)
    {
        keepScore = keep;
        if (enableDebugLogs)
            Debug.Log("💰 Keep score đã set thành: " + keep);
    }

    /// <summary>
    /// Bật/tắt việc ẩn Win Panel
    /// </summary>
    public void SetHideWinPanel(bool hide)
    {
        hideWinPanel = hide;
        if (enableDebugLogs)
            Debug.Log("👁️ Hide win panel đã set thành: " + hide);
    }

    /// <summary>
    /// Test button functionality (gọi từ Inspector)
    /// </summary>
    public void TestButton()
    {
        if (enableDebugLogs)
        {
            Debug.Log("🧪 === NEXT BUTTON TEST ===");
            Debug.Log("Target Scene: " + targetSceneName);
            Debug.Log("Keep Score: " + keepScore);
            Debug.Log("Hide Win Panel: " + hideWinPanel);
            Debug.Log("Button Component: " + (nextButton != null ? "✅" : "❌"));
            Debug.Log("Win Panel Found: " + (foundWinPanel != null ? foundWinPanel.name : "❌"));
            Debug.Log("Current Score: " + PlayerController.currentScore);
            Debug.Log("=========================");
        }
    }

    /// <summary>
    /// Manually trigger scene load (không qua button click)
    /// </summary>
    public void ManualLoadTargetScene()
    {
        if (enableDebugLogs)
            Debug.Log("🔧 Manual load triggered cho scene: " + targetSceneName);

        LoadTargetScene();
    }

    // ===== INTEGRATION VỚI GAMEMANAGER =====

    /// <summary>
    /// Load scene thông qua GameManager nếu có
    /// </summary>
    public void LoadThroughGameManager()
    {
        try
        {
            if (GameManager.instance != null)
            {
                // Sử dụng GameManager để load scene
                if (targetSceneName.ToLower().Contains("scene1"))
                    GameManager.instance.LoadGameScene();
                else if (targetSceneName.ToLower().Contains("scene2"))
                    GameManager.instance.LoadLevel2();
                else if (targetSceneName.ToLower().Contains("scene3"))
                    GameManager.instance.LoadLevel3();
                else if (targetSceneName.ToLower().Contains("boss"))
                    GameManager.instance.LoadBossLevel();
                else if (targetSceneName.ToLower().Contains("menu"))
                    GameManager.instance.LoadMainMenu();
                else
                    LoadTargetScene(); // Fallback to direct load

                if (enableDebugLogs)
                    Debug.Log("✅ Scene loaded qua GameManager");
            }
            else
            {
                LoadTargetScene(); // Direct load nếu không có GameManager
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ GameManager load failed: " + e.Message);
            LoadTargetScene(); // Fallback to direct load
        }
    }
}