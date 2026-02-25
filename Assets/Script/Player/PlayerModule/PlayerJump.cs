using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJump : MonoBehaviour
{
    public float jumpForce = 10f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundRadius = 0.2f;

    [Header("Audio")]
    public AudioClip jumpSound;
    public UnityEngine.Audio.AudioMixerGroup sfxMixerGroup;
    private AudioSource jumpAudioSource;

    Rigidbody2D rb;
    Animator animator;
    bool isGrounded;
    public bool IsGrounded => isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Setup AudioSource for Jump
        jumpAudioSource = gameObject.AddComponent<AudioSource>();
        if (jumpSound != null) jumpAudioSource.clip = jumpSound;
        if (sfxMixerGroup != null) jumpAudioSource.outputAudioMixerGroup = sfxMixerGroup;
        jumpAudioSource.playOnAwake = false;
        
        if (sfxMixerGroup == null)
            jumpAudioSource.volume = PlayerPrefs.GetFloat("Setting_SFX", 0.75f);
    }

    void Update()
    {
        CheckGround();
        Jump();
        UpdateAnimation();
    }

    void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
    }

    void Jump()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            // Play jump sound
            if (jumpSound != null)
            {
                jumpAudioSource.Play();
            }
        }
    }

    void UpdateAnimation()
    {
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
    }
}
