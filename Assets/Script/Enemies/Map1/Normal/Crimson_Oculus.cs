using UnityEngine;

public class Crimson_Oculus : MonoBehaviour
{
    [Header("Patrol")]
    public float patrolDistance = 0.5f;
    public float patrolSpeed = 1.2f;

    [Header("Chase")]
    public float detectRange = 4f;
    public float chaseSpeed = 2.5f;

    [Header("Attack")]
    public float attackRange = 1.2f;
    public float attackCooldown = 1.5f;
    public int damage = 10;

    private Vector2 startPos;
    private bool movingRight = true;
    private float attackTimer;
    private float faceDelay = 0.15f;
    private float faceTimer;

    private Rigidbody2D rb;
    private Animator animator;
    private Transform player;
    private PlayerHealth playerHealth;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();
        }
        else
        {
            Debug.LogError("PLAYER NOT FOUND! Add Tag Player to Dreamshaper.");
        }

        startPos = transform.position;
    }


    void Update()
    {
        if (player == null || playerHealth == null) return;

        attackTimer -= Time.deltaTime;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackRange)
        {
            Attack();
        }
        else if (dist <= detectRange)
        {
            Chase();
        }
        else
        {
            Patrol();
        }
    }


    // ================= PATROL =================
    void Patrol()
    {
        animator.SetBool("IsMoving", true);

        if (movingRight)
        {
            rb.linearVelocity = new Vector2(patrolSpeed, rb.linearVelocity.y);
            if (transform.position.x >= startPos.x + patrolDistance)
                movingRight = false;
        }
        else
        {
            rb.linearVelocity = new Vector2(-patrolSpeed, rb.linearVelocity.y);
            if (transform.position.x <= startPos.x - patrolDistance)
                movingRight = true;
        }

        Flip();
    }

    // ================= CHASE =================
    void Chase()
    {
        animator.SetBool("IsMoving", true);

        float dir = player.position.x > transform.position.x ? 1 : -1;
        rb.linearVelocity = new Vector2(dir * chaseSpeed, rb.linearVelocity.y);

        movingRight = dir > 0;
        Flip();
    }

    // ================= ATTACK =================
    void Attack()
    {
        rb.linearVelocity = Vector2.zero;

        // Tính hướng player
        float dirToPlayer = Mathf.Sign(player.position.x - transform.position.x);

        // Nếu quái đang quay sai hướng → chỉ quay, chưa đánh
        if((movingRight ? 1 : -1) != dirToPlayer)
{
            faceTimer = faceDelay;
            movingRight = dirToPlayer > 0;
            Flip();
            return;
        }

        if (faceTimer > 0)
        {
            faceTimer -= Time.deltaTime;
            return;
        }

        // Nếu đúng hướng rồi → mới đánh
        if (attackTimer <= 0)
        {
            attackTimer = attackCooldown;
            animator.SetTrigger("Attack");
            DealDamage();
        }
    }


    // ================= DAMAGE PLAYER =================
    void DealDamage()
    {
        if (playerHealth == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= attackRange)
        {
            playerHealth.TakeDamage(damage);
        }
    }

    // ================= FLIP =================
    void Flip()
    {
        GetComponent<SpriteRenderer>().flipX = !movingRight;
    }

}
