using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    [Header("Cài đặt chung")]
    [SerializeField] private int maxHealth = 15;
    [SerializeField] private int health = 15;
    [SerializeField] private int scoreValue = 2000;

    [Header("Cài đặt Di chuyển")]
    [SerializeField] private Vector2 roamingAreaCenter = new Vector2(0f, 0f);
    [SerializeField] private float roamingRadius = 3f;
    [SerializeField] private float horizontalMoveSpeed = 2f;

    [Header("Cài đặt Bắn đạn thường")]
    [SerializeField] private GameObject bossBulletPrefab;
    [SerializeField] private float bossBulletDamage = 3f;
    [SerializeField] private float fireRate = 2f;
    private float nextFireTime;

    [Header("Cài đặt Bắn đạn tỏa tròn")]
    [SerializeField] private GameObject circularBulletPrefab;
    [SerializeField] private int circularBulletsPerShot = 12;
    [SerializeField] private float circularBulletSpeed = 5f;
    [SerializeField] private float circularShotInterval = 6f;
    private float nextCircularShotTime;

    [Header("UI và Effects")]
    private BossHealthUI bossHealthUI;

    void Start()
    {
        maxHealth = health; // Đảm bảo maxHealth được set
        nextFireTime = Time.time + fireRate;
        nextCircularShotTime = Time.time + circularShotInterval;

        // Setup Boss Health UI
        bossHealthUI = FindObjectOfType<BossHealthUI>();
        if (bossHealthUI != null)
        {
            bossHealthUI.SetBoss(this);
        }

        // Phát âm thanh Boss xuất hiện
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayAlertSound();
        }

        Debug.Log("🐉 BOSS SPAWNED! Health: " + health + "/" + maxHealth);
    }

    void Update()
    {
        if (PlayerController.isGamePaused) return;

        HandleHorizontalMovement();

        // Bắn đạn thường
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }

        // Bắn đạn tỏa tròn sau mỗi khoảng thời gian
        if (Time.time >= nextCircularShotTime)
        {
            ShootCircular();
            nextCircularShotTime = Time.time + circularShotInterval;
        }
    }

    void HandleHorizontalMovement()
    {
        float newX = roamingAreaCenter.x + Mathf.Sin(Time.time * horizontalMoveSpeed) * roamingRadius;
        transform.position = new Vector2(newX, transform.position.y);
    }

    void Shoot()
    {
        if (bossBulletPrefab == null)
        {
            Debug.LogWarning("Boss Bullet Prefab (đạn thường) chưa được gán trên " + gameObject.name + "!");
            return;
        }

        int bulletsToShoot = Random.Range(1, 4);

        for (int i = 0; i < bulletsToShoot; i++)
        {
            float offset = (i - (bulletsToShoot - 1) / 2f) * 0.5f;
            Vector2 spawnPosition = (Vector2)transform.position + new Vector2(offset, -0.5f);

            GameObject newBullet = Instantiate(bossBulletPrefab, spawnPosition, Quaternion.identity);

            BossBullet bossBulletScript = newBullet.GetComponent<BossBullet>();
            if (bossBulletScript != null)
            {
                bossBulletScript.SetDamage(bossBulletDamage);
            }
        }

        // Phát âm thanh bắn
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayPlayerShootSound();
        }
    }

    private void ShootCircular()
    {
        if (circularBulletPrefab == null)
        {
            Debug.LogWarning("Circular Bullet Prefab (đạn tỏa tròn) chưa được gán vào BossController!");
            return;
        }

        float angleStep = 360f / circularBulletsPerShot;

        for (int i = 0; i < circularBulletsPerShot; i++)
        {
            float angle = i * angleStep;
            float angleInRadians = angle * Mathf.Deg2Rad;

            Vector2 bulletDirection = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

            GameObject newBullet = Instantiate(circularBulletPrefab, transform.position, Quaternion.identity);
            Rigidbody2D rb = newBullet.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.linearVelocity = bulletDirection.normalized * circularBulletSpeed;
            }

            // Set damage for circular bullets
            BossBullet bossBulletScript = newBullet.GetComponent<BossBullet>();
            if (bossBulletScript != null)
            {
                bossBulletScript.SetDamage(bossBulletDamage);
            }
        }

        Debug.Log("🌀 Boss fired circular shot!");
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        health = Mathf.Max(0, health);

        // Hiệu ứng flash khi nhận damage
        StartCoroutine(FlashEffect());

        Debug.Log($"🐉 Boss took {damageAmount} damage! Health: {health}/{maxHealth}");

        if (health <= 0)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator FlashEffect()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
    }

    void Die()
    {
        Debug.Log("🐉💀 BOSS ĐÃ BỊ TIÊU DIỆT! Bạn đã chiến thắng!");

        PlayerController.AddScore(scoreValue);

        // Ẩn Boss UI
        if (bossHealthUI != null)
        {
            bossHealthUI.HideBossUI();
        }

        // Phát âm thanh Boss chết
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayEnemyExplosionSound();
        }

        // Trigger Game Win
        if (GameStateManager.instance != null)
        {
            GameStateManager.instance.TriggerGameWin();
        }

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            PlayerBullet playerBullet = other.GetComponent<PlayerBullet>();
            if (playerBullet != null)
            {
                TakeDamage(playerBullet.GetDamage());
            }
            Destroy(other.gameObject);
        }
    }

    // Public getter methods cho UI
    public int GetCurrentHealth()
    {
        return health;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }
}