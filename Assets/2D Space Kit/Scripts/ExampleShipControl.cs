using UnityEngine;
using System.Collections;

public class ExampleShipControl : MonoBehaviour
{
    public float acceleration_amount = 1f;
    public float rotation_speed = 1f;
    public GameObject turret;
    public float turret_rotation_speed = 3f;

    private Rigidbody2D rb;

    void Start()
    {
        // Lấy Rigidbody2D một lần khi bắt đầu để tránh gọi nhiều lần trong Update
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component not found on the ship!");
        }

        // Khóa con trỏ chuột khi game bắt đầu
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Xử lý chuyển đổi trạng thái con trỏ chuột khi nhấn Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        // Đảm bảo Rigidbody2D tồn tại trước khi sử dụng
        if (rb == null) return;

        // Chuyển động tiến (W) và lùi (S)
        if (Input.GetKey(KeyCode.W))
        {
            rb.AddForce(transform.up * acceleration_amount * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {
            rb.AddForce(-transform.up * acceleration_amount * Time.deltaTime);
        }

        // Chuyển động ngang (strafe) khi giữ Shift
        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.LeftShift))
        {
            rb.AddForce(-transform.right * acceleration_amount * 0.6f * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.LeftShift))
        {
            rb.AddForce(transform.right * acceleration_amount * 0.6f * Time.deltaTime);
        }

        // Xoay tàu
        if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.LeftShift))
        {
            rb.AddTorque(-rotation_speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.LeftShift))
        {
            rb.AddTorque(rotation_speed * Time.deltaTime);
        }

        // Phanh (C)
        if (Input.GetKey(KeyCode.C))
        {
            rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, 0, rotation_speed * 0.06f * Time.deltaTime);
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, acceleration_amount * 0.06f * Time.deltaTime);
        }

        // Đặt lại vị trí (H)
        if (Input.GetKey(KeyCode.H))
        {
            transform.position = Vector3.zero;
        }
    }
}