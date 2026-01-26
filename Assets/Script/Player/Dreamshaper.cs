using UnityEngine;
using UnityEngine.InputSystem;

public class Dreamshaper : MonoBehaviour
{
    // ================= MOVEMENT =================
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float deceleration = 30f;

    // ================= JUMP =================
    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.3f; // Tăng radius để dễ detect ground hơn

    // ================= RANGED SKILL =================
    [Header("Skill - Light Orb")]
    [SerializeField] private GameObject lightOrbPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireCooldown = 0.3f;
    [SerializeField] private int orbDamage = 15;
    private float fireTimer;

    // ================= MELEE ATTACK =================
    [Header("Melee Attack")]
    [SerializeField] private GameObject slashPrefab;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 2.0f;
    [SerializeField] private int meleeDamage = 10;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float meleeCooldown = 0.3f;
    private float meleeTimer;

    // ================= COMPONENTS =================
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator animator;
    private Collider2D playerCollider;

    // ================= INPUT =================
    private float moveInput;
    private bool isRunning;
    private float coyoteCounter;
    private float jumpBufferCounter;

    // ================= INIT =================
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
    }

    void Start()
    {
        // Debug: Kiểm tra vị trí spawn
        Debug.Log($"Dreamshaper spawn tại: {transform.position}");
    }

    // ================= UPDATE =================
    void Update()
    {
        ReadInput();
        GroundCheck();
        HandleJump();
        HandleFire();
        HandleMelee();
        FlipPlayer();
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    // ================= INPUT =================
    void ReadInput()
    {
        // Kiểm tra Input System có sẵn không
        if (Keyboard.current == null)
        {
            Debug.LogError("Input System chưa được khởi tạo! Vui lòng kiểm tra Project Settings > Player > Active Input Handling.");
            return;
        }

        moveInput = 0;

        if (Keyboard.current.aKey.isPressed) moveInput = -1;
        if (Keyboard.current.dKey.isPressed) moveInput = 1;

        isRunning = Keyboard.current.leftShiftKey.isPressed;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpBufferCounter = jumpBufferTime;
            Debug.Log($"Nhấn Space! jumpBufferCounter = {jumpBufferCounter}, coyoteCounter = {coyoteCounter}");
        }
    }

    // ================= MOVEMENT =================
    void MovePlayer()
    {
        float speed = isRunning ? runSpeed : walkSpeed;
        float targetVelocityX = moveInput * speed;
        float accel = Mathf.Abs(moveInput) > 0 ? acceleration : deceleration;

        float newVelX = Mathf.MoveTowards(rb.linearVelocity.x, targetVelocityX, accel * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(newVelX, rb.linearVelocity.y);
    }

    // ================= GROUND CHECK =================
    void GroundCheck()
    {
        // Kiểm tra null để tránh lỗi
        if (groundCheck == null)
        {
            Debug.LogError("GroundCheck Transform chưa được gán! Vui lòng kéo GroundCheck vào Inspector.");
            return;
        }

        // Tìm tất cả collider trong phạm vi, nhưng chỉ lấy những cái thuộc groundLayer
        Collider2D[] hits = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, groundLayer);
        
        // Loại trừ collider của chính nhân vật
        bool grounded = false;
        foreach (Collider2D hit in hits)
        {
            // Bỏ qua collider của chính nhân vật (kiểm tra cả gameObject và collider)
            if (hit != playerCollider && hit.gameObject != gameObject && hit.transform != transform)
            {
                grounded = true;
                break;
            }
        }

        // Debug log để kiểm tra (log mỗi 60 frame để không spam)
        if (Time.frameCount % 60 == 0)
        {
            Collider2D allHit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius);
            string hitInfo = allHit != null ? $"Hit: {allHit.name} (Layer: {allHit.gameObject.layer})" : "No hit";
            string groundHits = hits.Length > 0 ? string.Join(", ", System.Array.ConvertAll(hits, h => h.name)) : "None";
            Debug.Log($"GroundCheck: grounded={grounded}, position={groundCheck.position:F2}, radius={groundCheckRadius}, layerMask={groundLayer.value} (Layer 8), GroundHits=[{groundHits}], AllHit={hitInfo}");
        }

        if (grounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        jumpBufferCounter -= Time.deltaTime;
    }

    // Helper method để debug layer mask
    string ConvertLayerMaskToLayers(LayerMask mask)
    {
        if (mask.value == 0) return "Nothing";
        if (mask.value == -1) return "Everything";
        
        System.Collections.Generic.List<int> layers = new System.Collections.Generic.List<int>();
        for (int i = 0; i < 32; i++)
        {
            if ((mask.value & (1 << i)) != 0)
                layers.Add(i);
        }
        return string.Join(", ", layers);
    }

    // ================= JUMP =================
    void HandleJump()
    {
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D chưa được tìm thấy!");
            return;
        }

        if (jumpBufferCounter > 0 && coyoteCounter > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            Debug.Log($"Nhảy! Velocity Y = {rb.linearVelocity.y}");
            jumpBufferCounter = 0;
            coyoteCounter = 0;
        }

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasReleasedThisFrame && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }
    }

    // ================= RANGED ATTACK =================
    void HandleFire()
    {
        fireTimer -= Time.deltaTime;

        if (Keyboard.current.jKey.wasPressedThisFrame && fireTimer <= 0)
        {
            fireTimer = fireCooldown;
            animator.SetTrigger("CastOrb");
            ShootOrb();
        }
    }

    public void ShootOrb()
    {
        if (lightOrbPrefab == null) return;

        GameObject orb = Instantiate(lightOrbPrefab, firePoint.position, Quaternion.identity);
        LightOrb orbScript = orb.GetComponent<LightOrb>();

        if (orbScript != null)
        {
            Vector2 direction = sr.flipX ? Vector2.left : Vector2.right;
            orbScript.Launch(direction, transform, orbDamage);
        }
    }


    // ================= MELEE ATTACK =================
    void HandleMelee()
    {
        meleeTimer -= Time.deltaTime;

        if (Keyboard.current.hKey.wasPressedThisFrame && meleeTimer <= 0)
        {
            meleeTimer = meleeCooldown;

            animator.SetTrigger("MeleeAttack");
            SpawnSlash();
            DoMeleeDamage(); 
        }
    }

    void SpawnSlash()
    {
        if (slashPrefab == null || attackPoint == null) return;

        Vector3 pos = attackPoint.position;
        Quaternion rot = sr.flipX ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;

        Instantiate(slashPrefab, pos, rot);
    }

    void DoMeleeDamage()
    {
        Vector2 size = new Vector2(1.55f, 1.1f); 
        Vector2 center = attackPoint.position;

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f, enemyLayer);

        //foreach (Collider2D hit in hits)
        //{
        //    var enemy = hit.GetComponent<Enemy>();
        //    if (enemy != null)
        //    {
        //        enemy.TakeDamage(meleeDamage);
        //    }
        //}
    }

    // ================= DEBUG GIZMOS =================
    void OnDrawGizmosSelected()
    {
        if (!attackPoint) return;

        Gizmos.color = Color.red;
        Vector2 size = new Vector2(1.55f, 1.1f); 
        Gizmos.DrawWireCube(attackPoint.position, size);
    }


    // ================= FLIP =================
    bool facingLeft;

    void FlipPlayer()
    {
        if (moveInput < 0 && !facingLeft)
        {
            facingLeft = true;
            sr.flipX = true;
            FlipPoints();
        }
        else if (moveInput > 0 && facingLeft)
        {
            facingLeft = false;
            sr.flipX = false;
            FlipPoints();
        }
    }

    void FlipPoints()
    {
        if (attackPoint != null)
        {
            Vector3 pos = attackPoint.localPosition;
            pos.x *= -1;
            attackPoint.localPosition = pos;
        }

        if (firePoint != null)
        {
            Vector3 pos = firePoint.localPosition;
            pos.x *= -1;
            firePoint.localPosition = pos;
        }
    }



    // ================= ANIMATION =================
    void UpdateAnimation()
    {
        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        animator.SetBool("IsGrounded", coyoteCounter > 0);
        animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
    }
}
