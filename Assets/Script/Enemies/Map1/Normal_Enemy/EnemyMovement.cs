using UnityEngine;

public class EnemyMovement : NormalEnemyBase
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float moveDistance = 3f;

    [Header("Attack")]
    public Transform target;
    public float attackRange = 2.5f;
    public float detectRange = 5f;
    public float attackCooldown = 1.0f;

    private Vector3 startPos;
    private int direction = 1;

    private float attackTimer;
    private SpriteRenderer spriteRenderer;

    protected override void Awake()
    {
        base.Awake();
        startPos = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
    }


    void Update()
    {
        if (IsDead) return; // Nếu đã chết thì không làm gì cả

        if (target == null)
        {
            Patrol();
            return;
        }

        attackTimer -= Time.deltaTime;

        float distance = Vector2.Distance(transform.position, target.position);

        if (distance <= attackRange)
        {
            SetSpeed(0); // Dừng di chuyển
            FaceTarget(); // Xoay mặt về phía người chơi
            if (attackTimer <= 0f)
            {
                DoAttack();
            }
        }
        else if (distance <= detectRange)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    void ChasePlayer()
    {
        // Tính hướng di chuyển
        float moveDir = (target.position.x > transform.position.x) ? 1 : -1;

        // Di chuyển
        transform.Translate(Vector2.right * moveDir * moveSpeed * Time.deltaTime);

        // Cập nhật Animation & Flip
        SetSpeed(moveSpeed);
        spriteRenderer.flipX = moveDir < 0;
    }

    void FaceTarget()
    {
        if (target != null)
        {
            spriteRenderer.flipX = target.position.x < transform.position.x;
        }
    }

    void Patrol()
    {
        transform.Translate(Vector2.right * direction * moveSpeed * Time.deltaTime);

        float dist = Vector2.Distance(startPos, transform.position);
        if (dist >= moveDistance)
        {
            direction *= -1;
            startPos = transform.position;
        }

        SetSpeed(Mathf.Abs(direction));
        spriteRenderer.flipX = direction < 0;
    }

    void DoAttack()
    {
        attackTimer = attackCooldown; // khóa spam
        SetSpeed(0);
        Attack(); // Trigger Animator "Attack"
    }

    // Vẽ Gizmos để dễ dàng căn chỉnh trong Unity Editor
    void OnDrawGizmosSelected()
    {
        // Vẽ tầm phát hiện (Màu vàng)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}
