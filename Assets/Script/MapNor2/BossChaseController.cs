using System.Collections;
using UnityEngine;

public class BossChaseController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f; // Tốc độ đuổi theo player (bằng với player moveSpeed)
    public float minDistance = 2f; // Khoảng cách tối thiểu giữa boss và player
    public float catchDistance = 0.5f; // Khoảng cách để boss bắt được player
    
    private bool isRunning = false;
    private Rigidbody2D rb;
    private Animator anim;
    private Transform player;
    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;

    [Header("Attack Settings")]
    public float attackInterval = 4f; // Cứ 4 giây xả chiêu 1 lần
    public GameObject obstaclePrefab; // Prefab tấn công (có thể là tia, đá, sóng phép...)
    public Transform attackPoint; // Điểm spawn tấn công
    public int minProjectiles = 1; // Số lượng tia tối thiểu
    public int maxProjectiles = 4; // Số lượng tia tối đa
    public float projectileSpacing = 1.5f; // Khoảng cách giữa các tia

    [Header("Collision Detection")]
    public Collider2D bossCollider; // Collider của boss để detect player
    
    private ChaseManager chaseManager;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        
        // Tìm player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerMovement = playerObj.GetComponent<PlayerMovement>();
            playerHealth = playerObj.GetComponent<PlayerHealth>();
        }
        
        // Nếu chưa có collider, tự động tìm
        if (bossCollider == null)
            bossCollider = GetComponent<Collider2D>();
        
        // Tìm ChaseManager
        chaseManager = FindObjectOfType<ChaseManager>();
    }

    void OnEnable()
    {
        Debug.Log("[BossChaseController] OnEnable được gọi");
        isRunning = true;
        if (anim != null) anim.SetBool("IsRunning", true);
        
        // Đảm bảo collider là trigger để detect player
        if (bossCollider != null)
            bossCollider.isTrigger = true;
        
        // Kiểm tra các reference trước khi bắt đầu coroutine
        if (obstaclePrefab == null)
        {
            Debug.LogError("[BossChaseController] obstaclePrefab chưa được gán trong Inspector!");
        }
        else
        {
            Debug.Log($"[BossChaseController] obstaclePrefab: {obstaclePrefab.name}");
            // Kiểm tra prefab có script di chuyển không
            ObstacleMover mover = obstaclePrefab.GetComponent<ObstacleMover>();
            if (mover == null)
            {
                Debug.LogWarning($"[BossChaseController] Prefab {obstaclePrefab.name} không có component ObstacleMover! Đạn sẽ không di chuyển.");
            }
            else
            {
                Debug.Log($"[BossChaseController] Prefab có ObstacleMover với speed: {mover.speed}");
            }
        }
        
        if (attackPoint == null)
        {
            Debug.LogError("[BossChaseController] attackPoint chưa được gán trong Inspector!");
        }
        else
        {
            Debug.Log($"[BossChaseController] attackPoint: {attackPoint.name} tại vị trí: {attackPoint.position}");
        }
        
        Debug.Log($"[BossChaseController] Attack Interval: {attackInterval} giây");
        Debug.Log($"[BossChaseController] Projectile Count: {minProjectiles}-{maxProjectiles}");
        
        // Bắt đầu coroutine tấn công
        StartCoroutine(AttackRoutine());
        Debug.Log("[BossChaseController] Đã bắt đầu AttackRoutine coroutine");
    }

    void Update()
    {
        if (!isRunning || player == null || rb == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Kiểm tra nếu player dừng lại hoặc chạy ngược (về phía boss)
        bool playerIsMovingRight = playerMovement != null && playerMovement.Facing > 0;
        bool playerIsMoving = Mathf.Abs(playerMovement != null ? playerMovement.Facing : 0) > 0.1f;
        
        // Nếu player dừng lại hoặc chạy ngược, boss sẽ tăng tốc để bắt
        if (!playerIsMoving || !playerIsMovingRight)
        {
            // Boss tăng tốc để bắt player
            float catchSpeed = moveSpeed * 1.5f;
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = new Vector2(direction.x * catchSpeed, rb.linearVelocity.y);
            
            // Nếu khoảng cách đủ gần, bắt player
            if (distanceToPlayer <= catchDistance)
            {
                KillPlayer();
                return;
            }
        }
        else
        {
            // Nếu player đang chạy, boss giữ khoảng cách nhất định
            if (distanceToPlayer > minDistance)
            {
                // Boss đuổi theo với tốc độ bằng player
                rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
            }
            else
            {
                // Giữ khoảng cách, không tiến gần hơn
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
    }

    // Xử lý khi chạm vào Player
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && isRunning)
        {
            KillPlayer();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && isRunning)
        {
            // Kiểm tra liên tục nếu player đang trong vùng boss
            float distance = Vector2.Distance(transform.position, collision.transform.position);
            if (distance <= catchDistance)
            {
                KillPlayer();
            }
        }
    }

    private void KillPlayer()
    {
        if (playerHealth == null) return;
        
        Debug.Log("Game Over! Boss caught the player.");
        
        // Set máu player về 0 để trigger Die()
        playerHealth.SetHealth(0);
        
        // Thông báo cho ChaseManager dừng chase
        if (chaseManager != null)
        {
            chaseManager.StopChase();
        }
        
        StopChasing();
        if (anim != null) anim.SetTrigger("Attack"); // Có thể play anim boss vồ lấy player
    }

    public void StopChasing()
    {
        isRunning = false;
        if (anim != null) anim.SetBool("IsRunning", false);
        StopAllCoroutines();
        
        if (rb != null)
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    // Vòng lặp bắn chiêu bắt player nhảy
    private IEnumerator AttackRoutine()
    {
        // Đợi một chút để đảm bảo boss đã được setup đầy đủ
        yield return new WaitForSeconds(0.5f);
        
        while (isRunning)
        {
            yield return new WaitForSeconds(attackInterval);
            
            // Kiểm tra điều kiện trước khi tấn công
            if (!isRunning)
            {
                Debug.Log("[BossChaseController] AttackRoutine: Boss không còn chạy, dừng tấn công");
                break;
            }
            
            if (obstaclePrefab == null)
            {
                Debug.LogWarning("[BossChaseController] AttackRoutine: obstaclePrefab chưa được gán!");
                continue;
            }
            
            if (attackPoint == null)
            {
                Debug.LogWarning("[BossChaseController] AttackRoutine: attackPoint chưa được gán!");
                continue;
            }
            
            Debug.Log($"[BossChaseController] Bắt đầu tấn công tại vị trí: {attackPoint.position}");
            
            // Play animation tấn công nếu có
            if (anim != null) 
            {
                anim.SetTrigger("SkillAttack");
                Debug.Log("[BossChaseController] Đã trigger animation SkillAttack");
            }
            
            // Random số lượng tia từ min đến max
            int projectileCount = Random.Range(minProjectiles, maxProjectiles + 1);
            Debug.Log($"[BossChaseController] Sẽ spawn {projectileCount} projectile(s)");
            
            // Spawn các tia tấn công
            for (int i = 0; i < projectileCount; i++)
            {
                Vector3 spawnPos = attackPoint.position;
                // Nếu có nhiều tia, phân bố theo chiều dọc
                if (projectileCount > 1)
                {
                    float offsetY = (i - (projectileCount - 1) / 2f) * projectileSpacing;
                    spawnPos.y += offsetY;
                }
                
                Debug.Log($"[BossChaseController] Spawning projectile {i + 1}/{projectileCount} tại vị trí: {spawnPos}");
                
                GameObject projectile = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
                
                if (projectile == null)
                {
                    Debug.LogError("[BossChaseController] Không thể instantiate projectile!");
                }
                else
                {
                    Debug.Log($"[BossChaseController] Đã spawn projectile: {projectile.name}");
                    
                    // Kiểm tra xem prefab có script di chuyển không
                    ObstacleMover mover = projectile.GetComponent<ObstacleMover>();
                    if (mover == null)
                    {
                        Debug.LogWarning($"[BossChaseController] Prefab {obstaclePrefab.name} không có component ObstacleMover! Đạn sẽ không di chuyển.");
                    }
                }
                
                // Delay nhỏ giữa các tia để tạo hiệu ứng
                if (i < projectileCount - 1)
                    yield return new WaitForSeconds(0.2f);
            }
        }
        
        Debug.Log("[BossChaseController] AttackRoutine đã kết thúc");
    }
}
