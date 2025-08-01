// File: Assets/Scripts/Boss/BossController.cs
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BossController : MonoBehaviour
{
    [Header("Cài đặt chung")]
    [SerializeField] private int health = 15;
    [SerializeField] private int scoreValue = 100;

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
    [Tooltip("Prefab của đạn Boss")]
    [SerializeField] private GameObject circularBulletPrefab;
    [Tooltip("Số lượng đạn bắn ra mỗi lần")]
    [SerializeField] private int circularBulletsPerShot = 12;
    [Tooltip("Tốc độ bay của đạn")]
    [SerializeField] private float circularBulletSpeed = 5f;
    [Tooltip("Thời gian chờ giữa các lần bắn đạn tỏa tròn")]
    [SerializeField] private float circularShotInterval = 6f;
    private float nextCircularShotTime;

    void Start()
    {
        nextFireTime = Time.time + fireRate;
        nextCircularShotTime = Time.time + circularShotInterval;
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

    /// <summary>
    /// Xử lý hành vi di chuyển ngang qua lại của Boss.
    /// </summary>
    void HandleHorizontalMovement()
    {
        float newX = roamingAreaCenter.x + Mathf.Sin(Time.time * horizontalMoveSpeed) * roamingRadius;
        transform.position = new Vector2(newX, transform.position.y);
    }

    /// <summary>
    /// Logic bắn đạn thường của Boss.
    /// </summary>
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
            else
            {
                Debug.LogWarning("Prefab BossBullet không có script BossBullet.cs!");
            }
        }
    }

    /// <summary>
    /// Bắn ra nhiều viên đạn tỏa tròn xung quanh boss.
    /// </summary>
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
            else
            {
                Debug.LogWarning("Prefab đạn tỏa tròn của boss không có Rigidbody2D!");
            }
        }
    }

    /// <summary>
    /// Xử lý khi Boss nhận sát thương
    /// </summary>
    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        health = Mathf.Max(0, health);
        if (health <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Xử lý kết thúc game khi Boss bị tiêu diệt.
    /// </summary>
    void Die()
    {
        Debug.Log("BOSS ĐÃ BỊ TIÊU DIỆT! Bạn đã chiến thắng!");

        PlayerController.AddScore(scoreValue);

        Destroy(gameObject);
    }

    /// <summary>
    /// Xử lý va chạm với đạn của người chơi
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            PlayerBullet playerBullet = other.GetComponent<PlayerBullet>();
            if (playerBullet != null)
            {
                TakeDamage(playerBullet.GetDamage());
            }
        }
    }
}