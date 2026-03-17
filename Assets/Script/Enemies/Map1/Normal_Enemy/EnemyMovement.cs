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

    [Header("Attack Type")]
    public bool isRanged = false;               // true = bắn đạn, false = đánh gần
    public GameObject projectilePrefab;          // kéo prefab đạn vào đây
    public Transform projectileSpawnPoint;       // vị trí bắn đạn (thường là tầm tay ra gàn nhân vật)

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
        attackTimer = attackCooldown;
        SetSpeed(0);
        Attack(); // Trigger Animator "Attack"

        if (AudioManager.Instance != null)
        {
            if (isRanged) AudioManager.Instance.PlayMonsterRangedAttackSFX();
            else AudioManager.Instance.PlayMonsterNormalAttackSFX();
        }

        if (isRanged)
        {
            ShootProjectile(); // Bắn đạn ngay lập tức
        }
    }

    // Được gọi từ Animation Event trên Animator khi đến frame bắn
    public void ShootProjectile()
    {
        Debug.Log($"[SHOOT] ShootProjectile được gọi. isRanged={isRanged}, prefab={projectilePrefab}");

        if (!isRanged || projectilePrefab == null)
        {
            Debug.LogWarning("[SHOOT] Dừng lại: isRanged=false HOẶC projectilePrefab chưa được gán!");
            return;
        }

        Transform spawnPoint = projectileSpawnPoint != null ? projectileSpawnPoint : transform;

        // Tính hướng bắn thẳng về phía Player (bao gồm cả trục Y - bắn chéo xuống được)
        Vector2 dir = Vector2.right; // Mặc định bắn sang phải
        if (target != null)
        {
            // Offset Y âm vì pivot Player ở đầu, cần nhắm xuống giữa thân
            Vector2 aimPoint = (Vector2)target.position + new Vector2(0, -0.5f);
            dir = (aimPoint - (Vector2)spawnPoint.position).normalized;
        }

        GameObject bullet = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);
        Debug.Log($"[SHOOT] Đạn đã tạo ra tại {spawnPoint.position}, hướng={dir}");

        EnemyProjectile proj = bullet.GetComponent<EnemyProjectile>();
        if (proj != null)
        {
            proj.Init(dir, this);
        }
    }

    // Vẽ Gizmos để dễ dàng căn chỉnh trong Unity Editor
    void OnDrawGizmosSelected()
    {
        // Vẽ tầm phát hiện (Màu vàng)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}
