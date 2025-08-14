using UnityEngine;

public class ScoreDebugger : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            PlayerController.AddScore(100);
            Debug.Log("🧪 TEST: Added 100 points");
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            PlayerController.currentScore = 0;
            if (GameStateManager.instance != null)
                GameStateManager.instance.ForceUpdateHUD();
            Debug.Log("🔄 TEST: Reset score");
        }
    }

    void OnGUI()
    {
        GUI.Box(new Rect(10, Screen.height - 100, 300, 80), "");
        GUI.Label(new Rect(15, Screen.height - 95, 290, 20), "Debug Score: " + PlayerController.currentScore);
        GUI.Label(new Rect(15, Screen.height - 75, 290, 20), "Press T: Add 100 points");
        GUI.Label(new Rect(15, Screen.height - 55, 290, 20), "Press R: Reset score");

        GameStateManager gsm = GameStateManager.instance;
        if (gsm != null)
        {
            GUI.Label(new Rect(15, Screen.height - 35, 290, 20), "GSM Status: " + (gsm.IsGameWon() ? "Won" : "Active"));
        }
    }
}