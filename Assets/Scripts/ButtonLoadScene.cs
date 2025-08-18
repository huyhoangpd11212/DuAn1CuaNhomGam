using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonLoadScene : MonoBehaviour
{
    [SerializeField] private string sceneName = "MainMenu"; // Tên scene muốn load

    // Hàm gọi khi nhấn Button
    public void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("❌ Scene name is null or empty!");
            return;
        }

        // Kiểm tra scene có nằm trong Build Settings không
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            Debug.Log("✅ Loading scene: " + sceneName);
        }
        else
        {
            Debug.LogError("❌ Scene '" + sceneName + "' chưa được add vào Build Settings!");
        }
    }
}
