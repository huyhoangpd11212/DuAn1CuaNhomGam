// Tập tin: Assets/Scripts/EnemySpawner.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class WaveConfig
{
    public string waveName = "Đợt mới";
    public List<FormationData> formationsInWave;
}

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [Header("Cấu hình tất cả các Đợt")]
    [Tooltip("Danh sách các cấu hình đợt theo thứ tự")]
    [SerializeField] private List<WaveConfig> allWaves;

    [Header("Cài đặt Spawn Chung")]
    [Tooltip("Vị trí Y mà các đội hình xuất hiện ban đầu (từ trên màn hình)")]
    [SerializeField] private float spawnYPosition = 5.0f;

    [Header("Cài đặt Đợt")]
    [Tooltip("Thời gian chờ giữa các lần spawn đội hình trong cùng một đợt")]
    [SerializeField] private float formationSpawnInterval = 3.0f;

    [Header("Cài đặt Boss")]
    [Tooltip("Thời gian chờ trước khi Boss xuất hiện")]
    [SerializeField] private float bossSpawnDelay = 3.0f;
    [Tooltip("Tham chiếu đến BossSpawner trong scene")]
    [SerializeField] private BossSpawner bossSpawnerRef;

    [Header("Kích hoạt Boss theo số kẻ địch")]
    [Tooltip("Số kẻ địch cần tiêu diệt để kích hoạt Boss")]
    [SerializeField] private int enemyCountToSpawnBoss = 9;
    private int enemiesDefeated = 0;
    private bool bossSequenceStarted = false;

    private int currentWaveIndex = -1;
    private Coroutine spawnCoroutine;

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
    }

    void Start()
    {
        if (bossSpawnerRef == null)
        {
            bossSpawnerRef = FindAnyObjectByType<BossSpawner>();
            if (bossSpawnerRef == null)
            {
                Debug.LogError("BossSpawner GameObject chưa được gán hoặc không tìm thấy trong scene. Boss sẽ không thể spawn!");
            }
        }
        StartNextWave();
    }

    public void NotifyEnemyDefeated()
    {
        if (!bossSequenceStarted)
        {
            enemiesDefeated++;
            Debug.Log("Kẻ địch bị tiêu diệt. Tổng cộng: " + enemiesDefeated);

            if (enemiesDefeated >= enemyCountToSpawnBoss)
            {
                Debug.Log("Đã đạt đủ số kẻ địch tiêu diệt. Chuẩn bị spawn Boss!");

                // Dừng Coroutine spawn kẻ địch thường để không spawn thêm
                if (spawnCoroutine != null)
                {
                    StopCoroutine(spawnCoroutine);
                }

                // Bắt đầu Coroutine spawn Boss một cách an toàn
                StartCoroutine(StartBossSequence());
                bossSequenceStarted = true;
            }
        }
    }

    void StartNextWave()
    {
        currentWaveIndex++;

        if (currentWaveIndex < allWaves.Count)
        {
            WaveConfig currentWaveConfig = allWaves[currentWaveIndex];
            Debug.Log("Bắt đầu Đợt " + (currentWaveIndex + 1) + ": " + currentWaveConfig.waveName + "!");

            if (currentWaveConfig.formationsInWave.Count > 0)
            {
                // Lưu tham chiếu của Coroutine spawn để có thể dừng nó sau này
                spawnCoroutine = StartCoroutine(SpawnFormations(currentWaveConfig.formationsInWave));
            }
            else
            {
                Debug.LogWarning("Không có cấu hình đội hình nào cho đợt " + (currentWaveIndex + 1) + "! Chuyển sang đợt tiếp theo.");
                StartNextWave();
            }
        }
        else
        {
            Debug.Log("Đã hoàn thành tất cả các đợt đội hình. Tiếp tục spawn ngẫu nhiên hoặc chờ Boss được kích hoạt.");
        }
    }

    IEnumerator SpawnFormations(List<FormationData> formationDatas)
    {
        foreach (FormationData formationData in formationDatas)
        {
            while (PlayerController.isGamePaused)
            {
                yield return null;
            }

            float spawnX = 0f;
            Vector2 spawnPosition = new Vector2(spawnX, spawnYPosition);
            formationData.CreateFormationGameObject(spawnPosition);

            yield return new WaitForSeconds(formationSpawnInterval);
        }
        Debug.Log("Đã hoàn thành việc spawn tất cả các đội hình cho đợt " + (currentWaveIndex + 1) + ".");
        StartNextWave();
    }

    IEnumerator StartBossSequence()
    {
        Debug.Log("Đang chờ để kích hoạt BossSpawner...");
        yield return new WaitForSeconds(bossSpawnDelay);

        while (PlayerController.isGamePaused)
        {
            yield return null;
        }

        if (bossSpawnerRef != null)
        {
            Debug.Log("Gọi SpawnBoss().");
            bossSpawnerRef.SpawnBoss();
            Debug.Log("Đã kích hoạt BossSpawner!");
        }
        else
        {
            Debug.LogError("Không tìm thấy BossSpawner để kích hoạt!");
        }
    }
}