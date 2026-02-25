using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    public Vector3 offset = new Vector3(2f, 1.5f, -10f);

    [Header("Settings")]
    public float smoothTime = 0.25f;
    private Vector3 currentVelocity;

    [Header("Axis Locking")]
    [Tooltip("Tích vào đây nếu muốn Camera KHÔNG di chuyển lên xuống (Dùng cho map chạy vô tận)")]
    public bool lockY = true;

    [Header("Map Limits (Dùng cho Map Boss)")]
    [Tooltip("Tích vào đây để giới hạn camera trong khung ảnh")]
    public bool useLimits = false;
    public float minX, maxX;
    public float minY, maxY; // Thêm giới hạn Y cho map Boss

    void LateUpdate()
    {
        if (player == null) return;

        // 1. Tính toán vị trí mục tiêu
        float targetX = player.position.x + offset.x;

        // Logic chọn Y: Nếu khóa thì lấy vị trí hiện tại, không khóa thì theo nhân vật
        float targetY = lockY ? transform.position.y : player.position.y + offset.y;

        Vector3 targetPosition = new Vector3(targetX, targetY, transform.position.z);

        // 2. Kẹp vị trí trong giới hạn (Nếu bật useLimits)
        if (useLimits)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);

            // Nếu không khóa Y thì mới kẹp giới hạn Y
            if (!lockY)
            {
                targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
            }
        }

        // 3. Di chuyển mượt
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
    }
}