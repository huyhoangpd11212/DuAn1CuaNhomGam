// Tập tin: Assets/Scripts/EnemySpawnPoint.cs
using UnityEngine;
using System; // Cần thiết cho [Serializable]

[Serializable]
public struct EnemySpawnPoint
{
    [Tooltip("Prefab của kẻ thù sẽ được tạo ra tại điểm này")]
    public GameObject enemyPrefab;
    [Tooltip("Vị trí tương đối so với gốc của đội hình")]
    public Vector2 offset;
}