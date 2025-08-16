using UnityEngine;
using System.Collections;

public class EnemySpawnerLevel2 : MonoBehaviour
{
    [Header("Level 2 Settings")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private float spawnRate = 1.5f; // Nhanh hơn Level 1
    [SerializeField] private int maxEnemies = 10;    // Nhiều enemies hơn
    [SerializeField] private float enemySpeedBonus = 1.3f; // Enemies nhanh hơn 30%

    private float nextSpawnTime;

    void Start()
    {
        nextSpawnTime = Time.time + 2f;
        Debug.Log("🌊 Level 2 bắt đầu - Khó hơn Level 1!");
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemyLevel2();
            nextSpawnTime = Time.time + spawnRate;

            // Game khó dần theo thời gian
            spawnRate = Mathf.Max(0.5f, spawnRate - 0.02f);
        }
    }

    void SpawnEnemyLevel2()
    {
        if (GameObject.FindGameObjectsWithTag("Enemy").Length >= maxEnemies)
            return;

        // Spawn ở vị trí random
        float randomX = Random.Range(-8f, 8f);
        Vector3 spawnPos = new Vector3(randomX, 6f, 0f);

        GameObject enemy = Instantiate(enemyPrefabs[0], spawnPos, Quaternion.identity);

        // Làm enemy nhanh hơn
        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.down * 3f * enemySpeedBonus; // Nhanh hơn Level 1
        }

        // Đổi màu để phân biệt
        SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.red; // Màu đỏ cho Level 2
        }

        Debug.Log("👾 Spawn enemy Level 2 - Tốc độ: " + (3f * enemySpeedBonus));
    }
}