using UnityEngine;

/// <summary>
/// Script này gắn vào các Prefab hiệu ứng kỹ năng (Lửa, Meteor, Cọc nhọn...).
/// Tự động gây sát thương cho Player khi chạm vào.
/// </summary>
public class BossSkillEffect : MonoBehaviour
{
    [Header("Base Settings")]
    public int damage = 25;
    public float lifetime = 3.0f;
    public LayerMask playerLayer;

    [Header("Status Effects")]
    public bool isPoisonZone = false;
    public bool isSlowEffect = false;
    public float slowAmount = 0.5f;
    public float effectDuration = 2.0f;
    public float tickInterval = 0.5f;

    private float tickTimer;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isPoisonZone)
        {
            tickTimer -= Time.deltaTime;
            if (tickTimer <= 0)
            {
                ApplyDamage(other);
                tickTimer = tickInterval;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPoisonZone) return; // Poison handled by OnTriggerStay
        ApplyDamage(other);
    }

    private void ApplyDamage(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var playerHealth = other.GetComponent<PlayerHealth>();
            var statusEffects = other.GetComponent<PlayerStatusEffects>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            if (statusEffects != null)
            {
                if (isSlowEffect) statusEffects.ApplySlow(slowAmount, effectDuration);
                // Nếu là vùng độc, có thể gọi ApplyBurn (đã có trong script của bạn) làm hiệu ứng độc
                if (isPoisonZone) statusEffects.ApplyBurn(damage / 2, effectDuration);
            }
        }
    }
}
