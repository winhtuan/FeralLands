using UnityEngine;
using System.Collections;

/// <summary>
/// ĐIỀU KHIỂN BOSS CHÍNH - MAP 2 (DROGON & GRYM)
/// Hỗ trợ 2 Phase, 2 Kỹ năng mỗi Phase, Tự động Phase Transition tại 50% HP.
/// </summary>
public class Map2BossController : MonoBehaviour, IBossController
{
    enum State { Idle, Chase, Attack, Cast1, Cast2 }
    State currentState;

    [Header("Detection & Range")]
    public float detectRange = 50f;     // Tầm phát hiện
    public float attackRange = 3.5f;    // Tầm đánh cận chiến
    public float skillRange = 25f;      // Tầm dùng kỹ năng

    [Header("Movement")]
    public float chaseSpeed = 3.8f;

    [Header("Skill Settings")]
    public float skillCooldown = 5.0f;
    public GameObject effectP1_C1, effectP1_C2; // Prefabs chiêu Phase 1
    
    [Header("Refined Skills Prefabs")]
    public GameObject rootPrefab;         // P1 C1: Rễ cây
    public GameObject poisonZonePrefab;   // P1 C1: Vùng độc
    public GameObject laserPrefab;        // P1 C2: Laser
    public GameObject sporePrefab;        // P1 C2: Bào tử nổ
    public GameObject warningCircle;      // P2 C1/C2: Vòng cảnh báo
    public GameObject shockwavePrefab;    // P2 C1: Sóng chấn động
    public GameObject lightningPrefab;    // P2 C2: Sét

    [Header("Components")]
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;
    private Transform player;
    private BossHealth bossHealth;

    [Header("Timers & Status")]
    public bool initialFacingRight = true; // TÍCH CHỌN nếu Sprite gốc quay mặt sang PHẢI
    private int currentPhase = 1;
    private bool isTransitioning = false;
    private float attackTimer;
    private float skillTimer;
    private bool facingRight = false; // Phản ánh trạng thái thực tế
    private bool isActioning = false;
    private bool isInvulnerable = false;
    public bool isBattleStarted { get; set; } = false;

    [Header("Skill Scaling & FX")]
    public float rootScale = 2.0f; 
    public Transform mouthPos;           // Vị trí miệng để bắn laser
    public GameObject chargingEffectPrefab; // Hiệu ứng tụ lực

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        bossHealth = GetComponent<BossHealth>();
        
        facingRight = initialFacingRight;

        if (bossHealth == null) 
            Debug.LogError($"[Map2Boss] ❌ KHÔNG tìm thấy BossHealth trên {gameObject.name}!");
        else
            Debug.Log($"[Map2Boss] ✅ Đã kết nối BossHealth. HP: {bossHealth.GetCurrentHP()}/{bossHealth.GetMaxHP()}");

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
        skillTimer = skillCooldown;
    }

    void Update()
    {
        if (player == null || isTransitioning) return;

        // 1. KIỂM TRA HP HERO: Nếu Hero chết, Boss đứng im cười (không đánh nữa)
        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph != null && ph.currentHealth <= 0)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isRunning", false);
            StopAllCoroutines(); // Hủy các chiêu đang vận
            return;
        }

        // 2. KIỂM TRA HP BOSS: Nếu Boss chết, dừng mọi thứ để chờ Death Animation của BossHealth
        if (bossHealth != null && bossHealth.GetCurrentHP() <= 0)
        {
            rb.linearVelocity = Vector2.zero;
            StopAllCoroutines();
            return;
        }

        // Nếu trận đấu chưa bắt đầu (đang chạy Banner), vẫn luôn xoay mặt về Hero
        if (!isBattleStarted)
        {
            UpdateFacing();
            return;
        }

        CheckPhase();

        if (isActioning) return;

        attackTimer -= Time.deltaTime;
        skillTimer -= Time.deltaTime;

        float dist = Vector2.Distance(transform.position, player.position);

        // NẾU HERO QUÁ GẦN (Dưới 3.5m) -> ƯU TIÊN ĐÁNH CẬN CHIẾN TRƯỚC
        if (dist <= attackRange && attackTimer <= 0)
        {
            HandleAttack();
        }
        // NẾU KHÔNG THÌ MỚI KIỂM TRA DÙNG SKILL
        else if (skillTimer <= 0 && dist <= skillRange)
        {
            if (Random.value > 0.5f) ExecuteSkill(1);
            else ExecuteSkill(2);
        }
        else if (dist <= detectRange)
        {
            HandleChase();
        }
        else
        {
            HandleIdle();
        }
    }

    private void CheckPhase()
    {
        if (bossHealth == null) return;

        if (currentPhase == 1 && bossHealth.GetCurrentHP() <= bossHealth.GetMaxHP() * 0.5f)
        {
            Debug.Log("[Map2Boss] 🩸 HP dưới 50%! Đang chuyển Phase 2...");
            StartCoroutine(PhaseTransitionRoutine());
        }
    }

    private IEnumerator PhaseTransitionRoutine()
    {
        currentPhase = 2;
        isTransitioning = true;
        isActioning = true;
        rb.linearVelocity = Vector2.zero;
        if (animator != null)
        {
            animator.SetInteger("Phase", 2);
            animator.SetTrigger("PhaseTransition");
        }
        yield return new WaitForSeconds(2.0f);
        isTransitioning = false;
        isActioning = false;
    }

    private void HandleIdle()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        animator.SetBool("isRunning", false);
        UpdateFacing();
    }

    private void HandleChase()
    {
        float dir = player.position.x > transform.position.x ? 1 : -1;
        rb.linearVelocity = new Vector2(dir * chaseSpeed, rb.linearVelocity.y);
        animator.SetBool("isRunning", true);
        UpdateFacing();
    }

    private void UpdateFacing()
    {
        if (player == null) return;
        
        // Thêm khoảng đệm (deadzone) để tránh boss quay trái quay phải liên tục khi hero đứng quá gần tâm boss
        if (Mathf.Abs(player.position.x - transform.position.x) < 1.0f) return;

        bool playerRightOfBoss = player.position.x > transform.position.x;
        
        if (initialFacingRight)
        {
            if (playerRightOfBoss != facingRight) Flip();
        }
        else
        {
            if (playerRightOfBoss == facingRight) Flip();
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void HandleAttack()
    {
        if (isActioning) return; // Bảo vệ: không cho phép ghi đè khi đang đánh
        isActioning = true;
        StartCoroutine(BasicAttackRoutine());
    }

    private IEnumerator BasicAttackRoutine()
    {
        Debug.Log($"[Map2Boss] ⚔️ Đang tấn công cận chiến! Khoảng cách: {Vector2.Distance(transform.position, player.position)}");
        UpdateFacing(); 
        
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("isRunning", false);
        animator.SetTrigger("Attack");
        
        attackTimer = 1.8f;

        // Đợi animation đánh xong. Bạn hãy kiểm tra tên Animation đòn đánh trong Animator xem có đúng không.
        yield return new WaitForSeconds(1.2f); 
        isActioning = false;
    }

    private void ExecuteSkill(int skillNum)
    {
        isActioning = true;
        rb.linearVelocity = Vector2.zero;
        skillTimer = skillCooldown;
        animator.SetBool("isRunning", false);
        
        if (currentPhase == 1)
        {
            if (skillNum == 1) StartCoroutine(Skill_SeismicRootSlam());
            else StartCoroutine(Skill_NaturesRuinBreath());
        }
        else
        {
            if (skillNum == 1) StartCoroutine(Skill_AbyssalLeap());
            else StartCoroutine(Skill_CataclysmicRoar());
        }
    }

    public void EndAction() { isActioning = false; }

    // --- PHASE 1 SKILLS (REFINED) ---

    IEnumerator Skill_SeismicRootSlam()
    {
        UpdateFacing();
        animator.SetTrigger("Cast1");
        yield return new WaitForSeconds(0.8f);

        Vector3 targetPos = player.position;
        if (rootPrefab) 
        {
            GameObject roots = Instantiate(rootPrefab, targetPos, Quaternion.identity);
            // Nhân tỷ lệ gốc của Prefab với biến rootScale để linh hoạt hơn
            roots.transform.localScale = rootPrefab.transform.localScale * rootScale; 
        }
        
        yield return new WaitForSeconds(0.5f);
        if (poisonZonePrefab) 
        {
            GameObject poison = Instantiate(poisonZonePrefab, targetPos, Quaternion.identity);
            poison.transform.localScale = poisonZonePrefab.transform.localScale * rootScale; 
        }
        
        yield return new WaitForSeconds(0.5f);
        EndAction();
    }

    IEnumerator Skill_NaturesRuinBreath()
    {
        UpdateFacing();
        isActioning = true;
        animator.SetTrigger("Cast2"); 
        
        // 1. Xác định vị trí miệng
        Vector3 spawnPos = (mouthPos != null) ? mouthPos.position : transform.position + (facingRight ? new Vector3(1.5f, 2.5f, 0) : new Vector3(-1.5f, 2.5f, 0));
        
        // 2. Tụ lực (Charging)
        if (chargingEffectPrefab)
        {
            GameObject charge = Instantiate(chargingEffectPrefab, spawnPos, Quaternion.identity, transform);
            Destroy(charge, 0.8f);
        }
        
        yield return new WaitForSeconds(0.8f); // Chờ gồng mượt mà

        // 3. Bắn Laser
        if (laserPrefab && player != null)
        {
            if (mouthPos != null) spawnPos = mouthPos.position;

            GameObject laser = Instantiate(laserPrefab, spawnPos, Quaternion.identity);
            
            // TÍNH TOÁN HƯỚNG BẮN: Nhắm thẳng về phía Hero
            Vector2 targetDir = (Vector2)player.position - (Vector2)spawnPos;
            float angle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;
            
            // Nếu Sprite Laser của bạn mặc định là nằm dọc hoặc quay trái, ta cần cộng thêm 180 hoặc 90 độ
            // Ở đây tôi giả định Sprite Laser mặc định nằm ngang quay sang PHẢI
            laser.transform.rotation = Quaternion.Euler(0, 0, angle);
            
            // Kéo dài Laser để đảm bảo xuyên trúng Hero
            laser.transform.localScale = new Vector3(40f, 1.5f, 1f); 

            // 4. Tạo nổ bào tử tại điểm va chạm thực tế (Impact)
            yield return new WaitForSeconds(0.2f);
            
            // ImpactPos sẽ nằm trên đường thẳng từ miệng đến Hero
            Vector3 impactPos = spawnPos + (Vector3)targetDir.normalized * (targetDir.magnitude + 2f);
            if (sporePrefab)
            {
                for(int i = 0; i < 4; i++)
                {
                    Vector3 offset = new Vector3(Random.Range(-1f, 1f), Random.Range(-0.5f, 0.5f), 0);
                    Instantiate(sporePrefab, impactPos + offset, Quaternion.identity);
                }
            }
        }

        yield return new WaitForSeconds(0.5f);
        EndAction();
    }

    // --- PHASE 2 SKILLS (REFINED) ---

    IEnumerator Skill_AbyssalLeap()
    {
        UpdateFacing();
        animator.SetTrigger("Cast1"); 
        yield return new WaitForSeconds(0.5f);
        
        isInvulnerable = true;
        sr.enabled = false; // Biến mất
        
        yield return new WaitForSeconds(1.0f);
        
        Vector3 landPos = player.position;
        if (warningCircle) Instantiate(warningCircle, landPos, Quaternion.identity);
        
        yield return new WaitForSeconds(1.0f); // Thời gian người chơi thấy vòng đỏ để né
        
        // Lao xuống
        transform.position = landPos + Vector3.up * 15f; 
        sr.enabled = true;
        
        // Ở đây nên có logic làm Boss rơi nhanh xuống đất (Physics hoặc Lerp)
        float jumpElapsed = 0;
        Vector3 startJump = transform.position;
        while(jumpElapsed < 0.2f)
        {
            jumpElapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startJump, landPos, jumpElapsed / 0.2f);
            yield return null;
        }

        if (shockwavePrefab) 
        {
            GameObject sw = Instantiate(shockwavePrefab, landPos, Quaternion.identity);
            sw.transform.localScale = shockwavePrefab.transform.localScale * rootScale; // Dùng chung rootScale cho to
        }
        
        isInvulnerable = false;
        yield return new WaitForSeconds(0.5f);
        EndAction();
    }

    IEnumerator Skill_CataclysmicRoar()
    {
        UpdateFacing();
        isActioning = true; // Đảm bảo khóa hành động khi gầm
        animator.SetTrigger("Cast2"); // Animation Gầm
        
        yield return new WaitForSeconds(1.0f);
        
        // 1. Sét ngẫu nhiên (BỎ phần cảnh báo đỏ theo ý bạn)
        for (int i = 0; i < 4; i++)
        {
            // Spawn từ trên trời (cao hơn vị trí đất 10 đơn vị)
            Vector3 spawnPos = new Vector3(transform.position.x + Random.Range(-15f, 15f), player.position.y + 10f, 0);
            
            if (lightningPrefab) 
            {
                GameObject bolt = Instantiate(lightningPrefab, spawnPos, Quaternion.identity);
                // Kéo dài sét theo trục Y (20) và làm rộng bề ngang theo trục X (vắt ví dụ 5.0f hoặc 8.0f)
                bolt.transform.localScale = new Vector3(8.0f, 25f, 1f); 
            }
            
            yield return new WaitForSeconds(0.4f);
        }
        
        // 2. Sét nhắm trúng Player (BỎ cảnh báo đỏ, spawn từ trên cao)
        if (player != null)
        {
            Vector3 finalSpawnPos = new Vector3(player.position.x, player.position.y + 10f, 0);
            if (lightningPrefab) 
            {
                GameObject bolt = Instantiate(lightningPrefab, finalSpawnPos, Quaternion.identity);
                bolt.transform.localScale = new Vector3(8.0f, 25f, 1f);
            }
        }
        
        yield return new WaitForSeconds(0.8f);
        EndAction();
    }
}
