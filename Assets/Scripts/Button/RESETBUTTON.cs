using UnityEngine;
using UnityEngine.UI;

public class ResetButton : MonoBehaviour
{
    [Header("Reset Button Settings")]
    [SerializeField] private bool debugButtonClicks = true;
    [SerializeField] private bool confirmBeforeReset = false;
    [SerializeField] private string confirmMessage = "Are you sure you want to reset your score?";

    private Button resetButton;

    void Start()
    {
        resetButton = GetComponent<Button>();

        if (resetButton != null)
        {
            resetButton.onClick.AddListener(OnResetButtonClicked);
            if (debugButtonClicks)
                Debug.Log("?? Reset button listener added");
        }
        else
        {
            Debug.LogError("? No Button component found on " + gameObject.name);
        }
    }

    public void OnResetButtonClicked()
    {
        if (debugButtonClicks)
            Debug.Log("?? Reset button clicked!");

        if (confirmBeforeReset)
        {
            if (Application.isEditor)
            {
                if (UnityEditor.EditorUtility.DisplayDialog("Reset Score", confirmMessage, "Yes", "No"))
                {
                    PerformReset();
                }
            }
            else
            {
                // In build, just reset (no dialog available)
                PerformReset();
            }
        }
        else
        {
            PerformReset();
        }
    }

    private void PerformReset()
    {
        if (GameManager.instance != null)
        {
            if (debugButtonClicks)
                Debug.Log("?? Calling GameManager.MasterResetScore()");

            GameManager.instance.MasterResetScore();
        }
        else
        {
            Debug.LogError("? GameManager.instance is null! Cannot reset score.");

            // Fallback reset
            if (debugButtonClicks)
                Debug.Log("?? Using fallback reset method");

            PlayerController.currentScore = 0;

            // Try to update UI manually
            GameStateManager gsm = FindObjectOfType<GameStateManager>();
            if (gsm != null)
            {
                gsm.ForceUpdateHUD();
            }
        }
    }

    // ? Public method for other scripts to call
    public void ResetScore()
    {
        OnResetButtonClicked();
    }
}