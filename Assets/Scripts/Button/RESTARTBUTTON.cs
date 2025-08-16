using UnityEngine;
using UnityEngine.UI;

public class RestartButton : MonoBehaviour
{
    [Header("Restart Button Settings")]
    [SerializeField] private bool debugButtonClicks = true;
    [SerializeField] private bool resetScoreOnRestart = true;
    [SerializeField] private bool confirmBeforeRestart = false;
    [SerializeField] private string confirmMessage = "Are you sure you want to restart the level?";

    private Button restartButton;

    void Start()
    {
        restartButton = GetComponent<Button>();

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartButtonClicked);
            if (debugButtonClicks)
                Debug.Log("🔄 Restart button listener added");
        }
        else
        {
            Debug.LogError("❌ No Button component found on " + gameObject.name);
        }
    }

    public void OnRestartButtonClicked()
    {
        if (debugButtonClicks)
            Debug.Log("🎮 Restart button clicked!");

        if (confirmBeforeRestart)
        {
            if (Application.isEditor)
            {
                if (UnityEditor.EditorUtility.DisplayDialog("Restart Level", confirmMessage, "Yes", "No"))
                {
                    PerformRestart();
                }
            }
            else
            {
                // In build, just restart (no dialog available)
                PerformRestart();
            }
        }
        else
        {
            PerformRestart();
        }
    }

    private void PerformRestart()
    {
        if (GameManager.instance != null)
        {
            if (debugButtonClicks)
                Debug.Log("🔄 Calling GameManager.RestartGameButton()");

            if (resetScoreOnRestart)
            {
                GameManager.instance.RestartGameButton(); // This resets score AND restarts level
            }
            else
            {
                GameManager.instance.RestartCurrentLevel(); // This only restarts level
            }
        }
        else
        {
            Debug.LogError("❌ GameManager.instance is null! Cannot restart game.");

            // Fallback restart
            if (debugButtonClicks)
                Debug.Log("🔄 Using fallback restart method");

            if (resetScoreOnRestart)
            {
                PlayerController.currentScore = 0;
            }

            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }
    }

    // ✅ Public methods for other scripts to call
    public void RestartLevel()
    {
        OnRestartButtonClicked();
    }

    public void RestartWithoutScoreReset()
    {
        bool originalSetting = resetScoreOnRestart;
        resetScoreOnRestart = false;
        PerformRestart();
        resetScoreOnRestart = originalSetting;
    }
}