using UnityEngine;

public class BossWakeUp : MonoBehaviour
{
    [Header("Kéo thả vào đây")]
    public GameObject bossObject;    // Con Boss (đang bị tắt)
    public Dreamshaper player;       // Kéo nhân vật (Dreamshaper) vào đây
    public Rigidbody2D playerRb;     // Rigidbody của nhân vật

    private void Start()
    {
        // Tự động tìm Dreamshaper nếu chưa kéo vào Inspector
        if (player == null)
            player = FindFirstObjectByType<Dreamshaper>();

        if (playerRb == null && player != null)
            playerRb = player.GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("[BossWakeUp] Player chạm trigger → Bắt đầu khoá + thả Boss!");

            // 1. Dừng nhân vật lại
            if (playerRb != null) playerRb.linearVelocity = Vector2.zero;

            // 2. Khóa toàn bộ module điều khiển
            if (player != null)
            {
                player.SetAllModulesEnabled(false);
                Debug.Log("[BossWakeUp] Đã khoá Player.");
            }
            else
            {
                Debug.LogWarning("[BossWakeUp] player là null! Kéo Dreamshaper vào Inspector.");
            }

            // 3. Thả Boss!
            if (bossObject != null)
                bossObject.SetActive(true);

            // 4. Hủy trigger (không kích hoạt lại)
            Destroy(gameObject);
        }
    }
}
