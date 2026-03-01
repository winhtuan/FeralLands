using UnityEngine;

public class Portal : MonoBehaviour
{
    [Tooltip("Gõ đúng tên Scene Boss vào đây")]
    public string nextSceneName = "Boss1Scene";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Nếu cái chạm vào cổng có Tag là Player
        if (collision.CompareTag("Player"))
        {
            // Gọi Canvas khép màn hình và chuyển map
            SceneTransitionManager.Instance.TeleportToMap(nextSceneName);
        }
    }
}