using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Tốc độ di chuyển của đạn (phải NHANH HƠN tốc độ player để đuổi kịp và vượt qua)")]
    public float speed = 30f; // Tốc độ cao để đạn bay nhanh và không bị lùi lại
    
    [Header("Lifetime Settings")]
    [Tooltip("Thời gian tồn tại của đạn (giây) - đảm bảo đạn bay đủ xa để hero thấy và né")]
    public float maxLifetime = 20f; // Tăng lên 20 giây để đạn tồn tại lâu hơn
    
    [Header("Damage Settings")]
    [Tooltip("Bật/tắt việc gây sát thương khi đạn chạm Player (tắt để dễ quan sát quỹ đạo đạn)")]
    public bool enableDamage = false; // MẶC ĐỊNH: KHÔNG gây sát thương để bạn dễ test quỹ đạo
    
    [Tooltip("Sát thương đạn gây ra khi chạm vào Player (khi enableDamage = true)")]
    public int damage = 15; // Sát thương mỗi viên đạn
    
    private float lifetime; // Thời gian đã tồn tại
    private Rigidbody2D rb; // Dùng Rigidbody để di chuyển với tốc độ tuyệt đối
    private Vector3 startPosition; // Vị trí bắt đầu để debug
    
    void Start()
    {
        lifetime = 0f;
        startPosition = transform.position;
        
        // Lấy hoặc thêm Rigidbody2D để di chuyển với tốc độ tuyệt đối
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // Setup Rigidbody
        rb.gravityScale = 0; // Không bị ảnh hưởng bởi gravity
        rb.freezeRotation = true; // Không quay
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Detect collision tốt hơn
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // Mượt hơn
        
        // Đặt tốc độ ban đầu về phía phải (world space)
        rb.linearVelocity = Vector2.right * speed;
        
        // Tự động destroy sau maxLifetime giây
        Destroy(gameObject, maxLifetime);
        
        Debug.Log($"[ObstacleMover] Đạn được spawn tại {transform.position}, tốc độ: {speed}, lifetime: {maxLifetime}s, damage: {damage}, enableDamage={enableDamage}");
    }
    
    void Update()
    {
        // Cập nhật lifetime
        lifetime += Time.deltaTime;
        
        // Đảm bảo đạn luôn di chuyển với tốc độ cố định về phía phải
        // Sử dụng Rigidbody để đảm bảo tốc độ tuyệt đối, không bị ảnh hưởng bởi camera
        if (rb != null)
        {
            // Luôn đảm bảo velocity về phía phải với tốc độ cố định
            rb.linearVelocity = Vector2.right * speed;
        }
        else
        {
            // Fallback nếu không có Rigidbody
            transform.Translate(Vector2.right * speed * Time.deltaTime, Space.World);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Chỉ xử lý khi chạm vào Player
        if (!collision.CompareTag("Player"))
            return;

        // Nếu đang ở chế độ TEST quỹ đạo (enableDamage = false) thì cho xuyên qua player, KHÔNG gây sát thương
        if (!enableDamage)
        {
            Debug.Log($"[ObstacleMover] Đạn đi xuyên qua Player (TEST MODE) sau {lifetime:F2} giây.");
            return;
        }

        // === PHẦN DƯỚI CHỈ CHẠY KHI enableDamage = true ===

        // Gây sát thương cho Player
        PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            // Thử tìm trong parent hoặc children
            playerHealth = collision.GetComponentInParent<PlayerHealth>();
            if (playerHealth == null)
                playerHealth = collision.GetComponentInChildren<PlayerHealth>();
        }
        
        if (playerHealth != null)
        {
            Debug.Log($"[ObstacleMover] Đạn trúng Player sau {lifetime:F2} giây! Gây {damage} sát thương");
            playerHealth.TakeDamage(damage);
        }
        else
        {
            // Fallback: Tìm IDamageable interface
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable == null)
                damageable = collision.GetComponentInParent<IDamageable>();
            if (damageable == null)
                damageable = collision.GetComponentInChildren<IDamageable>();
            
            if (damageable != null)
            {
                Debug.Log($"[ObstacleMover] Đạn trúng Player (qua IDamageable) sau {lifetime:F2} giây! Gây {damage} sát thương");
                damageable.TakeDamage(damage);
            }
            else
            {
                Debug.LogWarning($"[ObstacleMover] Không tìm thấy PlayerHealth hoặc IDamageable trên {collision.name}");
            }
        }
        
        // Destroy đạn sau khi gây sát thương
        Destroy(gameObject);
    }
    
    void OnDestroy()
    {
        float distanceTraveled = Vector2.Distance(startPosition, transform.position);
        Debug.Log($"[ObstacleMover] Đạn bị destroy sau {lifetime:F2} giây, đã bay được {distanceTraveled:F2} units");
    }
}
