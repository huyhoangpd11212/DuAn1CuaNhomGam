// Tập tin: Assets/Scripts/PlayerBullet.cs
using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [Header("Cài đặt viên đạn")]
    [SerializeField] private float speed = 10f; // Tốc độ bay của viên đạn người chơi
    [SerializeField] private Vector2 direction = Vector2.up; // Hướng bay mặc định là lên trên (0,1)
    [SerializeField] private int damage = 1; // Sát thương gây ra bởi viên đạn này
    [SerializeField] private float lifeTime = 3f; // Thời gian tồn tại tối đa của viên đạn trước khi tự hủy

    void Start()
    {
        // Tự động hủy GameObject của viên đạn sau một khoảng thời gian nhất định (lifeTime).
        // Điều này giúp dọn dẹp cảnh trò chơi, tránh làm đầy bộ nhớ với các viên đạn đã bay ra ngoài màn hình.
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Di chuyển viên đạn theo hướng (direction) và tốc độ (speed) đã định.
        // Time.deltaTime đảm bảo tốc độ di chuyển độc lập với tốc độ khung hình (frame rate) của trò chơi.
        transform.Translate(direction * speed * Time.deltaTime);
    }

    // Hàm công khai này cho phép các script khác (ví dụ: PlayerController) thiết lập hướng bay của viên đạn
    // sau khi nó được tạo ra.
    public void SetDirection(Vector2 newDirection)
    {
        // Chuẩn hóa vector hướng. Điều này đảm bảo rằng độ dài của vector hướng luôn là 1,
        // giúp tốc độ của viên đạn nhất quán, không bị ảnh hưởng bởi độ lớn của vector newDirection truyền vào.
        direction = newDirection.normalized;
    }

    // Hàm tùy chọn: Cho phép các script khác (ví dụ: Enemy) lấy giá trị sát thương của viên đạn này.
    public int GetDamage()
    {
        return damage;
    }

    // Hàm này được gọi khi Collider 2D của viên đạn này va chạm (dưới dạng trigger) với một Collider 2D khác.
    // Để hàm này hoạt động:
    // 1. Cả hai GameObject phải có Collider 2D.
    // 2. Ít nhất một trong hai Collider 2D phải được đặt là "Is Trigger".
    // 3. Ít nhất một trong hai GameObject phải có Rigidbody2D.
    void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem đối tượng mà viên đạn va chạm có Tag là "Enemy" hay không.
        // (Bạn cần đảm bảo tất cả các Prefab/GameObject kẻ thù của bạn đều có Tag "Enemy" trong Unity Editor.)
        if (other.CompareTag("Enemy"))
        {
            // Cố gắng lấy script "Enemy" từ GameObject mà viên đạn va chạm vào.
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Nếu tìm thấy script Enemy, gọi hàm TakeDamage() của nó với giá trị sát thương của viên đạn.
                enemy.TakeDamage(damage);
            }
            Debug.Log("Viên đạn người chơi trúng kẻ thù!"); // Log ra console để kiểm tra

            // Hủy viên đạn ngay sau khi nó va chạm với kẻ thù để nó không bay xuyên qua.
            Destroy(gameObject);
        }
        // Tùy chọn: Bạn có thể thêm các điều kiện va chạm khác ở đây.
        // Ví dụ: Nếu viên đạn va chạm với một "Boundary" (biên giới màn hình)
        // thì cũng hủy nó để không bị tích tụ ngoài màn hình.
        // else if (other.CompareTag("Boundary"))
        // {
        //     Debug.Log("Viên đạn người chơi chạm biên giới.");
        //     Destroy(gameObject);
        // }
    }
}