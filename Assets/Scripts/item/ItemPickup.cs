using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Settings")]
    [Tooltip("Chọn loại item: Health, Speed, Score")]
    [SerializeField] private string itemType = "Health";

    [Tooltip("Giá trị của item (vd: lượng máu hồi, điểm số, etc.)")]
    [SerializeField] private float itemValue = 1f;

    [Tooltip("Tốc độ rơi xuống")]
    [SerializeField] private float dropSpeed = 2f;

    void Update()
    {
        // Item rơi xuống đều
        transform.Translate(Vector3.down * dropSpeed * Time.deltaTime);

        // Destroy nếu rơi ra ngoài màn hình
        if (transform.position.y < -6f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra nếu người chơi nhặt item
        if (other.CompareTag("Player"))
        {
            // ✅ SỬA LỖI: Xử lý item effects trực tiếp thay vì gọi ApplyItemEffect
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                ApplyItemEffectDirectly(player);

                Debug.Log($"Player picked up {itemType} with value {itemValue}");
                Destroy(gameObject); // Hủy item sau khi nhặt
            }
        }
    }

    // ✅ THÊM method mới để xử lý effects trực tiếp
    private void ApplyItemEffectDirectly(PlayerController player)
    {
        switch (itemType.ToLower())
        {
            case "health":
            case "heal":
                player.Heal((int)itemValue);
                Debug.Log($"💚 Player healed: +{itemValue} health");
                break;

            case "speed":
            case "speedboost":
                // Tạm thời tăng speed (có thể implement sau)
                Debug.Log($"⚡ Speed boost collected (not implemented yet)");
                break;

            case "score":
            case "points":
                PlayerController.AddScore((int)itemValue);
                Debug.Log($"💰 Score added: +{itemValue} points");
                break;

            case "ammo":
            case "firerateboost":
                Debug.Log($"🔫 Fire rate boost collected (not implemented yet)");
                break;

            default:
                Debug.LogWarning($"⚠️ Unknown item type: {itemType}");
                // Default: give points
                PlayerController.AddScore(10);
                break;
        }
    }
}