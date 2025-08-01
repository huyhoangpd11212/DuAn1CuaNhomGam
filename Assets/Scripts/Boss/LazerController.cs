using UnityEngine;
using System.Collections.Generic;

public class CircularShotController : MonoBehaviour
{
    [Header("Cài đặt đạn tròn")]
    [Tooltip("Prefab của đạn Boss")]
    [SerializeField] private GameObject bossBulletPrefab;

    [Tooltip("Sát thương mỗi viên đạn gây ra")]
    [SerializeField] private int damagePerBullet = 1;

    [Tooltip("Số lượng đạn bắn ra mỗi lần")]
    [SerializeField] private int bulletsPerShot = 12;

    [Tooltip("Tốc độ bay của đạn")]
    [SerializeField] private float bulletSpeed = 5f;

    [Tooltip("Thời gian tồn tại của đạn trước khi tự hủy")]
    [SerializeField] private float bulletLifeTime = 5f;

    [Tooltip("Tần suất bắn (giây giữa các lần bắn)")]
    [SerializeField] private float fireRate = 6.0f;
    private float nextFireTime;

    // Danh sách để lưu trữ thông tin của các viên đạn đang hoạt động
    private struct BulletInfo
    {
        public GameObject bulletObject;
        public Vector2 direction;
        public float spawnTime;
    }

    private List<BulletInfo> activeBullets = new List<BulletInfo>();
    private PlayerController playerController;

    void Start()
    {
        nextFireTime = Time.time + fireRate;
        // Tìm PlayerController một lần để tái sử dụng
        playerController = FindAnyObjectByType<PlayerController>();
    }

    void Update()
    {
        if (PlayerController.isGamePaused) return;

        // Xử lý logic bắn đạn mới
        if (Time.time >= nextFireTime)
        {
            ShootCircular();
            nextFireTime = Time.time + fireRate;
        }

        // Xử lý logic di chuyển, va chạm và hủy đạn cũ
        UpdateBullets();
    }

    /// <summary>
    /// Bắn ra nhiều viên đạn tỏa tròn xung quanh boss.
    /// </summary>
    private void ShootCircular()
    {
        if (bossBulletPrefab == null)
        {
            Debug.LogWarning("Boss Bullet Prefab chưa được gán!");
            return;
        }

        float angleStep = 360f / bulletsPerShot;

        for (int i = 0; i < bulletsPerShot; i++)
        {
            float angle = i * angleStep;
            float angleInRadians = angle * Mathf.Deg2Rad;
            Vector2 bulletDirection = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

            GameObject newBulletGO = Instantiate(bossBulletPrefab, transform.position, Quaternion.identity);

            // Lưu thông tin của viên đạn vào danh sách
            BulletInfo newBulletInfo = new BulletInfo
            {
                bulletObject = newBulletGO,
                direction = bulletDirection.normalized,
                spawnTime = Time.time
            };
            activeBullets.Add(newBulletInfo);
        }
    }

    /// <summary>
    /// Cập nhật vị trí, kiểm tra va chạm và hủy đạn.
    /// </summary>
    private void UpdateBullets()
    {
        // Vòng lặp ngược để xóa các viên đạn khỏi danh sách một cách an toàn
        for (int i = activeBullets.Count - 1; i >= 0; i--)
        {
            BulletInfo bullet = activeBullets[i];

            // Di chuyển viên đạn
            if (bullet.bulletObject != null)
            {
                bullet.bulletObject.transform.Translate(bullet.direction * bulletSpeed * Time.deltaTime);

                // Kiểm tra va chạm với người chơi
                if (CheckCollisionWithPlayer(bullet.bulletObject))
                {
                    if (playerController != null)
                    {
                        playerController.TakeDamage(damagePerBullet);
                    }
                    Destroy(bullet.bulletObject);
                    activeBullets.RemoveAt(i);
                    continue; // Chuyển sang viên đạn tiếp theo
                }
            }

            // Kiểm tra thời gian tồn tại
            if (Time.time - bullet.spawnTime >= bulletLifeTime)
            {
                if (bullet.bulletObject != null)
                {
                    Destroy(bullet.bulletObject);
                }
                activeBullets.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Phương thức kiểm tra va chạm của viên đạn với người chơi
    /// </summary>
    private bool CheckCollisionWithPlayer(GameObject bulletObject)
    {
        if (playerController == null) return false;

        // Sử dụng khoảng cách đơn giản để kiểm tra va chạm
        float distance = Vector2.Distance(bulletObject.transform.position, playerController.transform.position);

        // Thay đổi giá trị 0.5f tùy thuộc vào kích thước của đạn và người chơi
        return distance < 0.5f;
    }
}