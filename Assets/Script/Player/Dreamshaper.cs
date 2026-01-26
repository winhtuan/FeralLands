using UnityEngine;
using UnityEngine.InputSystem;

public class Dreamshaper : MonoBehaviour
{
    // ================= MOVEMENT =================
    [Header("Movement")]
    [SerializeField] float walkSpeed = 5f;
    [SerializeField] float runSpeed = 8f;
    [SerializeField] float acceleration = 25f;
    [SerializeField] float deceleration = 30f;

    // ================= JUMP =================
    [Header("Jump")]
    [SerializeField] float jumpForce = 12f;
    [SerializeField] float coyoteTime = 0.15f;
    [SerializeField] float jumpBufferTime = 0.15f;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius = 0.25f;

    // ================= RANGED =================
    [Header("Light Orb")]
    [SerializeField] GameObject lightOrbPrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] float fireCooldown = 0.3f;
    [SerializeField] int orbDamage = 15;
    float fireTimer;

    // ================= MELEE =================
    [Header("Melee")]
    [SerializeField] MeleeHitbox meleeHitbox;
    [SerializeField] GameObject slashPrefab;
    [SerializeField] float meleeCooldown = 0.3f;
    float meleeTimer;

    // ================= COMPONENTS =================
    Rigidbody2D rb;
    SpriteRenderer sr;
    Animator animator;

    // ================= INPUT =================
    float moveInput;
    bool isRunning;
    float coyoteCounter;
    float jumpBufferCounter;
    bool facingLeft;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

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
            jumpBufferCounter = jumpBufferTime;
    }

    // ================= MOVE =================
    void MovePlayer()
    {
        float speed = isRunning ? runSpeed : walkSpeed;
        float target = moveInput * speed;
        float accel = Mathf.Abs(moveInput) > 0 ? acceleration : deceleration;

        float newVelX = Mathf.MoveTowards(rb.linearVelocity.x, target, accel * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(newVelX, rb.linearVelocity.y);
    }

    // ================= GROUND =================
    void GroundCheck()
    {
        bool grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        coyoteCounter = grounded ? coyoteTime : coyoteCounter - Time.deltaTime;
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

    // ================= RANGED =================
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

    void ShootOrb()
    {
        if (!lightOrbPrefab || !firePoint) return;

        GameObject orb = Instantiate(lightOrbPrefab, firePoint.position, Quaternion.identity);
        orb.GetComponent<LightOrb>()?.Launch(facingLeft ? Vector2.left : Vector2.right, transform, orbDamage);
    }

    // ================= MELEE =================
    void HandleMelee()
    {
        meleeTimer -= Time.deltaTime;

        if (Keyboard.current.hKey.wasPressedThisFrame && meleeTimer <= 0)
        {
            meleeTimer = meleeCooldown;
            animator.SetTrigger("MeleeAttack");
            SpawnSlash();
        }
    }

    void SpawnSlash()
    {
        if (!slashPrefab || !meleeHitbox) return;

        Quaternion rot = sr.flipX ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
        Instantiate(slashPrefab, meleeHitbox.transform.position, rot);
    }

    // Animation Event
    public void EnableMeleeHitbox()
    {
        meleeHitbox.EnableHitbox();
    }

    public void DisableMeleeHitbox()
    {
        meleeHitbox.DisableHitbox();
    }

    // ================= FLIP =================
    void FlipPlayer()
    {
        if (moveInput < 0 && !facingLeft)
        {
            facingLeft = true;
            sr.flipX = true;
            FlipTransforms();
        }
        else if (moveInput > 0 && facingLeft)
        {
            facingLeft = false;
            sr.flipX = false;
            FlipTransforms();
        }
    }

    void FlipTransforms()
    {
        FlipTransform(firePoint);
        FlipTransform(meleeHitbox.transform);
    }

    void FlipTransform(Transform t)
    {
        if (!t) return;
        Vector3 p = t.localPosition;
        p.x *= -1;
        t.localPosition = p;
    }

    // ================= ANIMATION =================
    void UpdateAnimation()
    {
        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        animator.SetBool("IsGrounded", coyoteCounter > 0);
        animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
    }
}
