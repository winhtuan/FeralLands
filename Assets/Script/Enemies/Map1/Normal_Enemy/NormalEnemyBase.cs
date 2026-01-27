using UnityEngine;
using System;

public abstract class NormalEnemyBase : MonoBehaviour, IDamageable
{
    protected Animator animator;

    [Header("Stats")]
    public int maxHP = 50;
    protected int currentHP;
    public bool IsDead { get; protected set; }

    public event Action OnDeath;
    public event Action<int> OnDamaged;

    [Header("Attack Settings")]
    public AttackHitboxController hitbox; // KÉO TỪ ATTACKHITBOX

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        currentHP = maxHP;

        if (hitbox == null)
        {
            Debug.LogError($"{name}: CHƯA GÁN AttackHitboxController!");
        }
    }

    public virtual void TakeDamage(int dmg)
    {
        if (IsDead) return;

        currentHP -= dmg;
        OnDamaged?.Invoke(dmg);

        if (currentHP <= 0)
            Die();
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
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }
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
}
