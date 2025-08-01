// File: Assets/Scripts/EnemyBullet.cs
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float lifeTime = 3f; // Thời gian tồn tại của đạn
    private int damageAmount = 1; // Sát thương mặc định, sẽ được Enemy.cs gán

    void Start()
    {
        Destroy(gameObject, lifeTime); // Tự hủy sau một thời gian
    }

    void Update()
    {
        if (PlayerController.isGamePaused) return; // Dừng di chuyển khi game tạm dừng

        transform.Translate(Vector2.down * speed * Time.deltaTime); // Đạn kẻ địch bay xuống
    }

    // Phương thức để Enemy.cs gán sát thương cho viên đạn này
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
            // Đạn của kẻ địch bị đạn người chơi bắn trúng (tùy chọn: tự hủy hoặc không làm gì)
            // Trong nhiều game, đạn kẻ địch sẽ không bị hủy bởi đạn người chơi trừ khi có shield
            // Nếu bạn muốn đạn kẻ địch tự hủy khi chạm đạn người chơi:
            // Destroy(gameObject);
            // Destroy(other.gameObject); // Hủy cả đạn người chơi nếu bạn muốn
        }
    }
}