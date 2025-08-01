// File: Assets/Scripts/FormationController.cs
using UnityEngine;
using System.Collections.Generic;

public class FormationController : MonoBehaviour
{
    private float descentSpeed;
    private Vector2 targetPosition;
    private bool hasTarget = false;

    void Update()
    {
        // Kiểm tra tạm dừng game
        if (PlayerController.isGamePaused) return;

        if (hasTarget)
        {
            // Di chuyển đội hình từ từ xuống targetPosition
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, descentSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, targetPosition) < 0.01f)
            {
                hasTarget = false;
                Debug.Log("Đội hình đã đến vị trí mục tiêu: " + targetPosition);
            }
        }
    }

    /// <summary>
    /// Phương thức để thiết lập cả vị trí mục tiêu và tốc độ di chuyển của đội hình.
    /// </summary>
    public void SetTarget(Vector2 pos, float speed)
    {
        targetPosition = pos;
        descentSpeed = speed;
        hasTarget = true;
    }

    /// <summary>
    /// Phương thức này không còn cần thiết cho logic spawn Boss mới.
    /// Kẻ địch sẽ tự gọi EnemySpawner.NotifyEnemyDefeated() khi chúng chết.
    /// </summary>
    public void EnemyInFormationDefeated()
    {
        // Bạn có thể giữ lại logic này nếu muốn GameObject cha của đội hình tự hủy
        // khi không còn kẻ địch nào, nhưng nó không liên quan đến việc spawn Boss.

        int enemyChildCount = 0;
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Enemy>() != null)
            {
                enemyChildCount++;
            }
        }

        if (enemyChildCount == 0)
        {
            Debug.Log("Tất cả kẻ địch trong đội hình đã bị tiêu diệt! GameObject đội hình sẽ bị hủy.");
            Destroy(gameObject);
        }
    }
}