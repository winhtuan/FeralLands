using UnityEngine;
using UnityEngine.InputSystem;

public class Dreamshaper : MonoBehaviour
{
    // ================= MOVEMENT =================
    [Header("Movement")]
    public float moveSpeed = 6f;

    // ================= JUMP =================
    [Header("Jump")]
    public float jumpForce = 12f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundRadius = 0.2f;

    // ================= RANGED ATTACK =================
    [Header("Ranged Attack")]
    public GameObject lightOrbPrefab;
    public Transform firePoint;
    public float orbSpeed = 12f;
    public float fireCooldown = 0.3f;
    public float fireRadiusDebug = 0.15f;
    public int orbDamage = 10;
    public float orbScale = 1.5f;
    float castTimer;
    float fireTimer;

    // ================= COMPONENTS =================
    Rigidbody2D rb;
    SpriteRenderer sr;
    Animator animator;

    // ================= STATE =================
    float moveInput;
    bool isGrounded;
    bool facingLeft;
    bool isCasting;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        ReadInput();
        CheckGround();
        Jump();
        HandleCastOrb();
        Flip();
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        Move();
    }

    // ================= INPUT =================
    void ReadInput()
    {
        moveInput = 0;
        if (Keyboard.current.aKey.isPressed) moveInput = -1;
        if (Keyboard.current.dKey.isPressed) moveInput = 1;
    }

    // ================= MOVE =================
    void Move()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    // ================= GROUND CHECK =================
    void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
    }

    // ================= JUMP =================
    void Jump()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded && !isCasting)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    // ================= FLIP =================
    void Flip()
    {
        if (moveInput < 0 && !facingLeft)
        {
            facingLeft = true;
            sr.flipX = true;
            FlipFirePoint();
        }
        else if (moveInput > 0 && facingLeft)
        {
            facingLeft = false;
            sr.flipX = false;
            FlipFirePoint();
        }
    }

    void FlipFirePoint()
    {
        if (!firePoint) return;
        Vector3 p = firePoint.localPosition;
        p.x *= -1;
        firePoint.localPosition = p;
    }

    // ================= CAST ORB (ANIMATION DRIVEN) =================
    void HandleCastOrb()
    {
        fireTimer -= Time.deltaTime;

        if (Keyboard.current.jKey.wasPressedThisFrame && fireTimer <= 0 && !isCasting)
        {
            fireTimer = fireCooldown;
            isCasting = true;
            castTimer = 0.6f; 
            animator.SetTrigger("CastOrb");
        }

        if (isCasting)
        {
            castTimer -= Time.deltaTime;
            if (castTimer <= 0)
                isCasting = false;
        }
    }

    public void SpawnOrb()
    {
        Debug.Log("SpawnOrb called");

        if (!lightOrbPrefab || !firePoint)
            return;

        GameObject orbObj = Instantiate(lightOrbPrefab, firePoint.position, Quaternion.identity);
        orbObj.transform.localScale = Vector3.one * orbScale;
        LightOrb orb = orbObj.GetComponent<LightOrb>();

        float dir = facingLeft ? -1 : 1;
        orb.Launch(new Vector2(dir, 0), transform, orbDamage);
    }

    public void EndCast()
    {
        Debug.Log("EndCast called");
        isCasting = false;
    }


    // ================= ANIMATION =================
    void UpdateAnimation()
    {
        animator.SetFloat("Speed", Mathf.Abs(moveInput));
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
    }

    // ================= DEBUG GIZMOS =================
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }

        if (firePoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(firePoint.position, fireRadiusDebug);
            Gizmos.DrawLine(transform.position, firePoint.position);
        }
    }
}
