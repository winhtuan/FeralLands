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
    private IBossController bossController;
    private bool isDead = false;

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
        bossController = GetComponent<IBossController>();
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

        // Trigger damage animation nếu có
        // animator.SetTrigger("Hit");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    protected override void Die()
    {
        if (isDead) return; // Đã chết rồi thì không chạy lại nữa
        isDead = true;
        
        if (currentHP > 0) return; 
        
        Debug.Log("Boss Defeated!");

        // Kích hoạt event cho các Manager (như EnemyClearManager) biết
        InvokeOnDeath();

        // Trigger death animation
        if (animator != null)
        {
            Debug.Log($"[BossHealth] 💀 Đang gọi Animation 'Death' trên {gameObject.name}");
            animator.SetBool("isRunning", false);
            animator.SetTrigger("Death");
        }
        else
        {
            Debug.LogError($"[BossHealth] ❌ KHÔNG tìm thấy Animator trên {gameObject.name} để chạy chiêu chết!");
        }

        // Tắt AI controller ngay lập tức
        if (bossController != null)
        {
            bossController.isBattleStarted = false;
            (bossController as MonoBehaviour).enabled = false; 
        }

        // Stop movement hoàn toàn
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        // Tắt va chạm (Collider)
        if (col != null)
            col.enabled = false;

        // DÙNG DỰ PHÒNG: Tự destroy sau 5 giây nếu không có Animation Event gọi hàm OnDeathAnimationEnd
        Destroy(gameObject, 5.0f);
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
