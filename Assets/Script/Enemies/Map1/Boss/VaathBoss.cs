using UnityEngine;

public class VaathBoss : MonoBehaviour
{
    enum State { Idle, Chase, Attack, RangedAttack }
    State currentState;

    [Header("Detection & Range")]
    public float detectRange = 50f; 
    public float attackRange = 2.0f; 
    public float meteorRange = 25f; 

    [Header("Movement")]
    public float chaseSpeed = 3.5f;

    [Header("Attack Settings")]
    public float attackCooldown = 1.2f;
    public int damage = 20;
    public float rangedCooldown = 5.0f; 

    [Header("Melee Attack Hitbox")]
    public GameObject attackHitboxPrefab;
    public Vector2 hitboxOffset = new Vector2(1.5f, 0f);
    public float hitboxLifetime = 0.2f;

    [Header("Meteor Attack")]
    public GameObject meteorPrefab;
    public int meteorCount = 3;
    public float meteorHorizontalOffset = 3f; 

    [Header("Screen Shake")]
    public float shakeIntensity = 0.5f;
    public float shakeDuration = 0.3f;

    [Header("Enrage Settings (HP < 50%)")]
    public float enragedAttackCooldown = 0.7f;
    public float enragedMeteorSpeed = 20f;
    public float enragedMeleeAnimSpeed = 1.5f;
    private bool isEnraged = false;
    private BossHealth bossHealth;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;
    private Transform player;

    private float attackTimer;
    private float rangedTimer;
    private bool facingRight = false;
    private bool isCasting = false;
    private bool isAttacking = false;
    public bool isBattleStarted = false;

    // Lưu vị trí gốc của camera để rung xong còn quay lại
    private Vector3 originalCamPos;
    private Transform mainCamera;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        bossHealth = GetComponent<BossHealth>();

        // Đảm bảo boss không tự ý đánh khi chưa hết banner intro
        isBattleStarted = false;

        if (Camera.main != null)
        {
            mainCamera = Camera.main.transform;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            bool shouldFaceRight = player.position.x > transform.position.x;
            facingRight = shouldFaceRight;

            Vector3 scale = transform.localScale;
            if ((scale.x > 0f) != facingRight)
            {
                scale.x *= -1f;
                transform.localScale = scale;
            }
        }

        // Đặt timer bằng 0 để tấn công thiên thạch ngay khi hết banner
        rangedTimer = 0f;
    }

    void Update()
    {
        if (player == null)
        {
            Debug.LogWarning($"[VaathBoss] Update: Player null!");
            return;
        }

        if (!isBattleStarted) return; 

        // Kiểm tra Enrage (Dưới 50% máu)
        CheckEnrage();

        if (isCasting || isAttacking) return;

        attackTimer -= Time.deltaTime;
        rangedTimer -= Time.deltaTime;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (rangedTimer <= 0)
        {
            currentState = State.RangedAttack;
        }
        else if (distanceToPlayer <= attackRange)
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

        switch (currentState)
        {
            case State.Idle: Idle(); break;
            case State.Chase: Chase(); break;
            case State.Attack: Attack(); break;
            case State.RangedAttack: StartRangedAttack(); break;
        }
    }

    void CheckEnrage()
    {
        if (isEnraged || bossHealth == null) return;

        float hpPercent = (float)bossHealth.GetCurrentHP() / bossHealth.GetMaxHP();
        if (hpPercent < 0.5f)
        {
            isEnraged = true;
            Debug.Log("<color=red>BOSS ENRAGED!</color>");
            
            // 🎨 HIỆU ỨNG THỊ GIÁC (Visual Effects)
            
            // 1. Đổi màu boss sang tông đỏ vĩnh viễn
            if (sr != null) sr.color = new Color(1f, 0.5f, 0.5f); 

            // 2. Rung màn hình mạnh một cái để báo hiệu
            TriggerScreenShake();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBossRoarSFX();
            }

            // 3. Kích hoạt Animator
            if (animator != null)
            {
                animator.SetTrigger("Enrage");
                animator.SetBool("isEnraged", true);
            }
        }
    }

    void Idle()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        animator.SetBool("isRunning", false);
        UpdateFacing();
    }

    void Chase()
    {
        float direction = player.position.x > transform.position.x ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);
        animator.SetBool("isRunning", true);
        UpdateFacing();
    }

    void Attack()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        animator.SetBool("isRunning", false);
        UpdateFacing();

        if (attackTimer <= 0)
        {
            isAttacking = true;
            
            // Buff tốc độ đánh khi Enraged
            float currentCooldown = isEnraged ? enragedAttackCooldown : attackCooldown;
            attackTimer = currentCooldown;

            if (isEnraged) animator.speed = enragedMeleeAnimSpeed;
            if (animator != null) animator.SetTrigger("Attack");

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBossNormalAttackSFX(); 
            }
            
            // Unlock hướng sau khi animation attack kết thúc nhanh hơn (tùy thuộc vào animation speed)
            float unlockTime = isEnraged ? (1.8f / enragedMeleeAnimSpeed) : 1.8f;
            Invoke(nameof(EndAttack), unlockTime);
        }
    }

    void EndAttack()
    {
        isAttacking = false;
        animator.speed = 1f; // Trả lại tốc độ bình thường
    }

    void StartRangedAttack()
    {
        isCasting = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        animator.SetBool("isRunning", false);
        UpdateFacing();

        // Tính toán timer cho lần sau dựa trên khoảng cách hiện tại
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            // Trong tầm đánh cận chiến: ngẫu nhiên 3-6s (nhanh hơn nếu enraged)
            float minT = isEnraged ? 2f : 3f;
            float maxT = isEnraged ? 4f : 6f;
            rangedTimer = Random.Range(minT, maxT);
        }
        else
        {
            // Ngoài tầm đánh cận chiến: cố định 3s (nhanh hơn nếu enraged)
            rangedTimer = isEnraged ? 2f : 3f;
        }
        
        animator.SetTrigger("Cast"); 
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBossFireSFX();
        }

        // Thời gian chờ Meteor rơi (nhanh hơn nếu enraged)
        float castDelay = isEnraged ? 0.8f : 1.5f;
        Invoke("PerformMeteorStrike", castDelay); 
    }

    public void PerformMeteorStrike()
    {
        if (player == null) return;

        // Số lượng thiên thạch ngẫu nhiên từ 5 đến 10
        int count = Random.Range(5, 11);

        // Vị trí xuất hiện: phía trên và hơi lùi về phía sau boss (tùy direction boss đang nhìn)
        float spawnOffsetX = facingRight ? -8f : 8f; 
        Vector2 spawnBasePos = new Vector2(transform.position.x + spawnOffsetX, transform.position.y + 15f);

        for (int i = 0; i < count; i++)
        {
            // Thêm một chút ngẫu nhiên cho vị trí spawn để không bị trùng khít
            Vector2 spawnPos = spawnBasePos + new Vector2(Random.Range(-2f, 2f), Random.Range(-1f, 1f));
            
            if (meteorPrefab != null)
            {
                GameObject meteor = Instantiate(meteorPrefab, spawnPos, Quaternion.identity);
                Meteor meteorScript = meteor.GetComponent<Meteor>();
                if (meteorScript != null)
                {
                    // Tăng tốc độ rơi nếu enraged
                    if (isEnraged) meteorScript.fallSpeed = enragedMeteorSpeed;

                    // Tính hướng bay về phía player (có thêm một chút xOffset để né được)
                    float xOffsetPerMeteor = (i - (count - 1) / 2f) * meteorHorizontalOffset;
                    Vector2 targetPos = new Vector2(player.position.x + xOffsetPerMeteor, player.position.y);
                    Vector2 direction = (targetPos - spawnPos).normalized;
                    
                    meteorScript.SetDirection(direction);
                }
            }
        }

        // Tạo hiệu ứng rung màn hình khi thiên thạch bắt đầu giáng xuống
        TriggerScreenShake();

        // Chờ kết thúc casting phù hợp với độ dài animation mới
        float endCastDelay = isEnraged ? 0.5f : 1.0f;
        Invoke("EndCasting", endCastDelay);
    }


    void TriggerScreenShake()
    {
        if (mainCamera == null) return;
        originalCamPos = mainCamera.position;
        InvokeRepeating("DoShake", 0, 0.05f);
        Invoke("StopShake", shakeDuration);
    }

    void DoShake()
    {
        if (mainCamera == null) return;
        float shakeX = Random.Range(-1f, 1f) * shakeIntensity;
        float shakeY = Random.Range(-1f, 1f) * shakeIntensity;
        mainCamera.position = originalCamPos + new Vector3(shakeX, shakeY, 0);
    }

    void StopShake()
    {
        CancelInvoke("DoShake");
        if (mainCamera != null)
            mainCamera.position = originalCamPos;
    }

    void EndCasting()
    {
        isCasting = false;
    }

    void UpdateFacing()
    {
        if (player == null || isAttacking || isCasting) return;
        bool shouldFaceRight = player.position.x > transform.position.x;
        if (shouldFaceRight != facingRight)
        {
            facingRight = shouldFaceRight;
            Flip();
        }
    }

    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public void SpawnAttackHitbox()
    {
        if (attackHitboxPrefab == null) return;
        
        float direction = facingRight ? 1f : -1f;
        
        // Tầm với thực tế của đòn đánh
        float reach = attackRange; 
        
        // Đặt tâm hitbox tại vị trí (reach / 2) tính từ Boss để nó bao phủ toàn bộ vùng từ 0 đến reach
        Vector2 spawnPos = (Vector2)transform.position + new Vector2((reach / 2f) * direction, hitboxOffset.y);
        
        // Tạo hitbox ở world space
        GameObject hitbox = Instantiate(attackHitboxPrefab, spawnPos, Quaternion.identity);
        
        // Điều chỉnh kích thước hitbox cho khớp với reach (giả sử BoxCollider2D mặc định size = 1)
        Vector3 newScale = hitbox.transform.localScale;
        newScale.x = reach; 
        newScale.y = 3.0f; // Tăng vùng quét theo chiều dọc để dễ trúng hơn khi Hero nhảy
        hitbox.transform.localScale = newScale;

        BossAttackHitbox hitboxScript = hitbox.GetComponent<BossAttackHitbox>();
        if (hitboxScript != null) hitboxScript.Init(direction, damage, hitboxLifetime);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, meteorRange);
    }
}
