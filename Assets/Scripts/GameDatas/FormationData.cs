// File: Assets/Scripts/FormationData.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewFormation", menuName = "Enemy/Formation Data")]
public class FormationData : ScriptableObject
{
    [Tooltip("Tên của đội hình (dễ nhìn trong Inspector)")]
    public string formationName = "Default Formation";

    [Tooltip("Danh sách các điểm sinh gà trong đội hình")]
    public List<EnemySpawnPoint> spawnPoints;

    [Tooltip("Tốc độ rơi xuống của đội hình này")]
    public float descentSpeed = 2.0f;

    [Tooltip("Vị trí Y mục tiêu mà đội hình này sẽ di chuyển đến và dừng lại")]
    public float targetY = 0.0f;

    // Phương thức này sẽ tạo ra GameObject đội hình thực tế
    public GameObject CreateFormationGameObject(Vector2 initialSpawnPosition)
    {
        GameObject formationParent = new GameObject(formationName + " (Instance)");
        formationParent.transform.position = initialSpawnPosition;

        FormationController controller = formationParent.AddComponent<FormationController>();

        // Dòng code gây lỗi đã được loại bỏ.
        // controller.SetDescentSpeed(descentSpeed);

        // Truyền thông tin tốc độ và vị trí mục tiêu vào FormationController
        controller.SetTarget(new Vector2(initialSpawnPosition.x, targetY), descentSpeed);

        foreach (EnemySpawnPoint sp in spawnPoints)
        {
            if (sp.enemyPrefab != null)
            {
                GameObject enemyGO = Instantiate(sp.enemyPrefab,
                                                 (Vector2)formationParent.transform.position + sp.offset,
                                                 Quaternion.identity,
                                                 formationParent.transform);
                enemyGO.transform.localPosition = sp.offset;
            }
        }
        return formationParent;
    }
}