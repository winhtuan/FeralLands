using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 6f;
    public int damage = 5;
    public float lifeTime = 3f;

    private Vector2 moveDir; 
    private bool shouldBurn;
    private int burnDmg;
    private float burnDur;

    private bool shouldSlow;
    private float slowPct;
    private float slowDur;

    // Được gọi bởi EnemyMovement.ShootProjectile()
    public void Init(Vector2 direction, NormalEnemyBase source = null)
    {
        moveDir = direction.normalized;
        if (source != null)
        {
            shouldBurn = source.inflictBurnOnHit;
            burnDmg = source.burnDamagePerTick;
            burnDur = source.burnDuration;

            shouldSlow = source.inflictSlowOnHit;
            slowPct = source.slowPercent;
            slowDur = source.slowDuration;
        }

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

            PlayerStatusEffects playerEffects = other.GetComponent<PlayerStatusEffects>();
            if (playerEffects != null)
            {
                if (shouldBurn) playerEffects.ApplyBurn(burnDmg, burnDur);
                if (shouldSlow) playerEffects.ApplySlow(slowPct, slowDur);
            }
        }

        Destroy(gameObject);
    }
}
