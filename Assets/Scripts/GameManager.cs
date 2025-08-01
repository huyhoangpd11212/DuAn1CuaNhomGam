using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public void LoadGameScene()
    {
        // Tên scene bạn muốn tải (phải khớp chính xác với tên scene trong Build Settings)
        string sceneToLoad = "Scene1"; // THAY THẾ "GameScene" BẰNG "Scene1" Ở ĐÂY

        // Kiểm tra xem scene có tồn tại trong Build Settings không
        if (SceneUtility.GetBuildIndexByScenePath(sceneToLoad) != -1)
        {
            SceneManager.LoadScene(sceneToLoad); // Tải scene
            Debug.Log("Loading scene: " + sceneToLoad);
        }
        else
        {
            Debug.LogError("Scene '" + sceneToLoad + "' not found in Build Settings! Please add it.");
        }
    }

    // Các hàm khác nếu có
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}