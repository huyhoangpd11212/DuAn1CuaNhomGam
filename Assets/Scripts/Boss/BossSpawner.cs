using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BossSpawner : MonoBehaviour
{
    [Header("Boss Settings")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform bossInitialSpawnPoint;

    [Header("Warning System")]
    [SerializeField] private Text bossWarningText;
    [SerializeField] private Animator bossWarningAnimator;
    [SerializeField] private float warningDuration = 3f;

    [Header("Effects")]
    [SerializeField] private ScreenShake screenShake;

    [Header("Spawn Conditions")]
    [SerializeField] private bool spawnByScore = true;
    [SerializeField] private int scoreThreshold = 1500;
    [SerializeField] private bool spawnByTime = false;
    [SerializeField] private float timeThreshold = 120f;

    private BossController currentBoss;
    private bool bossSpawned = false;
    private float gameStartTime;

    void Start()
    {
        gameStartTime = Time.time;
    }

    void Update()
    {
        if (bossSpawned || PlayerController.isGamePaused) return;

        bool shouldSpawnBoss = false;

        // Kiểm tra điều kiện spawn boss
        if (spawnByScore && PlayerController.currentScore >= scoreThreshold)
        {
            shouldSpawnBoss = true;
            Debug.Log($"🐉 Boss spawn condition met - Score: {PlayerController.currentScore}/{scoreThreshold}");
        }

        if (spawnByTime && (Time.time - gameStartTime) >= timeThreshold)
        {
            shouldSpawnBoss = true;
            Debug.Log($"🐉 Boss spawn condition met - Time: {Time.time - gameStartTime:F1}s/{timeThreshold}s");
        }

        if (shouldSpawnBoss)
        {
            StartCoroutine(SpawnBossWithWarning());
        }
    }

    private System.Collections.IEnumerator SpawnBossWithWarning()
    {
        bossSpawned = true;

        // Hiển thị cảnh báo
        if (bossWarningText != null)
        {
            bossWarningText.gameObject.SetActive(true);
            bossWarningText.text = "⚠️ WARNING: BOSS APPROACHING! ⚠️";
        }

        if (bossWarningAnimator != null)
        {
            bossWarningAnimator.SetTrigger("ShowWarning");
        }

        // Screen shake
        if (screenShake != null)
        {
            screenShake.TriggerShake();
        }

        // Phát âm thanh cảnh báo
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayAlertSound();
        }

        Debug.Log("⚠️ WARNING: BOSS APPROACHING!");

        // Đợi warning duration
        yield return new WaitForSeconds(warningDuration);

        // Ẩn warning
        if (bossWarningText != null)
        {
            bossWarningText.gameObject.SetActive(false);
        }

        // Spawn boss
        SpawnBoss();
    }

    public void SpawnBoss()
    {
        if (bossPrefab == null)
        {
            Debug.LogError("Boss Prefab is not assigned!");
            return;
        }

        Vector3 spawnPosition = bossInitialSpawnPoint != null ?
            bossInitialSpawnPoint.position :
            new Vector3(0, 5, 0);

        GameObject bossObj = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);

        currentBoss = bossObj.GetComponent<BossController>();

        if (currentBoss != null)
        {
            Debug.Log("🐉 Boss đã được spawn thành công!");
        }
        else
        {
            Debug.LogError("Prefab boss được spawn không có script BossController.cs!");
        }

        this.enabled = false;
    }

    // Public method để force spawn boss (để test)
    public void ForceSpawnBoss()
    {
        if (!bossSpawned)
        {
            StartCoroutine(SpawnBossWithWarning());
        }
    }
}