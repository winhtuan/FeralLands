using UnityEngine;

public class BossWakeUp : MonoBehaviour
{
    [Header("Kéo thả vào đây")]
    public GameObject bossObject; // Con Boss (đang bị tắt)
    public MonoBehaviour playerScript; // Script điều khiển nhân vật (Dreamshaper)
    public Rigidbody2D playerRb;       // Rigidbody của nhân vật

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Dừng nhân vật lại
            if (playerRb != null) playerRb.linearVelocity = Vector2.zero; // Unity 6 dùng linearVelocity

            // 2. Khóa nút bấm (Không cho đi tiếp)
            if (playerScript != null) playerScript.enabled = false;

            // 3. Thả Boss!
            if (bossObject != null)
            {
                bossObject.SetActive(true); // Bật Boss lên -> Nó sẽ tự rơi
            }

            // 4. Hủy cái còi này đi (để không kích hoạt lại)
            Destroy(gameObject);
        }
    }
}