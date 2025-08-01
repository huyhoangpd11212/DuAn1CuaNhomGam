using UnityEngine;

public class BossBullet : MonoBehaviour
{
    [SerializeField] private float speed = 3f; // Tốc độ đạn
    [SerializeField] private float lifeTime = 5f; // Thời gian tồn tại của đạn
    private float damage; // Sát thương của đạn

    private Vector2 targetDirection;

    void Start()
    {
        // Viên đạn sẽ tự hủy sau 'lifeTime' giây, bất kể có va chạm hay không.
        Destroy(gameObject, lifeTime);

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            // Viên đạn sẽ bay theo hướng của người chơi khi được sinh ra
            targetDirection = (player.transform.position - transform.position).normalized;
        }
        else
        {
            // Nếu không tìm thấy người chơi, đạn bay thẳng xuống
            targetDirection = Vector2.down;
            Debug.LogWarning("Không tìm thấy Player với tag 'Player' để BossBullet nhắm tới.");
        }
    }

    void Update()
    {
        // Di chuyển đạn
        transform.Translate(targetDirection * speed * Time.deltaTime);
    }

    public void SetDamage(float dmg)
    {
        damage = dmg;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem viên đạn có va chạm với người chơi không
        if (other.CompareTag("Player"))
        {
            // Lấy tham chiếu đến script PlayerController
            PlayerController player = other.GetComponent<PlayerController>();

            if (player != null)
            {
                // GỌI PHƯƠNG THỨC TAKEDAMAGE ĐỂ GÂY SÁT THƯƠNG
                player.TakeDamage((int)damage);
            }

            // Hủy viên đạn sau khi va chạm với người chơi
            Destroy(gameObject);
        }
    }
}