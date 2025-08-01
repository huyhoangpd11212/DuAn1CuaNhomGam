// File: Assets/Scripts/BossSpawner.cs
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BossSpawner : MonoBehaviour
{
    // ... (Các biến và Header)
    public GameObject bossPrefab;
    public Transform bossInitialSpawnPoint;
    public Text bossWarningText;
    public Animator bossWarningAnimator;
    public ScreenShake screenShake;

    // Thay đổi kiểu dữ liệu từ Enemy sang BossController
    private BossController currentBoss;

    // ... (Các phương thức Start và Awake giữ nguyên)

    public void SpawnBoss()
    {
        // ... (Logic cũ để hiển thị cảnh báo và hiệu ứng rung)

        GameObject bossObj = Instantiate(bossPrefab, bossInitialSpawnPoint.position, Quaternion.identity);

        // Lấy component BossController từ đối tượng vừa tạo
        currentBoss = bossObj.GetComponent<BossController>();

        if (currentBoss != null)
        {
            Debug.Log("Boss đã được spawn thành công!");
        }
        else
        {
            Debug.LogError("Prefab boss được spawn không có script BossController.cs!");
        }

        this.enabled = false;
    }

    // ... (Các phương thức khác)
}