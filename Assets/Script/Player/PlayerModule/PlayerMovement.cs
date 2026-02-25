using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 3f;

    [Header("Audio")]
    public AudioClip footstepSound;
    public UnityEngine.Audio.AudioMixerGroup sfxMixerGroup; // Thêm biến lưu AudioMixerGroup
    private AudioSource footstepSource;
    private PlayerJump playerJump;

    Rigidbody2D rb;
    SpriteRenderer sr;
    Animator animator;

    float moveInput;
    float facing = 1; 
    public float Facing => facing;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        
        playerJump = GetComponent<PlayerJump>();
        
        // Setup AudioSource for footsteps
        footstepSource = gameObject.AddComponent<AudioSource>();
        if (footstepSound != null) footstepSource.clip = footstepSound;
        if (sfxMixerGroup != null) footstepSource.outputAudioMixerGroup = sfxMixerGroup; // Gắn MixerGroup
        
        footstepSource.loop = true;
        footstepSource.playOnAwake = false;
        
        // Cập nhật âm lượng ban đầu (nếu không có MixerGroup)
        if (sfxMixerGroup == null)
        {
            footstepSource.volume = PlayerPrefs.GetFloat("Setting_SFX", 0.75f);
        }
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
        HandleFootsteps();
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

        if (scheme == 0) // WASD
        {
            if (Keyboard.current.aKey.isPressed) moveInput = -1;
            if (Keyboard.current.dKey.isPressed) moveInput = 1;
        }
        else // Arrows
        {
            if (Keyboard.current.leftArrowKey.isPressed) moveInput = -1;
            if (Keyboard.current.rightArrowKey.isPressed) moveInput = 1;
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
    }

    void HandleFootsteps()
    {
        bool isMoving = Mathf.Abs(moveInput) > 0.1f;
        bool isGrounded = playerJump != null ? playerJump.IsGrounded : true;

        if (isMoving && isGrounded)
        {
            if (!footstepSource.isPlaying)
            {
                footstepSource.Play();
            }
        }
        else
        {
            if (footstepSource.isPlaying)
            {
                footstepSource.Stop();
            }
        }
    }
}
