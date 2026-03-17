using UnityEngine;

public class LightOrb : MonoBehaviour
{
    public float speed = 25f;   
    public float lifeTime = 3f;   
    public int damage = 10;
    public LayerMask hitLayer; // Thêm LayerMask để kĩ năng biết nên đánh trúng ai

    Rigidbody2D rb;

    public void Launch(Vector2 direction, Transform shooter, int dmg)
    {
        damage = dmg;
        rb = GetComponent<Rigidbody2D>();

        rb.linearVelocity = direction.normalized * speed;

        Collider2D orbCollider = GetComponent<Collider2D>();
        Collider2D shooterCollider = shooter.GetComponent<Collider2D>();
        
        if (orbCollider != null && shooterCollider != null)
        {
            // tránh va chạm vật lý với player
            Physics2D.IgnoreCollision(orbCollider, shooterCollider);
        }

        // auto destroy
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Tránh tự huỷ khi vừa mới sinh ra nếu lỡ đụng người chơi
        if (other.gameObject.CompareTag("Player")) return;

        // 2. Nếu người dùng quên set hitLayer (để = Nothing / 0) thì tránh việc đụng đất nổ ngay.
        if (hitLayer == 0)
        {
            // Tạm thời nếu hitLayer = 0, chỉ nổ khi có IDamageable
            IDamageable dmgTmp = other.GetComponent<IDamageable>() ?? other.GetComponentInParent<IDamageable>() ?? other.GetComponentInChildren<IDamageable>();
            if (dmgTmp != null) 
            {
                dmgTmp.TakeDamage(damage);
                Destroy(gameObject);
            }
            return;
        }

        // 3. Nếu layer của đối tượng chạm phải KHÔNG NẰM TRONG hitLayer, bỏ qua, KHÔNG HỦY NGỌC
        if (((1 << other.gameObject.layer) & hitLayer) == 0) return;

        IDamageable damageable = other.GetComponent<IDamageable>() ?? other.GetComponentInParent<IDamageable>() ?? other.GetComponentInChildren<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            Debug.Log("Orb damaged: " + other.gameObject.name);
        }

        // Đã đụng vào mục tiêu hợp lệ (nằm trong hitLayer) thì huỷ ngọc
        Destroy(gameObject); 
    }
}
