using UnityEngine;
using UnityEngine.UI;

// Nếu bạn sử dụng TextMeshPro, hãy thêm: using TMPro;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }
    public static bool isGamePaused = false;

    [Header("Di chuyển Người chơi")]
    [Tooltip("Tốc độ di chuyển của người chơi")]
    [SerializeField] private float moveSpeed = 5f;
    [Tooltip("Giới hạn di chuyển theo trục X của người chơi (nửa chiều rộng màn hình)")]
    [SerializeField] private float xBoundary = 4.5f;
    [Tooltip("Giới hạn di chuyển theo trục Y của người chơi (nửa chiều cao màn hình)")]
    [SerializeField] private float yBoundary = 4.5f;
    [Tooltip("Hệ số giảm tốc độ di chuyển khi động cơ bị hỏng (ví dụ: 0.5f cho 50% tốc độ)")]
    [SerializeField] private float damagedMoveSpeedMultiplier = 0.3f;

    [Header("Chiến đấu của Người chơi")]
    [Tooltip("Prefab của đạn người chơi")]
    [SerializeField] private GameObject playerBulletPrefab;
    [Tooltip("Điểm xuất phát của đạn (offset từ vị trí người chơi)")]
    [SerializeField] private Vector2 bulletSpawnOffset = new Vector2(0f, 0.5f);
    [Tooltip("Tần suất bắn đạn (giây giữa các lần bắn)")]
    [SerializeField] private float fireRate = 0.2f;
    private float nextFireTime;

    [Header("Multi-Shot Settings")]
    [Tooltip("Số lượng đạn bắn ra mỗi lần")]
    [SerializeField] private int bulletsPerShot = 1;
    [Tooltip("Khoảng cách giữa các viên đạn khi bắn nhiều")]
    [SerializeField] private float multiShotSpread = 0.2f;

    [Header("Máu Người chơi")]
    [Tooltip("Máu hiện tại của người chơi")]
    [SerializeField] private int currentHealth = 3;
    [Tooltip("Máu tối đa của người chơi")]
    [SerializeField] private int maxHealth = 3;
    [Tooltip("Tham chiếu đến script HealthUIController để hiển thị máu")]
    [SerializeField] private HealthUIController healthUIController;

    [Header("Hệ thống Quá nhiệt")]
    [Tooltip("Lượng nhiệt tăng lên mỗi khi bắn")]
    [SerializeField] private float heatPerShot = 10f;
    [Tooltip("Nhiệt năng tối đa trước khi quá nhiệt")]
    [SerializeField] private float maxHeat = 100f;
    [Tooltip("Thời gian cần để nhiệt năng giảm về 0 sau khi quá nhiệt")]
    [SerializeField] private float overheatCooldownTime = 6f;

    [Tooltip("Tham chiếu đến UI Slider để hiển thị nhiệt năng")]
    [SerializeField] private Slider heatSlider;

    private float currentHeat;
    private bool isOverheated = false;
    private float overheatEndTime;

    private Rigidbody2D rb;

    [Header("Điều khiển Hoạt ảnh")]
    [Tooltip("Animator của động cơ (kéo thả GameObject động cơ vào đây)")]
    [SerializeField] private Animator engineAnimator;
    [Tooltip("Animator của Body Player (sẽ đảm nhận animation nổ khi chết)")]
    [SerializeField] private Animator playerBodyAnimator;

    private bool engineAnimationStopped = false;
    private bool playerIsDead = false;

    [Header("Hệ thống Điểm")]
    [Tooltip("Tham chiếu đến UI Text để hiển thị điểm số")]
    [SerializeField] private Text scoreText;
    public static int currentScore = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Không tìm thấy Rigidbody2D trên người chơi! Vui lòng thêm thành phần Rigidbody2D.");
        }
    }

    void Start()
    {
        nextFireTime = Time.time;
        currentHeat = 0f;
        currentScore = 0;
        UpdateScoreUI();

        Time.timeScale = 1f;
        isGamePaused = false;

        if (healthUIController != null)
        {
            healthUIController.UpdateHealth(currentHealth);
        }
        else
        {
            Debug.LogWarning("Health UI Controller chưa được gán trong PlayerController!");
        }

        if (heatSlider != null)
        {
            heatSlider.maxValue = maxHeat;
            heatSlider.value = currentHeat;
        }
        else
        {
            Debug.LogWarning("Heat Slider chưa được gán trong PlayerController! Thanh nhiệt sẽ không hiển thị.");
        }

        if (engineAnimator == null)
        {
            Debug.LogWarning("Engine Animator chưa được gán trong PlayerController! Hoạt ảnh động cơ sẽ không được điều khiển.");
        }
        else
        {
            engineAnimator.SetBool("IsEngineRunning", true);
        }

        if (playerBodyAnimator == null)
        {
            Debug.LogWarning("Player Body Animator chưa được gán trong PlayerController! Hoạt ảnh nổ sẽ không phát.");
        }
    }

    void Update()
    {
        if (isGamePaused)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
            return;
        }

        if (!playerIsDead)
        {
            HandleMovement();
            HandleShooting();
            HandleHeatSystem();
        }
    }

    void HandleMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        float currentMoveSpeed = moveSpeed;

        if (engineAnimationStopped)
        {
            currentMoveSpeed *= damagedMoveSpeedMultiplier;
        }

        Vector2 moveDirection = new Vector2(horizontalInput, verticalInput).normalized;
        rb.linearVelocity = moveDirection * currentMoveSpeed;

        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -xBoundary, xBoundary);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, -yBoundary, yBoundary);
        transform.position = clampedPosition;
    }

    void HandleShooting()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime && !isOverheated)
        {
            if (currentHeat + heatPerShot > maxHeat)
            {
                SetOverheated(true);
                return;
            }

            Shoot();
            currentHeat += heatPerShot;
            UpdateHeatUI();
            nextFireTime = Time.time + fireRate;
        }
        else if (!isOverheated && currentHeat > 0)
        {
            currentHeat = Mathf.Max(0, currentHeat - Time.deltaTime * (maxHeat / overheatCooldownTime / 2));
            UpdateHeatUI();
        }
    }

    void Shoot()
    {
        if (playerBulletPrefab == null)
        {
            Debug.LogWarning("Player Bullet Prefab chưa được gán trong PlayerController!");
            return;
        }

        for (int i = 0; i < bulletsPerShot; i++)
        {
            float offset = 0;
            if (bulletsPerShot > 1)
            {
                offset = (i - (bulletsPerShot - 1) / 2f) * multiShotSpread;
            }

            Vector2 spawnPosition = (Vector2)transform.position + bulletSpawnOffset + new Vector2(offset, 0);
            Instantiate(playerBulletPrefab, spawnPosition, Quaternion.identity);
        }
    }

    void HandleHeatSystem()
    {
        if (isOverheated)
        {
            if (Time.time >= overheatEndTime)
            {
                SetOverheated(false);
                currentHeat = 0f;
                UpdateHeatUI();
                Debug.Log("Quá nhiệt đã kết thúc. Sẵn sàng bắn!");
            }
        }
    }

    void SetOverheated(bool state)
    {
        isOverheated = state;
        if (isOverheated)
        {
            overheatEndTime = Time.time + overheatCooldownTime;
            Debug.Log("QUÁ NHIỆT! Không thể bắn trong " + overheatCooldownTime + " giây.");
            currentHeat = maxHeat;
            UpdateHeatUI();
        }
    }

    void UpdateHeatUI()
    {
        if (heatSlider != null)
        {
            heatSlider.value = currentHeat;
        }
    }

    public static void AddScore(int amount)
    {
        Debug.Log("Thêm điểm: " + amount);
        currentScore += amount;
        PlayerController playerInstance = FindAnyObjectByType<PlayerController>();
        if (playerInstance != null)
        {
            playerInstance.UpdateScoreUI();
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore.ToString();
        }
        else
        {
            Debug.LogWarning("Score Text UI chưa được gán trong PlayerController! Điểm số sẽ không hiển thị.");
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (playerIsDead) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log("Người chơi nhận " + damageAmount + " sát thương. Máu hiện tại: " + currentHealth);

        // Cập nhật UI trái tim
        if (healthUIController != null)
        {
            healthUIController.UpdateHealth(currentHealth);
        }

        if (currentHealth == 1 && !engineAnimationStopped)
        {
            Debug.Log("Máu người chơi nguy cấp! Động cơ ngừng hoạt động và giảm tốc độ.");
            if (engineAnimator != null)
            {
                engineAnimator.SetBool("IsEngineRunning", false);
                engineAnimationStopped = true;
            }
        }
        else if (currentHealth > 1 && engineAnimationStopped)
        {
            Debug.Log("Động cơ đã sửa chữa một phần. Tốc độ trở lại bình thường.");
            if (engineAnimator != null)
            {
                engineAnimator.SetBool("IsEngineRunning", true);
                engineAnimationStopped = false;
            }
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (playerIsDead) return;
        playerIsDead = true;

        Debug.Log("Người chơi đã chết! Phát hoạt ảnh nổ.");

        if (rb != null) rb.bodyType = RigidbodyType2D.Static;
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        if (playerBodyAnimator != null)
        {
            playerBodyAnimator.SetTrigger("Explode");
        }

        float explosionDuration = 1.0f;
        Destroy(gameObject, explosionDuration);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (playerIsDead) return;

        // Xóa logic này để tránh nhận 2 sát thương, việc này sẽ do EnemyBullet.cs xử lý
        // if (other.CompareTag("EnemyBullet"))
        // {
        //     TakeDamage(1);
        //     Destroy(other.gameObject);
        // }

        if (other.CompareTag("Enemy"))
        {
            TakeDamage(1);
        }
        else if (other.CompareTag("Comet"))
        {
            TakeDamage(3);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Item"))
        {
            Debug.Log("Người chơi chạm vào Item. ItemPickup sẽ xử lý.");
        }
    }

    public void ApplyItemEffect(ItemType type, float value)
    {
        switch (type)
        {
            case ItemType.Heal:
                currentHealth = Mathf.Min(maxHealth, currentHealth + (int)value);
                UpdateHealthUI();
                Debug.Log("Hồi máu: " + (int)value + ". Máu hiện tại: " + currentHealth);
                if (currentHealth > 1 && engineAnimationStopped)
                {
                    engineAnimator.SetBool("IsEngineRunning", true);
                    engineAnimationStopped = false;
                }
                break;
            case ItemType.ExtraBullet:
                bulletsPerShot = Mathf.Min(bulletsPerShot + 1, 5);
                Debug.Log("Thêm 1 viên đạn. Tổng số đạn: " + bulletsPerShot);
                break;

            case ItemType.ReduceHeat:
                currentHeat = 0;
                UpdateHeatUI();
                Debug.Log("Nhiệt đã được đặt về 0.");
                if (isOverheated)
                {
                    SetOverheated(false);
                    Debug.Log("Thoát trạng thái quá nhiệt nhờ item!");
                }
                break;

            default:
                Debug.LogWarning("Loại item không được xử lý: " + type);
                break;
        }
    }

    // Phương thức cập nhật UI trái tim đã được đặt ở đây để thuận tiện
    void UpdateHealthUI()
    {
        if (healthUIController != null)
        {
            healthUIController.UpdateHealth(currentHealth);
        }
    }
}