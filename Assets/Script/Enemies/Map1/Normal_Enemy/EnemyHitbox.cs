using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public int damage = 1;
    private bool hasHit;

    // Mỗi lần hitbox được bật (Animation Event)
    public void ResetHitbox()
    {
        hasHit = false;
        // Debug.Log("Hitbox has been reset.");
    }

    private void OnEnable()
    {
        ResetHitbox();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (hasHit) return;

        // Tìm bất kỳ script nào kế thừa IDamageable (bao gồm DummyHealth và Player sau này)
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable == null) damageable = other.GetComponentInParent<IDamageable>();
        if (damageable == null) damageable = other.GetComponentInChildren<IDamageable>();

        if (damageable != null)
        {
            Debug.Log($"[HIT] Quái đánh trúng {other.name}, gây {damage} sát thương.");
            damageable.TakeDamage(damage);
            hasHit = true; // Chỉ đánh trúng 1 lần trong 1 chu kỳ bật hitbox
        }
    }
}
