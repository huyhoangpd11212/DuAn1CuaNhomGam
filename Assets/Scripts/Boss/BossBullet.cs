using UnityEngine;

public class BossBullet : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private float lifeTime = 5f;
    private float damage = 1f;

    private Vector2 targetDirection;

    void Start()
    {
        Destroy(gameObject, lifeTime);

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            targetDirection = (player.transform.position - transform.position).normalized;
        }
        else
        {
            targetDirection = Vector2.down;
            Debug.LogWarning("Không tìm thấy Player với tag 'Player' để BossBullet nhắm tới.");
        }
    }

    void Update()
    {
        if (PlayerController.isGamePaused) return;

        transform.Translate(targetDirection * speed * Time.deltaTime);
    }

    public void SetDamage(float dmg)
    {
        damage = dmg;
    }

    public void SetDirection(Vector2 direction)
    {
        targetDirection = direction.normalized;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();

            if (player != null)
            {
                player.TakeDamage((int)damage);
            }

            // Phát âm thanh khi player bị trúng đạn Boss
            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlayPlayerExplosionSound();
            }

            Destroy(gameObject);
        }
        else if (other.CompareTag("PlayerBullet"))
        {
            // Boss bullet vs Player bullet - triệt tiêu nhau
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}