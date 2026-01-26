//using UnityEngine;

//public class CameraFollow : MonoBehaviour
//{
//    [Header("Target")]
//    public Transform player;      // Kéo Dreamshaper vào đây
//    public Vector3 offset = new Vector3(0, 2f, -10f); // Khoảng cách camera với nhân vật

//    [Header("Settings")]
//    public float smoothTime = 0.25f; // Thời gian trễ (càng cao càng mượt nhưng chậm)
//    private Vector3 currentVelocity;

//    [Header("Limits (Optional)")]
//    public bool useLimits = false;
//    public float minX, maxX;

//    void LateUpdate() // Dùng LateUpdate để camera đi sau khi nhân vật đã di chuyển xong
//    {
//        if (player == null) return;
//        Debug.Log("Camera đang chạy! Vị trí nhân vật: " + player.position);

//        //// Tính toán vị trí mục tiêu
//        //Vector3 targetPosition = player.position + offset;

//        //// Giữ nguyên Z của camera (để không bị mất hình)
//        //targetPosition.z = transform.position.z;

//        //// Nếu có giới hạn map (minX, maxX)
//        //if (useLimits)
//        //{
//        //    targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
//        //}

//        //// Di chuyển mượt mà từ vị trí hiện tại đến mục tiêu
//        //transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
//        // 1. Chỉ lấy vị trí X của nhân vật, còn Y và Z giữ nguyên theo Camera hiện tại
//        Vector3 targetPosition = new Vector3(
//            player.position.x + offset.x,
//            transform.position.y,        // Giữ nguyên độ cao Y hiện tại của Camera
//            transform.position.z         // Giữ nguyên độ sâu Z
//        );

//        // 2. Nếu có giới hạn map (minX, maxX)
//        if (useLimits)
//        {
//            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
//        }

//        // 3. Di chuyển mượt
//        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
//    }
//}


using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    public Vector3 offset = new Vector3(0, 2f, -10f);

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