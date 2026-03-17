using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    [HideInInspector] public float baseMoveSpeed;

    Rigidbody2D rb;
    SpriteRenderer sr;
    Animator animator;

    float moveInput;
    float facing = 1; 
    public float Facing => facing;

    void Awake()
    {
        baseMoveSpeed = moveSpeed;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    // TEMP DEBUG – xóa sau khi fix xong
    bool _debugLogged = false;
    void Update()
    {
        // Chỉ log 1 lần để không spam console
        if (!_debugLogged)
        {
            Debug.Log($"[PlayerMovement] Update đang chạy! moveInput={moveInput}");
            _debugLogged = true;
        }
        ReadInput();
        Flip();
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        Move();
    }

    void ReadInput()
    {
        moveInput = 0;
        
        // Safety check if Input System is not initialized correctly for Keyboard
        if (Keyboard.current == null) return;

        int scheme = PlayerPrefs.GetInt("ControlScheme", 0); // 0 = WASD, 1 = Arrows

        if (scheme == 0) // Chế độ WASD
        {
            if (Keyboard.current.aKey.isPressed) moveInput = -1f;
            if (Keyboard.current.dKey.isPressed) moveInput = 1f;
        }
        else // Chế độ Arrows
        {
            if (Keyboard.current.leftArrowKey.isPressed) moveInput = -1f;
            if (Keyboard.current.rightArrowKey.isPressed) moveInput = 1f;
        }
    }

    void Move()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    void Flip()
    {
        if (moveInput < 0 && facing != -1)
        {
            facing = -1;
            sr.flipX = true;
        }
        else if (moveInput > 0 && facing != 1)
        {
            facing = 1;
            sr.flipX = false;
        }
    }

    void UpdateAnimation()
    {
        float normalizedSpeed = Mathf.Abs(rb.linearVelocity.x) / moveSpeed;
        animator.SetFloat("Speed", normalizedSpeed);

        if (Mathf.Abs(moveInput) > 0)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayRunSFX();
        }
        else
        {
            if (AudioManager.Instance != null) AudioManager.Instance.StopRunSFX();
        }
    }
}
