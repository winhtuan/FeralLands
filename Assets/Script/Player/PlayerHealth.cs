using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D col;
    private Dreamshaper controller;
    private float invincibleTime = 1f;
    private float invincibleTimer;
    void Awake()
    {
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

    void Die()
    {
        Debug.Log("Player Dead!");

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
