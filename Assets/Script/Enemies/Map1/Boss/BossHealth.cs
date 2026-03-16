using UnityEngine;
using System;

public class BossHealth : EnemyBase
{
    [Header("Boss Stats")]
    public int bossMaxHP = 500;

    [Header("Invincibility")]
    public float invincibleTime = 0.5f;
    private float invincibleTimer;

    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D col;
    private VaathBoss bossController;

    // Event cho UI health bar
    public event Action<int, int> OnHealthChanged; // (currentHP, maxHP)

    protected override void Awake()
    {
        // Set max HP theo boss
        maxHP = bossMaxHP;
        base.Awake();

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        bossController = GetComponent<VaathBoss>();
    }

    void Start()
    {
        // Thông báo HP ban đầu cho UI
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    void Update()
    {
        invincibleTimer -= Time.deltaTime;
    }

    public override void TakeDamage(int dmg)
    {
        // Invincibility check
        if (invincibleTimer > 0) return;

        invincibleTimer = invincibleTime;

        // Giảm HP
        currentHP -= dmg;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        Debug.Log($"Boss HP: {currentHP}/{maxHP}");

        // Thông báo UI update
        OnHealthChanged?.Invoke(currentHP, maxHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    protected override void Die()
    {
        Debug.Log("Boss Defeated!");

        // Trigger death animation
        animator.SetTrigger("Death");

        // Tắt AI controller
        if (bossController != null)
            bossController.enabled = false;

        // Stop movement
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        // Tắt collider
        if (col != null)
            col.enabled = false;
    }

    // Animation Event - gọi từ frame cuối của Death animation
    public void OnDeathAnimationEnd()
    {
        Debug.Log("Boss Death Animation Finished");
        Destroy(gameObject);
    }

    // Public getter cho current HP
    public int GetCurrentHP() => currentHP;
    public int GetMaxHP() => maxHP;
}
