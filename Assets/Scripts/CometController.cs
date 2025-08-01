using UnityEngine;

public class CometController : MonoBehaviour
{
    [Header("Cài đặt Sao Chổi")]
    [Tooltip("Tốc độ di chuyển của sao chổi")]
    [SerializeField] private float moveSpeed = 5f;
    [Tooltip("Sát thương mà sao chổi gây ra khi va chạm")]
    [SerializeField] private int damageAmount = 2;
    [Tooltip("Prefab của hiệu ứng nổ khi sao chổi chạm người chơi")]
    [SerializeField] private GameObject explosionEffectPrefab;

    private Vector2 moveDirection;
    private bool isExploded = false;

    // Phương thức được gọi bởi CometSpawner để thiết lập hướng bay
    public void Initialize(Vector2 direction)
    {
        moveDirection = direction.normalized;
    }

    void Update()
    {
        if (PlayerController.isGamePaused || isExploded)
        {
            return;
        }

        // Di chuyển sao chổi theo hướng đã định
        transform.position += (Vector3)moveDirection * moveSpeed * Time.deltaTime;

        // Tự hủy nếu sao chổi đi ra khỏi màn hình (với một khoảng an toàn)
        if (Mathf.Abs(transform.position.x) > 10f || Mathf.Abs(transform.position.y) > 10f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isExploded) return;

        // Nếu va chạm với người chơi
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damageAmount);
            }
            Explode();
        }
        // Có thể va chạm với đạn của người chơi nếu bạn muốn, nhưng yêu cầu là chạm người chơi
        // else if (other.CompareTag("PlayerBullet"))
        // {
        //     Explode();
        // }
    }

    private void Explode()
    {
        isExploded = true;

        // Kích hoạt hiệu ứng nổ nếu có
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // Ẩn sprite và collider để không gây thêm va chạm
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;

        // Hủy GameObject sao chổi sau một thời gian ngắn
        Destroy(gameObject, 0.5f);
    }
}