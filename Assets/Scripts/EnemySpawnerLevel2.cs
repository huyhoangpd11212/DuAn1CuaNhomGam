using UnityEngine;
using System.Collections;

public class EnemySpawnerLevel2 : MonoBehaviour
{
    [Header("Level 2 Enemy Settings")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private Transform[] level2SpawnPoints;
    [SerializeField] private float spawnRate = 2f;
    [SerializeField] private float spawnRateIncrease = 0.1f;
    [SerializeField] private int maxEnemiesOnScreen = 8;

    [Header("Level 2 Spawn Pattern")]
    [SerializeField] private bool useAlternatingPattern = true;
    [SerializeField] private bool useRandomMultipleSpawns = true;
    [SerializeField] private float multiSpawnChance = 0.3f;

    [Header("Level 2 Wave System")]
    [SerializeField] private bool enableWaveSystem = true;
    [SerializeField] private int enemiesPerWave = 5;
    [SerializeField] private float timeBetweenWaves = 10f;

    [Header("Enemy Modifications")]
    [SerializeField] private float level2SpeedMultiplier = 1.2f;
    [SerializeField] private float level2HealthMultiplier = 1.1f;

    private float nextSpawnTime;
    private int currentSpawnPointIndex = 0;
    private int enemiesSpawned = 0;
    private int currentWave = 1;
    private float nextWaveTime;
    private float gameStartTime;

    void Start()
    {
        gameStartTime = Time.time;
        nextSpawnTime = Time.time + 3f; // Delay 3s trước khi spawn
        nextWaveTime = Time.time + timeBetweenWaves;

        // Tạo spawn points nếu chưa có
        if (level2SpawnPoints == null || level2SpawnPoints.Length == 0)
        {
            CreateLevel2SpawnPoints();
        }

        Debug.Log("🌊 Level 2 Enemy Spawner activated!");
        Debug.Log($"⚙️ Wave system: {enableWaveSystem}, Max enemies: {maxEnemiesOnScreen}");
    }

    void Update()
    {
        if (PlayerController.isGamePaused) return;

        // Đếm enemies hiện tại
        int currentEnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;

        // Spawn logic
        if (Time.time >= nextSpawnTime && currentEnemyCount < maxEnemiesOnScreen)
        {
            if (enableWaveSystem)
            {
                SpawnWave();
            }
            else
            {
                SpawnEnemyLevel2();
            }

            // Tăng spawn rate theo thời gian (game khó dần)
            spawnRate = Mathf.Max(0.5f, spawnRate - spawnRateIncrease);
            nextSpawnTime = Time.time + spawnRate;
        }
    }

    private void CreateLevel2SpawnPoints()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float screenWidth = cam.orthographicSize * cam.aspect;
        float spawnY = cam.transform.position.y + cam.orthographicSize + 1f;

        level2SpawnPoints = new Transform[6];

        for (int i = 0; i < 6; i++)
        {
            GameObject spawnPoint = new GameObject("Level2SpawnPoint_" + i);
            spawnPoint.transform.SetParent(this.transform);

            // Vị trí spawn đa dạng hơn level 1
            float xPos = 0;
            switch (i)
            {
                case 0: xPos = -screenWidth + 1f; break;          // Trái xa
                case 1: xPos = -screenWidth * 0.3f; break;        // Trái gần
                case 2: xPos = 0f; break;                         // Giữa
                case 3: xPos = screenWidth * 0.3f; break;         // Phải gần
                case 4: xPos = screenWidth - 1f; break;           // Phải xa
                case 5: xPos = Random.Range(-screenWidth, screenWidth); break; // Random
            }

            spawnPoint.transform.position = new Vector3(xPos, spawnY, 0);
            level2SpawnPoints[i] = spawnPoint.transform;
        }

        Debug.Log("✅ Created 6 Level 2 spawn points!");
    }

    private void SpawnEnemyLevel2()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("No enemy prefabs assigned for Level 2!");
            return;
        }

        // Multiple spawn chance
        int enemiesToSpawn = 1;
        if (useRandomMultipleSpawns && Random.value < multiSpawnChance)
        {
            enemiesToSpawn = Random.Range(2, 4); // Spawn 2-3 enemies
            Debug.Log($"🎯 Multiple spawn triggered! Spawning {enemiesToSpawn} enemies");
        }

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            // Chọn enemy prefab ngẫu nhiên
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            // Chọn spawn point
            Transform spawnPoint;
            if (useAlternatingPattern)
            {
                spawnPoint = level2SpawnPoints[currentSpawnPointIndex];
                currentSpawnPointIndex = (currentSpawnPointIndex + 1) % level2SpawnPoints.Length;
            }
            else
            {
                spawnPoint = level2SpawnPoints[Random.Range(0, level2SpawnPoints.Length)];
            }

            // Spawn enemy với slight random offset
            Vector3 spawnPos = spawnPoint.position + new Vector3(Random.Range(-0.5f, 0.5f), 0, 0);
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            // Level 2 enemy modifications
            ModifyEnemyForLevel2(enemy);

            enemiesSpawned++;
        }
    }

    private void ModifyEnemyForLevel2(GameObject enemy)
    {
        // ✅ SỬA LỖI: Kiểm tra enemy có EnemyController không trước khi dùng
        MonoBehaviour[] scripts = enemy.GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour script in scripts)
        {
            // Tìm script có method SetSpeed (có thể là EnemyController hoặc tên khác)
            if (script.GetType().GetMethod("SetSpeed") != null)
            {
                try
                {
                    // Tăng speed bằng reflection (an toàn hơn)
                    var getSpeedMethod = script.GetType().GetMethod("GetSpeed");
                    var setSpeedMethod = script.GetType().GetMethod("SetSpeed");

                    if (getSpeedMethod != null && setSpeedMethod != null)
                    {
                        float currentSpeed = (float)getSpeedMethod.Invoke(script, null);
                        setSpeedMethod.Invoke(script, new object[] { currentSpeed * level2SpeedMultiplier });
                        Debug.Log($"✅ Modified enemy speed: {currentSpeed} → {currentSpeed * level2SpeedMultiplier}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Could not modify enemy speed: {e.Message}");
                }
                break;
            }
        }

        // Đổi màu enemy để phân biệt với level 1
        SpriteRenderer spriteRenderer = enemy.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Tint màu đỏ nhẹ cho level 2
            spriteRenderer.color = new Color(1f, 0.8f, 0.8f, 1f);
        }
    }

    private void SpawnWave()
    {
        if (Time.time >= nextWaveTime)
        {
            Debug.Log($"🌊 Wave {currentWave} spawning! ({enemiesPerWave} enemies)");

            // Spawn wave với delay giữa các enemies
            StartCoroutine(SpawnWaveCoroutine());

            currentWave++;
            nextWaveTime = Time.time + timeBetweenWaves;

            // Tăng độ khó theo wave
            enemiesPerWave = Mathf.Min(10, enemiesPerWave + 1); // Max 10 enemies per wave
            timeBetweenWaves = Mathf.Max(5f, timeBetweenWaves - 0.5f); // Min 5s between waves

            // Increase enemy modifications
            level2SpeedMultiplier += 0.05f;
            multiSpawnChance = Mathf.Min(0.7f, multiSpawnChance + 0.05f); // Max 70% multi spawn chance
        }
    }

    private System.Collections.IEnumerator SpawnWaveCoroutine()
    {
        for (int i = 0; i < enemiesPerWave; i++)
        {
            if (GameObject.FindGameObjectsWithTag("Enemy").Length < maxEnemiesOnScreen)
            {
                SpawnEnemyLevel2();
            }

            // Wait between spawns (faster spawns in later waves)
            float waitTime = Mathf.Max(0.2f, 0.5f - (currentWave * 0.02f));
            yield return new WaitForSeconds(waitTime);
        }
    }

    // Public methods để control từ bên ngoài
    public void SetSpawnRate(float newRate)
    {
        spawnRate = newRate;
        Debug.Log($"⚙️ Spawn rate changed to: {spawnRate}");
    }

    public void SetMaxEnemies(int maxEnemies)
    {
        maxEnemiesOnScreen = maxEnemies;
        Debug.Log($"⚙️ Max enemies changed to: {maxEnemiesOnScreen}");
    }

    public int GetCurrentWave()
    {
        return currentWave;
    }

    public int GetEnemiesSpawned()
    {
        return enemiesSpawned;
    }

    public float GetGameTime()
    {
        return Time.time - gameStartTime;
    }

    // Force spawn wave method for testing
    public void ForceSpawnWave()
    {
        nextWaveTime = Time.time;
    }
}