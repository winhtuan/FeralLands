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
        // Nếu layer của đối tượng chạm phải KHÔNG NẰM TRONG hitLayer, bỏ qua, KHÔNG HỦY NGỌC
        if (hitLayer != 0 && ((1 << other.gameObject.layer) & hitLayer) == 0) return;

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable == null) damageable = other.GetComponentInParent<IDamageable>();
        if (damageable == null) damageable = other.GetComponentInChildren<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            Debug.Log("Orb damaged: " + other.gameObject.name);
        }

        // Chỉ nổ hủy viên ngọc nếu nó đụng trúng người nằm trong LayerMask hợp lệ (Enemy)
        Destroy(gameObject); 
    }
}
