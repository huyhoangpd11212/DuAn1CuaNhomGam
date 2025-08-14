using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject bossUIPanel;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text bossNameText;
    [SerializeField] private Text healthText;
    [SerializeField] private Image healthFillImage;

    [Header("Settings")]
    [SerializeField] private string bossName = "MEGA DESTROYER";
    [SerializeField] private Color[] healthColors = { Color.green, Color.yellow, Color.red };

    private BossController currentBoss;

    void Start()
    {
        if (bossUIPanel != null)
            bossUIPanel.SetActive(false);
    }

    public void SetBoss(BossController boss)
    {
        currentBoss = boss;
        if (boss != null)
        {
            ShowBossUI(boss.GetMaxHealth());
        }
    }

    public void ShowBossUI(int maxHealth)
    {
        if (bossUIPanel != null)
            bossUIPanel.SetActive(true);

        if (bossNameText != null)
            bossNameText.text = bossName;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }

        UpdateHealthDisplay(maxHealth, maxHealth);
        Debug.Log("🐉 Boss Health UI activated!");
    }

    void Update()
    {
        if (currentBoss != null)
        {
            UpdateHealthDisplay(currentBoss.GetCurrentHealth(), currentBoss.GetMaxHealth());
        }
    }

    private void UpdateHealthDisplay(int currentHealth, int maxHealth)
    {
        if (healthSlider != null)
            healthSlider.value = currentHealth;

        if (healthText != null)
            healthText.text = $"{currentHealth} / {maxHealth}";

        // Thay đổi màu theo % máu
        if (healthFillImage != null && healthColors.Length >= 3)
        {
            float healthPercent = (float)currentHealth / maxHealth;
            if (healthPercent > 0.66f)
                healthFillImage.color = healthColors[0]; // Xanh
            else if (healthPercent > 0.33f)
                healthFillImage.color = healthColors[1]; // Vàng
            else
                healthFillImage.color = healthColors[2]; // Đỏ
        }
    }

    public void HideBossUI()
    {
        if (bossUIPanel != null)
            bossUIPanel.SetActive(false);

        Debug.Log("🐉 Boss Health UI hidden!");
    }
}