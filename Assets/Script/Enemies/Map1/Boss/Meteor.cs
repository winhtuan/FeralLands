using UnityEngine;

public class Meteor : MonoBehaviour
{
    [Header("Settings")]
    public float fallSpeed = 10f;
    public int damage = 30;
    public LayerMask playerLayer;
    public float lifetime = 5f;

    private Vector2 moveDirection = Vector2.down;
    private bool hasHit = false;

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;

        // Xoay sprite để hướng về phía di chuyển (giả sử sprite hướng xuống theo trục Y âm)
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + 90f); 
    }

    void Start()
    {
        // Tự động hủy sau một khoảng thời gian nếu không trúng gì
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Di chuyển theo hướng đã định
        transform.Translate(moveDirection * fallSpeed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        // Kiểm tra nếu trúng Player
        if (((1 << other.gameObject.layer) & playerLayer) != 0 || other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                hasHit = true;
                
                // Có thể thêm hiệu ứng nổ ở đây
                Debug.Log("Meteor hit player!");
                Destroy(gameObject);
            }
        }
        
        // Trúng mặt đất (giả sử mặt đất ở layer Ground hoặc có tag Ground)
        if (other.CompareTag("Ground"))
        {
            // Có thể spawn hiệu ứng va chạm mặt đất ở đây
            Destroy(gameObject);
        }
    }
}
