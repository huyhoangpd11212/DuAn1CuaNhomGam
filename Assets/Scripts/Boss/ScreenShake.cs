using System.Collections;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [Tooltip("Duration of the shake effect.")]
    public float shakeDuration = 0.5f; // Thời gian rung
    [Tooltip("Magnitude (intensity) of the shake.")]
    public float shakeMagnitude = 0.1f; // Mức độ rung
    [Tooltip("How quickly the shake dies down.")]
    public float dampingSpeed = 1.0f; // Tốc độ giảm rung

    private Vector3 initialPosition; // Vị trí ban đầu của camera
    private bool isShaking = false;

    void Awake()
    {
        // Lưu vị trí ban đầu của camera khi game bắt đầu
        initialPosition = transform.localPosition;
    }

    // Hàm công khai để các script khác có thể gọi để kích hoạt rung
    public void TriggerShake()
    {
        if (!isShaking) // Đảm bảo không bắt đầu rung khi đang rung
        {
            StartCoroutine(ShakeCoroutine());
        }
    }

    IEnumerator ShakeCoroutine()
    {
        isShaking = true;
        float currentShakeDuration = shakeDuration; // Thời gian rung hiện tại

        while (currentShakeDuration > 0)
        {
            // Tạo vị trí rung ngẫu nhiên
            // Random.insideUnitSphere tạo ra một điểm ngẫu nhiên trong hình cầu đơn vị
            // (cho 3D, nhưng dùng 2D thì Z thường bằng 0 hoặc bỏ qua)
            // Nhân với shakeMagnitude để điều chỉnh cường độ rung
            Vector3 randomOffset = Random.insideUnitCircle * shakeMagnitude;

            // Áp dụng offset vào vị trí ban đầu của camera
            transform.localPosition = initialPosition + new Vector3(randomOffset.x, randomOffset.y, 0f);

            // Giảm dần thời gian rung và cường độ rung
            currentShakeDuration -= Time.deltaTime * dampingSpeed;

            yield return null; // Đợi một khung hình
        }

        // Đảm bảo camera trở về vị trí ban đầu sau khi hết rung
        transform.localPosition = initialPosition;
        isShaking = false;
    }

    // Nếu bạn muốn camera luôn theo dõi người chơi, hãy cập nhật initialPosition
    // trong hàm Update hoặc LateUpdate. Ví dụ:
    // void Update()
    // {
    //    if (!isShaking) // Chỉ cập nhật nếu không đang rung
    //    {
    //        initialPosition = transform.localPosition;
    //    }
    // }
}