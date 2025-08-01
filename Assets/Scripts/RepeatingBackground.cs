// File: Assets/Scripts/RepeatingBackground.cs
using UnityEngine;

public class RepeatingBackground : MonoBehaviour
{
    [Tooltip("Tốc độ cuộn của background")]
    [SerializeField] private float scrollSpeed = 0.5f; // Điều chỉnh tốc độ

    // Biến để lưu trữ chiều cao của sprite background
    private float spriteHeight;

    void Start()
    {
        // Lấy kích thước chiều cao của sprite (đơn vị Unity)
        // Đảm bảo script này được gắn vào mỗi "tile" background của bạn
        spriteHeight = GetComponent<SpriteRenderer>().bounds.size.y;
    }

    void Update()
    {
        
        if (PlayerController.isGamePaused) return;

        // Di chuyển background xuống dưới
        transform.Translate(Vector2.down * scrollSpeed * Time.deltaTime);

        // Nếu background đã di chuyển xuống dưới màn hình (ngoài tầm nhìn)
        // Chúng ta đưa nó lên trên cùng để tạo hiệu ứng lặp lại
        if (transform.position.y < -spriteHeight)
        {
            // Di chuyển nó lên trên, ngay phía trên tile khác
            // (Thường là vị trí hiện tại + 2 lần chiều cao của sprite để khớp với tile tiếp theo
            transform.position = new Vector2(transform.position.x, transform.position.y + 2 * spriteHeight);
        }
    }
}