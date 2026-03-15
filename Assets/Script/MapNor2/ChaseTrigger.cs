using UnityEngine;

public class ChaseTrigger : MonoBehaviour
{
    public ChaseManager chaseManager;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem có phải nhân vật chạm vào không (dựa vào Tag)
        if (collision.CompareTag("Player"))
        {
            chaseManager.StartChase();

            // Xóa Trigger này đi để tránh bị gọi lại nhiều lần
            gameObject.SetActive(false);
        }
    }
}
