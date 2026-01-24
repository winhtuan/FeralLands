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
    [SerializeField] private float groundCheckRadius = 0.25f;

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
        moveInput = 0;

        if (Keyboard.current.aKey.isPressed) moveInput = -1;
        if (Keyboard.current.dKey.isPressed) moveInput = 1;

        isRunning = Keyboard.current.leftShiftKey.isPressed;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpBufferCounter = jumpBufferTime;
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
        bool grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (grounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        jumpBufferCounter -= Time.deltaTime;
    }

    // ================= JUMP =================
    void HandleJump()
    {
        if (jumpBufferCounter > 0 && coyoteCounter > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0;
            coyoteCounter = 0;
        }

        if (Keyboard.current.spaceKey.wasReleasedThisFrame && rb.linearVelocity.y > 0)
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
