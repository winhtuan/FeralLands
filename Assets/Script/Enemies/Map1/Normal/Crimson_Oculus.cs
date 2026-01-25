using UnityEngine;

public class CrimsonOculus : EnemyBase
{
    enum State { Patrol, Chase, Attack }
    State currentState;

    [Header("Movement")]
    public float patrolDistance = 0.5f;
    public float patrolSpeed = 1.2f;
    public float detectRange = 4f;
    public float chaseSpeed = 2.5f;

    [Header("Attack")]
    public float attackRange = 1.2f;
    public float attackCooldown = 1.5f;
    public int damage = 10;

    [Header("Death Effect")]
    public GameObject deathEffectPrefab;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;
    private Transform player;

    private Vector2 startPos;
    private bool movingRight = true;
    private float attackTimer;

    protected override void Awake()
    {
        base.Awake();

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;

        startPos = transform.position;
    }

    void Update()
    {
        if (!player) return;

        attackTimer -= Time.deltaTime;
        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackRange) currentState = State.Attack;
        else if (dist <= detectRange) currentState = State.Chase;
        else currentState = State.Patrol;

        switch (currentState)
        {
            case State.Patrol: Patrol(); break;
            case State.Chase: Chase(); break;
            case State.Attack: Attack(); break;
        }
    }

    // ================= PATROL =================
    void Patrol()
    {
        float target = movingRight ? patrolSpeed : -patrolSpeed;
        rb.linearVelocity = new Vector2(target, rb.linearVelocity.y);

        if (transform.position.x > startPos.x + patrolDistance) movingRight = false;
        if (transform.position.x < startPos.x - patrolDistance) movingRight = true;

        Flip();
    }

    // ================= CHASE =================
    void Chase()
    {
        movingRight = player.position.x > transform.position.x;
        rb.linearVelocity = new Vector2(movingRight ? chaseSpeed : -chaseSpeed, rb.linearVelocity.y);
        Flip();
    }

    // ================= ATTACK =================
    void Attack()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        movingRight = player.position.x > transform.position.x;
        Flip();

        if (attackTimer <= 0)
        {
            attackTimer = attackCooldown;
            animator.SetTrigger("Attack");
        }
    }

    // Animation Event
    public void DealDamage()
    {
        if (!player) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= attackRange)
        {
            player.GetComponent<PlayerHealth>()?.TakeDamage(damage);
        }
    }

    // ================= FLIP =================
    void Flip()
    {
        sr.flipX = !movingRight;
    }

    // ================= DEATH =================
    protected override void Die()
    {
        // Spawn effect ngay lập tức
        if (deathEffectPrefab != null)
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);

        // Play animation
        animator.SetTrigger("isDead");

        // Stop physics
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        // Disable collider để không va chạm
        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 1f);
    }

}
