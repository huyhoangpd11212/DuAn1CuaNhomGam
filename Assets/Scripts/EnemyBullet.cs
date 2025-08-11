// Tập tin: Assets/Scripts/EnemyBullet.cs
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float lifeTime = 3f; // Thời gian tồn tại của viên đạn
    private int damageAmount = 1; // Sát thương mặc định, sẽ được thiết lập bởi Enemy.cs

    void Start()
    {
        Destroy(gameObject, lifeTime); // Tự động hủy viên đạn sau một khoảng thời gian
    }

    void Update()
    {
        if (PlayerController.isGamePaused) return; // Dừng di chuyển khi trò chơi bị tạm dừng

        transform.Translate(Vector2.down * speed * Time.deltaTime); // Viên đạn kẻ thù bay xuống dưới
    }

    // Phương thức để Enemy.cs thiết lập sát thương cho viên đạn này
    public void SetDamage(int damage)
    {
        damageAmount = damage;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damageAmount); // Gây sát thương cho người chơi
            }
            Destroy(gameObject); // Hủy viên đạn sau khi va chạm
        }
        else if (other.CompareTag("PlayerBullet"))
        {
            // Viên đạn của kẻ thù bị viên đạn người chơi bắn trúng (tùy chọn: tự hủy hoặc không làm gì)
            // Trong nhiều trò chơi, viên đạn kẻ thù sẽ không bị hủy bởi viên đạn người chơi trừ khi có lá chắn
            // Nếu bạn muốn viên đạn kẻ thù tự hủy khi chạm viên đạn người chơi:
            // Destroy(gameObject);
            // Destroy(other.gameObject); // Hủy cả viên đạn người chơi nếu bạn muốn
        }
    }
}