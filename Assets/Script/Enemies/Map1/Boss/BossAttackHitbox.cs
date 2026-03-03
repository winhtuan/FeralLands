using UnityEngine;

public class BossAttackHitbox : MonoBehaviour
{
    [Header("Settings")]
    public LayerMask playerLayer;

    private int damage;
    private float lifetime;
    private bool hasHit = false; // Đảm bảo chỉ hit 1 lần

    public void Init(float direction, int dmg, float lifeTime)
    {
        damage = dmg;
        lifetime = lifeTime;

        // Flip hitbox theo hướng boss
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;

        // Tự destroy sau lifetime
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Đã hit rồi thì không hit nữa
        if (hasHit) return;

        // Ưu tiên check tag Player để Hero luôn nhận damage bất kể layer hay mask
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                hasHit = true;
                Debug.Log($"Boss hitbox dealt {damage} damage to player: {other.name}");

                // Destroy hitbox ngay sau khi hit
                Destroy(gameObject);
            }
        }
    }

    // Visualize hitbox trong Scene view
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f); // Đỏ trong suốt
        Gizmos.DrawCube(transform.position, transform.localScale);
    }
}
