using UnityEngine;
using UnityEngine.UI;

public class SimpleResetButton : MonoBehaviour
{
    void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            // Clear existing listeners
            button.onClick.RemoveAllListeners();

            // Add reset listener
            button.onClick.AddListener(ResetScore);

            Debug.Log("✅ Simple Reset Button setup complete");
        }
        else
        {
            Debug.LogError("❌ No Button component found!");
        }
    }

    public void ResetScore()
    {
        Debug.Log("🔄 Simple Reset Button clicked!");

        // Method 1: Via GameManager if available
        if (GameManager.instance != null)
        {
            GameManager.instance.ResetScore();
            Debug.Log("✅ Reset via GameManager.instance");
            return;
        }

        // Method 2: Find GameManager in scene
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.ResetScore();
            Debug.Log("✅ Reset via FindObjectOfType<GameManager>");
            return;
        }

        // Method 3: Direct reset as fallback
        int oldScore = PlayerController.currentScore;
        PlayerController.currentScore = 0;

        // Try to update UI
        GameStateManager gsm = FindObjectOfType<GameStateManager>();
        if (gsm != null)
        {
            gsm.ForceUpdateHUD();
        }

        Debug.Log("✅ Direct reset fallback: " + oldScore + " → 0");
    }
}