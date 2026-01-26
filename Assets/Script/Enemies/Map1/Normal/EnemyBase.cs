using UnityEngine;
using System;

public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int maxHP = 50;
    protected int currentHP;

    public event Action OnDeath;
    public event Action<int> OnDamaged;

    protected virtual void Awake()
    {
        currentHP = maxHP;
    }

    public virtual void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        OnDamaged?.Invoke(dmg);

        if (currentHP <= 0)
            Die();
    }

    protected virtual void Die()
    {
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}
