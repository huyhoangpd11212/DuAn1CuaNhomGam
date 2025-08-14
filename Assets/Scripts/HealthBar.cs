using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("Heart UI References")]
    [SerializeField] private Image[] heartImages; // Array chứa 3 trái tim

    [Header("Heart Sprites")]
    [SerializeField] private Sprite fullHeartSprite;   // Sprite trái tim đầy
    [SerializeField] private Sprite emptyHeartSprite;  // Sprite trái tim rỗng

    [Header("Settings")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private bool animateHeartLoss = true;
    [SerializeField] private float animationDuration = 0.3f;

    private int currentHealth;

    void Start()
    {
        // Tự động tìm heart images nếu chưa assign
        if (heartImages == null || heartImages.Length == 0)
        {
            SetupHeartReferences();
        }

        // Initialize với full health
        currentHealth = maxHealth;
        UpdateHealthUI();

        Debug.Log($"💖 HealthBar initialized with {maxHealth} hearts");
    }

    private void SetupHeartReferences()
    {
        // Tìm các Image components trong children
        Image[] allImages = GetComponentsInChildren<Image>();

        // Filter để chỉ lấy heart images (loại bỏ background, v.v.)
        System.Collections.Generic.List<Image> hearts = new System.Collections.Generic.List<Image>();

        foreach (Image img in allImages)
        {
            // Kiểm tra tên GameObject chứa "Image" và có số (2), (3), (4)
            if (img.name.Contains("Image") && img.name.Contains("("))
            {
                hearts.Add(img);
                Debug.Log($"✅ Found heart: {img.name}");
            }
        }

        // Sort theo tên để đảm bảo thứ tự đúng
        hearts.Sort((a, b) => a.name.CompareTo(b.name));
        heartImages = hearts.ToArray();

        Debug.Log($"🔍 Auto-found {heartImages.Length} heart images");
    }

    // Method được gọi từ PlayerController khi mất máu
    public void UpdateHealth(int newHealth, int maxHP)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHP);
        maxHealth = maxHP;

        UpdateHealthUI();

        Debug.Log($"💔 Health updated: {currentHealth}/{maxHealth}");
    }

    private void UpdateHealthUI()
    {
        if (heartImages == null || heartImages.Length == 0)
        {
            Debug.LogWarning("⚠️ No heart images assigned!");
            return;
        }

        // Cập nhật từng trái tim
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] != null)
            {
                // Nếu index < currentHealth thì hiện trái tim đầy, ngược lại thì rỗng
                bool shouldShowFull = i < currentHealth;
                UpdateSingleHeart(i, shouldShowFull);
            }
        }
    }

    private void UpdateSingleHeart(int heartIndex, bool isFull)
    {
        if (heartIndex >= heartImages.Length || heartImages[heartIndex] == null)
            return;

        Image heartImage = heartImages[heartIndex];

        if (isFull)
        {
            // Hiện trái tim đầy
            if (fullHeartSprite != null)
            {
                heartImage.sprite = fullHeartSprite;
            }
            heartImage.color = Color.white; // Màu bình thường
            heartImage.gameObject.SetActive(true);
        }
        else
        {
            // Hiện trái tim rỗng hoặc ẩn
            if (emptyHeartSprite != null)
            {
                heartImage.sprite = emptyHeartSprite;
                heartImage.color = Color.gray; // Màu xám
            }
            else
            {
                // Nếu không có sprite rỗng thì ẩn luôn
                heartImage.gameObject.SetActive(false);
            }

            // Animation khi mất máu
            if (animateHeartLoss)
            {
                StartCoroutine(AnimateHeartLoss(heartImage));
            }
        }
    }

    private System.Collections.IEnumerator AnimateHeartLoss(Image heartImage)
    {
        // Animation scale down-up
        Vector3 originalScale = heartImage.transform.localScale;

        // Scale down
        float timer = 0f;
        while (timer < animationDuration / 2)
        {
            timer += Time.unscaledDeltaTime;
            float scaleMultiplier = Mathf.Lerp(1f, 0.5f, timer / (animationDuration / 2));
            heartImage.transform.localScale = originalScale * scaleMultiplier;
            yield return null;
        }

        // Scale back up
        timer = 0f;
        while (timer < animationDuration / 2)
        {
            timer += Time.unscaledDeltaTime;
            float scaleMultiplier = Mathf.Lerp(0.5f, 1f, timer / (animationDuration / 2));
            heartImage.transform.localScale = originalScale * scaleMultiplier;
            yield return null;
        }

        // Ensure final scale
        heartImage.transform.localScale = originalScale;
    }

    // Public methods để control từ bên ngoài
    public void TakeDamage(int damage = 1)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        UpdateHealthUI();

        Debug.Log($"💔 Took {damage} damage! Health: {currentHealth}/{maxHealth}");
    }

    public void Heal(int healAmount = 1)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        UpdateHealthUI();

        Debug.Log($"💚 Healed {healAmount}! Health: {currentHealth}/{maxHealth}");
    }

    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateHealthUI();
    }

    // Getters
    public int GetCurrentHealth() { return currentHealth; }
    public int GetMaxHealth() { return maxHealth; }
    public bool IsAlive() { return currentHealth > 0; }
}