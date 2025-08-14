using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int currentHealth;

    [Header("Level Settings")]
    [SerializeField] private int startingHealthForLevel = 0; // 0 = use level-specific health

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.5f;
    private float nextFireTime;

    [Header("UI References")]
    [SerializeField] private GameObject healthBarObject; // ✅ SỬA LỖI: Dùng GameObject thay vì HealthBar

    // Static variables
    public static int currentScore = 0;
    public static bool isGamePaused = false;

    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // ✅ Thiết lập máu theo level
        SetHealthForCurrentLevel();

        // ✅ SỬA LỖI: Tìm HealthBar component an toàn hơn
        if (healthBarObject == null)
        {
            // Tìm GameObject có script quản lý health UI
            GameObject healthUI = GameObject.FindWithTag("HealthUI");
            if (healthUI != null)
            {
                healthBarObject = healthUI;
            }
        }

        // Update UI
        UpdateHealthUI();

        Debug.Log($"🎮 Player started with {currentHealth}/{maxHealth} health");
    }

    // ✅ Method thiết lập máu theo level
    private void SetHealthForCurrentLevel()
    {
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        switch (currentSceneName.ToLower())
        {
            case "scene1":
            case "level1":
            case "menuscene":
                // Level 1: máu full
                currentHealth = maxHealth;
                Debug.Log("🎮 Level 1: Full health (" + maxHealth + ")");
                break;

            case "scene2":
            case "level2":
                // Level 2: máu giảm 1
                currentHealth = maxHealth - 1;
                currentHealth = Mathf.Max(1, currentHealth); // Đảm bảo ít nhất 1 máu
                Debug.Log("🎮 Level 2: Reduced health (" + currentHealth + "/" + maxHealth + ")");
                break;

            case "scene3":
            case "level3":
                // Level 3: máu giảm 2
                currentHealth = maxHealth - 2;
                currentHealth = Mathf.Max(1, currentHealth);
                Debug.Log("🎮 Level 3: More reduced health (" + currentHealth + "/" + maxHealth + ")");
                break;

            case "bosslevel":
            case "finallevel":
                // Boss level: máu giảm 1 nhưng có thêm bonus nếu từ level trước
                currentHealth = maxHealth - 1;
                if (currentScore >= 2000) // Bonus nếu điểm cao
                {
                    currentHealth = maxHealth;
                    Debug.Log("🎮 Boss Level: Bonus full health for high score!");
                }
                else
                {
                    Debug.Log("🎮 Boss Level: Reduced health (" + currentHealth + "/" + maxHealth + ")");
                }
                break;

            default:
                // Default: sử dụng startingHealthForLevel nếu được set
                if (startingHealthForLevel > 0)
                {
                    currentHealth = Mathf.Min(startingHealthForLevel, maxHealth);
                    Debug.Log("🎮 Custom level: Health set to " + currentHealth);
                }
                else
                {
                    currentHealth = maxHealth;
                    Debug.Log("🎮 Unknown level: Using full health");
                }
                break;
        }
    }

    void Update()
    {
        if (isGamePaused) return;

        HandleMovement();
        HandleShooting();
    }

    void HandleMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector2 movement = new Vector2(horizontalInput, verticalInput) * moveSpeed;
        rb.linearVelocity = movement;

        // Keep player within screen bounds
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -8f, 8f);
        pos.y = Mathf.Clamp(pos.y, -4f, 4f);
        transform.position = pos;
    }

    void HandleShooting()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            // Play shoot sound
            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlayPlayerShootSound();
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        // Flash effect
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashEffect());
        }

        // Update UI
        UpdateHealthUI();

        // Play hurt sound
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayPlayerExplosionSound();
        }

        Debug.Log($"🩸 Player took {damage} damage! Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator FlashEffect()
    {
        if (spriteRenderer == null) yield break;

        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    void Die()
    {
        Debug.Log("💀 Player died!");

        // Trigger Game Over
        if (GameStateManager.instance != null)
        {
            GameStateManager.instance.TriggerGameOver();
        }

        // Play death sound
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayPlayerExplosionSound();
        }

        // Destroy player (or deactivate)
        gameObject.SetActive(false);
    }

    // ✅ SỬA LỖI: Method update health UI an toàn
    private void UpdateHealthUI()
    {
        if (healthBarObject != null)
        {
            // Tìm script quản lý health UI trong healthBarObject
            MonoBehaviour[] scripts = healthBarObject.GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour script in scripts)
            {
                // Tìm method UpdateHealth
                var updateMethod = script.GetType().GetMethod("UpdateHealth");
                if (updateMethod != null)
                {
                    try
                    {
                        updateMethod.Invoke(script, new object[] { currentHealth, maxHealth });
                        break;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"Could not update health UI: {e.Message}");
                    }
                }
            }
        }
    }

    // Static method to add score
    public static void AddScore(int points)
    {
        currentScore += points;
        Debug.Log($"💰 Score added: +{points} | Total: {currentScore}");
    }

    // Getter methods
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public void Heal(int healAmount)
    {
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        UpdateHealthUI();
        Debug.Log($"💚 Player healed +{healAmount}! Health: {currentHealth}/{maxHealth}");
    }

}