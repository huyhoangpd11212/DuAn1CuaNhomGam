using UnityEngine;
using System.Collections.Generic;

public class CometSpawner : MonoBehaviour
{
    [Header("Cài đặt Spawner")]
    [Tooltip("Danh sách Prefab của các loại sao chổi")]
    [SerializeField] private List<GameObject> cometPrefabs;

    [Tooltip("Khoảng thời gian tối thiểu giữa các lần sinh sao chổi")]
    [SerializeField] private float minSpawnInterval = 3f; // Đã giảm từ 5f

    [Tooltip("Khoảng thời gian tối đa giữa các lần sinh sao chổi")]
    [SerializeField] private float maxSpawnInterval = 10f; // Đã giảm từ 15f

    [Tooltip("Khoảng cách từ mép màn hình để sinh sao chổi")]
    [SerializeField] private float spawnOffset = 1f;

    private float nextSpawnTime;

    void Start()
    {
        SetNextSpawnTime();
    }

    void Update()
    {
        if (PlayerController.isGamePaused) return;

        if (Time.time >= nextSpawnTime)
        {
            SpawnComet();
            SetNextSpawnTime();
        }
    }

    private void SetNextSpawnTime()
    {
        float spawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
        nextSpawnTime = Time.time + spawnInterval;
    }

    private void SpawnComet()
    {
        if (cometPrefabs == null || cometPrefabs.Count == 0)
        {
            Debug.LogWarning("Danh sách Comet Prefabs chưa được gán hoặc rỗng!");
            return;
        }

        int randomIndex = Random.Range(0, cometPrefabs.Count);
        GameObject selectedCometPrefab = cometPrefabs[randomIndex];

        Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

        Vector2 spawnPosition = Vector2.zero;
        Vector2 targetPosition = Vector2.zero;

        int side = Random.Range(0, 4); // 0: Top, 1: Bottom, 2: Left, 3: Right

        switch (side)
        {
            case 0: // Sinh từ trên xuống
                spawnPosition = new Vector2(Random.Range(-screenBounds.x, screenBounds.x), screenBounds.y + spawnOffset);
                targetPosition = new Vector2(Random.Range(-screenBounds.x, screenBounds.x), -screenBounds.y - spawnOffset);
                break;
            case 1: // Sinh từ dưới lên
                spawnPosition = new Vector2(Random.Range(-screenBounds.x, screenBounds.x), -screenBounds.y - spawnOffset);
                targetPosition = new Vector2(Random.Range(-screenBounds.x, screenBounds.x), screenBounds.y + spawnOffset);
                break;
            case 2: // Sinh từ trái sang
                spawnPosition = new Vector2(-screenBounds.x - spawnOffset, Random.Range(-screenBounds.y, screenBounds.y));
                targetPosition = new Vector2(screenBounds.x + spawnOffset, Random.Range(-screenBounds.y, screenBounds.y));
                break;
            case 3: // Sinh từ phải sang
                spawnPosition = new Vector2(screenBounds.x + spawnOffset, Random.Range(-screenBounds.y, screenBounds.y));
                targetPosition = new Vector2(-screenBounds.x - spawnOffset, Random.Range(-screenBounds.y, screenBounds.y));
                break;
        }

        Vector2 direction = (targetPosition - spawnPosition).normalized;

        GameObject newComet = Instantiate(selectedCometPrefab, spawnPosition, Quaternion.identity);

        CometController cometController = newComet.GetComponent<CometController>();
        if (cometController != null)
        {
            cometController.Initialize(direction);
        }
        else
        {
            Debug.LogWarning("Prefab " + selectedCometPrefab.name + " không có script CometController.cs!");
        }
    }
}