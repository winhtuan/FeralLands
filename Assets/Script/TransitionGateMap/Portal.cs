using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("Scene Transition")]
    [Tooltip(
        "Nhập đúng tên Scene muốn chuyển đến (phân biệt hoa thường). Ví dụ: Boss1Scene, MapBeach 1, FeralLand_Normal_1"
    )]
    public string nextSceneName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Nếu cái chạm vào cổng có Tag là Player
        if (collision.CompareTag("Player"))
        {
            if (string.IsNullOrEmpty(nextSceneName))
            {
                Debug.LogWarning(
                    $"[Portal] '{gameObject.name}': nextSceneName chưa được điền trong Inspector!",
                    this
                );
                return;
            }

            // Gọi Canvas khép màn hình và chuyển map
            SceneTransitionManager.Instance.TeleportToMap(nextSceneName);
        }
    }
}
