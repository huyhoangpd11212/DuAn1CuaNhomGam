// Tập tin: Assets/Scripts/PauseMenu.cs
using UnityEngine;
using UnityEngine.SceneManagement; // Cần thiết để tải cảnh

public class PauseMenu : MonoBehaviour
{
    [Header("Giao diện tạm dừng")]
    [Tooltip("Kéo thả Panel chứa Menu Tạm Dừng vào đây")]
    [SerializeField] private GameObject pauseMenuUI; // Panel chứa toàn bộ giao diện menu tạm dừng

    // Biến để theo dõi trạng thái tạm dừng của trò chơi
    // Đã được định nghĩa trong PlayerController, nhưng chúng ta cũng có thể truy cập nó từ đây
    // public static bool isGamePaused = false; // Nếu bạn muốn biến này chỉ ở đây

    // Lưu tên Scene menu của bạn
    [Header("Cài đặt cảnh")]
    [Tooltip("Tên của cảnh Menu chính")]
    [SerializeField] private string menuSceneName = "Menu"; // Thay đổi tên này cho phù hợp với cảnh menu của bạn

    void Start()
    {
        // Đảm bảo menu tạm dừng ẩn khi game bắt đầu
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        // Đảm bảo game không bị tạm dừng khi bắt đầu scene
        Time.timeScale = 1f;
        PlayerController.isGamePaused = false; 
    }

    void Update()
    {
       
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (PlayerController.isGamePaused) // Nếu game đang tạm dừng
            {
                Resume(); // Tiếp tục game
            }
            else // Nếu game đang chạy
            {
                Pause(); // Tạm dừng game
            }
        }
    }

    public void Resume()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false); // Ẩn menu tạm dừng
        }
        Time.timeScale = 1f; // Đặt lại tốc độ thời gian về bình thường
        PlayerController.isGamePaused = false; // Cập nhật trạng thái game
        Debug.Log("Game đã tiếp tục.");
    }

    void Pause()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true); // Hiển thị menu tạm dừng
        }
        Time.timeScale = 0f; // Dừng mọi chuyển động trong game
        PlayerController.isGamePaused = true; // Cập nhật trạng thái game
        Debug.Log("Game đã tạm dừng.");
    }

    public void LoadMenu()
    {
        Debug.Log("Đang tải lại Menu...");
        Time.timeScale = 1f; // Đảm bảo tốc độ thời gian bình thường trước khi tải scene mới
        PlayerController.isGamePaused = false; // Đảm bảo game không bị tạm dừng khi về menu
        SceneManager.LoadScene(menuSceneName); // Tải scene menu
    }

    // Tùy chọn: Hàm này sẽ dùng cho nút "Thoát Game" trong các bản build
    public void QuitGame()
    {
        Debug.Log("Thoát trò chơi...");
        Application.Quit(); // Chỉ hoạt động trong bản build game
        // Nếu trong Editor, Debug.Log sẽ được hiển thị
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}