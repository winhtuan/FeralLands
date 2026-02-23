using UnityEngine;

public class VaathBoss : MonoBehaviour
{
    enum State { Idle, Chase, Attack }
    State currentState;

    [Header("Detection & Range")]
    public float detectRange = 10f;
    public float attackRange = 1.5f;  

    [Header("Movement")]
    public float chaseSpeed = 3.5f;

    [Header("Attack")]
    public float attackCooldown = 2.0f;
    public int damage = 20;

    [Header("Attack Hitbox")]
    public GameObject attackHitboxPrefab;
    public Vector2 hitboxOffset = new Vector2(1.5f, 0f);
    public float hitboxLifetime = 0.2f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;
    private Transform player;

    private float attackTimer;
    private bool facingRight = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        // Tìm player trong scene
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            // Xác định ngay từ đầu boss nên nhìn trái hay phải
        bool shouldFaceRight = player.position.x > transform.position.x;
        facingRight = shouldFaceRight;

        // Đồng bộ lại scale X cho đúng hướng
        Vector3 scale = transform.localScale;
        if ((scale.x > 0f) != facingRight)
        {
            scale.x *= -1f;
            transform.localScale = scale;
        }
        }
        else
        {
            Debug.LogError("Player not found! Make sure Player has tag 'Player'");
        }
    }

    void Update()
    {
        if (player == null) return;

        attackTimer -= Time.deltaTime;

        // Tính khoảng cách đến player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // State machine logic
        if (distanceToPlayer <= attackRange)
        {
            currentState = State.Attack;
        }
        else if (distanceToPlayer <= detectRange)
        {
            currentState = State.Chase;
        }
        else
        {
            currentState = State.Idle;
        }

        // Execute behavior theo state
        switch (currentState)
        {
            case State.Idle:
                Idle();
                break;
            case State.Chase:
                Chase();
                break;
            case State.Attack:
                Attack();
                break;
        }
    }

    // ================= IDLE STATE =================
    void Idle()
    {
        // Dừng di chuyển
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // Set animation về idle/breathing
        animator.SetBool("isRunning", false);

        // Vẫn hướng về phía player
        UpdateFacing();
    }

    // ================= CHASE STATE =================
    void Chase()
    {
        // Di chuyển về phía player
        float direction = player.position.x > transform.position.x ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);

        // Set running animation
        animator.SetBool("isRunning", true);

        // Update hướng
        UpdateFacing();
    }

    // ================= ATTACK STATE =================
    void Attack()
    {
        // Dừng di chuyển
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // Không chạy
        animator.SetBool("isRunning", false);

        // Hướng về player
        UpdateFacing();

        // Thực hiện attack nếu cooldown hết
        if (attackTimer <= 0)
        {
            attackTimer = attackCooldown;
            animator.SetTrigger("Attack");
        }
    }

    // ================= FLIP LOGIC =================
    void UpdateFacing()
    {
        if (player == null) return;

        // Xác định hướng
        bool shouldFaceRight = player.position.x > transform.position.x;

        if (shouldFaceRight != facingRight)
        {
            facingRight = shouldFaceRight;
            Flip();
        }
    }

    void Flip()
    {
        // Flip sprite bằng cách đảo scale X
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // ================= ATTACK HITBOX =================
    // Animation Event - được gọi từ Attack animation tại frame vung tay
    public void SpawnAttackHitbox()
    {
        if (attackHitboxPrefab == null)
        {
            Debug.LogWarning("Attack Hitbox Prefab is not assigned!");
            DealDamageDirectly(); // Fallback: damage trực tiếp
            return;
        }

        // Tính position spawn hitbox
        float direction = facingRight ? 1f : -1f;
        Vector2 spawnPos = (Vector2)transform.position + new Vector2(hitboxOffset.x * direction, hitboxOffset.y);

        // Spawn hitbox
        GameObject hitbox = Instantiate(attackHitboxPrefab, spawnPos, Quaternion.identity, transform);
        
        // Initialize hitbox
        BossAttackHitbox hitboxScript = hitbox.GetComponent<BossAttackHitbox>();
        if (hitboxScript != null)
        {
            hitboxScript.Init(direction, damage, hitboxLifetime);
        }
    }

    // Fallback: Nếu không có hitbox prefab, damage trực tiếp
    void DealDamageDirectly()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= attackRange)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Boss dealt {damage} damage to player directly");
            }
        }
    }

    // ================= GIZMOS =================
    void OnDrawGizmosSelected()
    {
        // Vẽ detect range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        // Vẽ attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Vẽ hitbox spawn position
        if (Application.isPlaying)
        {
            float dir = facingRight ? 1f : -1f;
            Vector2 hitboxPos = (Vector2)transform.position + new Vector2(hitboxOffset.x * dir, hitboxOffset.y);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(hitboxPos, Vector2.one * 0.5f);
        }
    }
}
