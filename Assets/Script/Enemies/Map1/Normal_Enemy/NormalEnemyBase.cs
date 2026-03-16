using UnityEngine;
using System;
using System.Collections;

public abstract class NormalEnemyBase : MonoBehaviour, IDamageable
{
    protected Animator animator;
    protected Rigidbody2D rb;

    [Header("Stats")]
    public int maxHP = 50;
    public int currentHP;
    public bool IsDead { get; protected set; }

    public event Action OnDeath;
    public event Action<int> OnDamaged;

    [Header("Visual Effects")]
    public Color hitColor = Color.white;
    public float flashDuration = 0.1f;
    private Color originalColor;
    private SpriteRenderer sr;

    [Header("Special Attributes")]
    public bool inflictBurnOnHit = false; 
    public int burnDamagePerTick = 2;
    public float burnDuration = 3f;

    [Space]
    public bool inflictSlowOnHit = false; 
    public float slowPercent = 0.5f;
    public float slowDuration = 2f;

    [Header("Attack Settings")]
    public AttackHitboxController hitbox; // KÉO TỪ ATTACKHITBOX

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;

        currentHP = maxHP;

        if (hitbox == null)
        {
            // Chỉ cảnh báo nếu là quái đánh gần (không phải quái bắn đạn)
            EnemyMovement em = GetComponent<EnemyMovement>();
            bool ranged = em != null && em.isRanged;
            if (!ranged)
                Debug.LogWarning($"{name}: Không có AttackHitboxController!");
        }
    }

    public virtual void TakeDamage(int dmg)
    {
        if (IsDead) return;

        currentHP -= dmg;
        OnDamaged?.Invoke(dmg);

        // Hiệu ứng chớp trắng khi trúng đòn
        if (sr != null) StartCoroutine(HitFlashRoutine());

        if (currentHP <= 0)
            Die();
    }

    // --- HIỆU ỨNG HÌNH ẢNH ---
    private IEnumerator HitFlashRoutine()
    {
        sr.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        sr.color = originalColor;
    }

    public void SetSpeed(float speed)
    {
        if (animator != null)
            animator.SetFloat("Speed", speed);
    }

    public void Attack()
    {
        if (animator != null)
            animator.SetTrigger("Attack");
    }

    public virtual void Die()
    {
        if (IsDead) return;
        IsDead = true;

        OnDeath?.Invoke();
        if (animator != null)
            animator.SetTrigger("Die");
        
        // Disable collider to stop physical interactions
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Freeze Rigidbody if exists
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // Hủy quái khỏi map sau 1.5 giây để animation kịp chạy xong
        Destroy(gameObject, 1.5f);
    }

    // 👉 ANIMATION EVENT GỌI HÀM NÀY
    public void EnableHitbox()
    {
        if (hitbox != null)
            hitbox.EnableHitbox();
    }

    public void DisableHitbox()
    {
        if (hitbox != null)
            hitbox.DisableHitbox();
    }

    private void OnDestroy()
    {
        Debug.LogWarning($"[DESTROY] {gameObject.name} đã bị biến mất khỏi Hierarchy!");
    }
}
