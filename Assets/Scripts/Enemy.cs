using UnityEngine;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    [Header("Cài đặt chung")]
    [Tooltip("Tốc độ di chuyển của kẻ địch")]
    [SerializeField] protected float moveSpeed = 2f;
    [Tooltip("Máu hiện tại của kẻ địch")]
    [SerializeField] protected int health = 1;
    protected int initialHealth;

    [Tooltip("Điểm xuất phát của đạn (offset từ vị trí kẻ địch)")]
    [SerializeField] protected Vector2 bulletSpawnOffset = new Vector2(0f, -0.5f);

    [Header("Chiến đấu của kẻ địch")]
    [Tooltip("Prefab của đạn kẻ địch")]
    [SerializeField] protected GameObject enemyBulletPrefab;
    [Tooltip("Tần suất bắn đạn (giây giữa các lần bắn)")]
    [SerializeField] protected float fireRate = 2f;
    protected float nextFireTime;

    [Tooltip("Sát thương mà viên đạn của kẻ địch gây ra")]
    [SerializeField] protected int bulletDamage = 1;

    // ✅ THÊM: Score System
    [Header("Hệ thống điểm số")]
    [Tooltip("Điểm số được nhận khi tiêu diệt kẻ địch này")]
    [SerializeField] protected int scoreValue = 10;
    [Tooltip("Điểm bonus nếu tiêu diệt bằng một phát")]
    [SerializeField] protected int oneHitKillBonus = 5;

    [Header("Cơ chế rơi Item")]
    [Tooltip("Tỉ lệ phần trăm để kẻ địch rơi item khi bị tiêu diệt")]
    [SerializeField] private float itemDropChance = 20f; // 20% tỉ lệ rơi item
    [Tooltip("Prefab của Item loại 1")]
    [SerializeField] private GameObject item1Prefab;
    [Tooltip("Prefab của Item loại 2")]
    [SerializeField] private GameObject item2Prefab;
    [Tooltip("Prefab của Item loại 3")]
    [SerializeField] private GameObject item3Prefab;

    protected Rigidbody2D rb;
    protected Vector2 targetPosition;
    protected bool hasTarget = false;

    // ✅ THÊM: Score tracking
    private bool wasOneHitKill = true; // Track if killed in one hit

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Không tìm thấy Rigidbody2D trên Enemy!");
        }
    }

    protected virtual void Start()
    {
        initialHealth = health;
        nextFireTime = Time.time + Random.Range(0f, fireRate);
    }

    protected virtual void Update()
    {
        if (PlayerController.isGamePaused) return;

        if (hasTarget)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, targetPosition) < 0.01f)
            {
                hasTarget = false;
            }
        }

        HandleShooting();
    }

    public void SetTargetPosition(Vector2 pos)
    {
        targetPosition = pos;
        hasTarget = true;
    }

    protected virtual void HandleShooting()
    {
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    protected virtual void Shoot()
    {
        if (enemyBulletPrefab == null)
        {
            Debug.LogWarning("Enemy Bullet Prefab chưa được gán trên " + gameObject.name + "!");
            return;
        }

        Vector2 spawnPosition = (Vector2)transform.position + bulletSpawnOffset;
        GameObject newBullet = Instantiate(enemyBulletPrefab, spawnPosition, Quaternion.identity);

        EnemyBullet bulletScript = newBullet.GetComponent<EnemyBullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDamage(bulletDamage);
        }
        else
        {
            Debug.LogWarning("EnemyBullet prefab không có script EnemyBullet.cs!");
        }
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        health = Mathf.Max(0, health);

        Debug.Log(gameObject.name + " nhận " + damageAmount + " sát thương. Máu hiện tại: " + health);

        // ✅ THÊM: Track if not one-hit kill
        if (health < initialHealth && health > 0)
        {
            wasOneHitKill = false;
        }

        if (health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log(gameObject.name + " đã chết!");

        // ✅ THÊM: Add score khi enemy chết
        AddScoreForKill();

        // Phát âm thanh enemy nổ
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayEnemyExplosionSound();
        }

        // Gọi phương thức mới trong EnemySpawner để đếm kẻ địch
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.NotifyEnemyDefeated();
        }

        // Cơ chế rơi item
        DropItem();

        Destroy(gameObject);
    }

    // ✅ THÊM: Method để add score
    protected virtual void AddScoreForKill()
    {
        int totalScore = scoreValue;

        // Add one-hit kill bonus
        if (wasOneHitKill && oneHitKillBonus > 0)
        {
            totalScore += oneHitKillBonus;
            Debug.Log("💥 One-hit kill bonus: +" + oneHitKillBonus + " điểm!");
        }

        // Add score through GameManager for proper UI update
        if (GameManager.instance != null)
        {
            GameManager.instance.AddScore(totalScore);
            Debug.Log("💰 " + gameObject.name + " killed! +" + totalScore + " điểm. Tổng: " + PlayerController.currentScore);
        }
        else
        {
            // Fallback: Add directly to PlayerController
            PlayerController.AddScore(totalScore);
            Debug.Log("💰 " + gameObject.name + " killed! +" + totalScore + " điểm. Tổng: " + PlayerController.currentScore);
        }
    }

    // ✅ THÊM: Public methods để config score
    public void SetScoreValue(int newScoreValue)
    {
        scoreValue = newScoreValue;
        Debug.Log("📊 " + gameObject.name + " score value set to: " + scoreValue);
    }

    public void SetOneHitKillBonus(int bonusValue)
    {
        oneHitKillBonus = bonusValue;
        Debug.Log("🎯 " + gameObject.name + " one-hit bonus set to: " + oneHitKillBonus);
    }

    public int GetScoreValue()
    {
        return scoreValue;
    }

    public int GetTotalPossibleScore()
    {
        return scoreValue + oneHitKillBonus;
    }

    /// <summary>
    /// Xử lý việc rơi item khi kẻ địch bị tiêu diệt.
    /// </summary>
    private void DropItem()
    {
        // Kiểm tra tỉ lệ rơi item
        float randomValue = Random.Range(0f, 100f);
        if (randomValue <= itemDropChance)
        {
            GameObject itemToDrop = null;
            int itemIndex = Random.Range(1, 4); // Chọn ngẫu nhiên từ 1 đến 3

            switch (itemIndex)
            {
                case 1:
                    itemToDrop = item1Prefab;
                    break;
                case 2:
                    itemToDrop = item2Prefab;
                    break;
                case 3:
                    itemToDrop = item3Prefab;
                    break;
            }

            if (itemToDrop != null)
            {
                Instantiate(itemToDrop, transform.position, Quaternion.identity);
                Debug.Log("Item " + itemToDrop.name + " đã được rơi ra!");
            }
            else
            {
                Debug.LogWarning("Prefab item bị thiếu! Không thể rơi item.");
            }
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            PlayerBullet playerBullet = other.GetComponent<PlayerBullet>();
            if (playerBullet != null)
            {
                TakeDamage(playerBullet.GetDamage());
            }
        }
        else if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(1);
            }
            Die(); // ✅ Vẫn cho điểm khi enemy chạm player (tự sát)
        }
    }
}