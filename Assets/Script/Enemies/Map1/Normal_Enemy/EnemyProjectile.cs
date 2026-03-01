using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 6f;
    public int damage = 5;
    public float lifeTime = 3f;

    private Vector2 moveDir; // Hướng bay (có cả X và Y - bắn chéo được)

    // Được gọi bởi EnemyMovement.ShootProjectile()
    public void Init(Vector2 direction)
    {
        moveDir = direction.normalized;

        // Xoay đạn theo hướng bay (để sprite quay đúng góc)
        float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Bay theo hướng đã xoay (luôn là "phải" theo local space của đạn)
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable == null) damageable = other.GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            Debug.Log($"[BULLET HIT] Đạn trúng {other.name}, gây {damage} sát thương.");
            damageable.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
