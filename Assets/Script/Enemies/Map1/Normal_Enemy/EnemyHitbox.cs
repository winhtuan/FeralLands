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

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDealDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDealDamage(other);
    }

    private void TryDealDamage(Collider2D other)
    {
        if (hasHit) return;

        // CHỈ gây sát thương lên Player. Quái sẽ không bao giờ đánh lẫn nhau dù đứng chồng lên nhau.
        if (!other.CompareTag("Player")) return;

        // Tránh tự đánh chính mình (Đề phòng trường hợp Player cũng có tag Player - thường là đúng)
        if (transform.parent != null && other.gameObject == transform.parent.gameObject) return;

        // Tìm bất kỳ script nào kế thừa IDamageable
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable == null) damageable = other.GetComponentInParent<IDamageable>();
        if (damageable == null) damageable = other.GetComponentInChildren<IDamageable>();

        if (damageable != null)
        {
            Debug.Log($"[HIT] {transform.parent.name} đánh trúng {other.name}, gây {damage} sát thương.");
            damageable.TakeDamage(damage);
            hasHit = true; 

            // Áp dụng hiệu ứng nếu quái có trait đặc biệt
            NormalEnemyBase enemyBase = GetComponentInParent<NormalEnemyBase>();
            PlayerStatusEffects playerEffects = other.GetComponent<PlayerStatusEffects>();

            if (enemyBase != null && playerEffects != null)
            {
                if (enemyBase.inflictBurnOnHit) 
                    playerEffects.ApplyBurn(enemyBase.burnDamagePerTick, enemyBase.burnDuration);
                
                if (enemyBase.inflictSlowOnHit) 
                    playerEffects.ApplySlow(enemyBase.slowPercent, enemyBase.slowDuration);
            }
        }
    }
}
