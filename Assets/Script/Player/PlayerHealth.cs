using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public int maxHealth = 100;
    public int currentHealth;
    [HideInInspector] public int baseMaxHealth; // captured in Awake before any augment modifies maxHealth

    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D col;
    private Dreamshaper controller;
    private float invincibleTime = 0.2f;
    private float invincibleTimer;
    private bool isDead;
    void Awake()
    {
        baseMaxHealth = maxHealth; // lock in Inspector value before any save-load modifies it
        currentHealth = maxHealth;

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        controller = GetComponent<Dreamshaper>();
    }

    void Update()
    {
        invincibleTimer -= Time.deltaTime;
    }

    public void TakeDamage(int damage)
    {
        if (invincibleTimer > 0) return;

        invincibleTimer = invincibleTime;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("Player HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Method để set health trực tiếp (dùng cho boss kill player)
    public void SetHealth(int health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Player Dead!");

        // Xoá save — player chết thì reset tiến trình
        SaveManager.Instance?.DeleteSave();

        // Trigger animation
        animator.SetTrigger("Death");

        // Tắt điều khiển
        if (controller != null)
            controller.enabled = false;

        // Stop movement ngay
        rb.linearVelocity = Vector2.zero;

        // Không cho rơi trong lúc animation chết
        rb.gravityScale = 0;
    }

    // GỌI TỪ ANIMATION EVENT (frame cuối Die)
    public void OnDieAnimationEnd()
    {
        Debug.Log("Die Animation Finished");

        // Freeze physics hoàn toàn
        rb.simulated = false;

        // Tắt collider
        if (col != null)
            col.enabled = false;

        // Ẩn player (hoặc gọi respawn sau này)
        gameObject.SetActive(false);
    }
}
