// File: Assets/Scripts/UI/HealthUIController.cs
using UnityEngine;
using UnityEngine.UI;

public class HealthUIController : MonoBehaviour
{
    [Tooltip("Danh sách các Image của trái tim")]
    [SerializeField] private Image[] hearts;

    /// <summary>
    /// Cập nhật giao diện trái tim dựa trên máu hiện tại của người chơi.
    /// </summary>
    /// <param name="currentHealth">Máu hiện tại của người chơi.</param>
    public void UpdateHealth(int currentHealth)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < currentHealth)
            {
                // Nếu chỉ số nhỏ hơn máu hiện tại, hiển thị trái tim
                hearts[i].enabled = true;
            }
            else
            {
                // Ngược lại, ẩn trái tim đi
                hearts[i].enabled = false;
            }
        }
    }
}