// File: Assets/Scripts/ItemPickup.cs
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Tooltip("Loại item này là gì")]
    [SerializeField] private ItemType itemType;

    [Tooltip("Giá trị của item (ví dụ: lượng máu hồi, số đạn thêm, lượng nhiệt giảm)")]
    [SerializeField] private float itemValue = 1f; // Giá trị mặc định, sẽ được cấu hình cho từng prefab

    [Tooltip("Tốc độ item di chuyển xuống (tùy chọn, tạo cảm giác rơi)")]
    [SerializeField] private float dropSpeed = 2f;

    void Update()
    {
        // Item rơi xuống để người chơi nhặt
        transform.Translate(Vector2.down * dropSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Giả sử người chơi có Tag "Player"
        if (other.CompareTag("Player"))
        {
            // Gọi phương thức xử lý item trên script người chơi
            PlayerController player = other.GetComponent<PlayerController>(); // Giả định script người chơi là PlayerController.cs
            if (player != null)
            {
                player.ApplyItemEffect(itemType, itemValue);
                Debug.Log("Player picked up " + itemType.ToString() + " with value " + itemValue);
                Destroy(gameObject); // Hủy item sau khi nhặt
            }
        }
    }
}