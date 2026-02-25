using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerUltimate : MonoBehaviour
{
    [Header("Ultimate Settings")]
    public float ultimateRadius = 1.2f;
    public int ultimateDamage = 50;
    public LayerMask enemyLayer;

    [Header("Audio")]
    public AudioClip ultimateSound;
    public UnityEngine.Audio.AudioMixerGroup sfxMixerGroup;
    private AudioSource ultimateAudioSource;

    public float cooldown = 10f;
    public float timer;

    Animator animator;
    PlayerActionState action;
    CameraShake camShake;
    void Awake()
    {
        animator = GetComponent<Animator>();
        action = GetComponent<PlayerActionState>();
        camShake = Camera.main.GetComponent<CameraShake>();

        // Setup AudioSource for Ultimate
        ultimateAudioSource = gameObject.AddComponent<AudioSource>();
        if (ultimateSound != null) ultimateAudioSource.clip = ultimateSound;
        if (sfxMixerGroup != null) ultimateAudioSource.outputAudioMixerGroup = sfxMixerGroup;
        ultimateAudioSource.playOnAwake = false;

        if (sfxMixerGroup == null)
            ultimateAudioSource.volume = PlayerPrefs.GetFloat("Setting_SFX", 0.75f);
    }

    void Update()
    {
        if (action.IsBusy) return;

        if (timer > 0)
            timer -= Time.deltaTime;

        if (Keyboard.current.uKey.wasPressedThisFrame && timer <= 0)
        {
            action.SetBusy(true);
            timer = cooldown;
            animator.SetTrigger("Ultimate");

            if (ultimateSound != null)
            {
                ultimateAudioSource.Play();
            }
        }
    }

    // Animation Event
    public void DoUltimate()
    {
        if (camShake != null)
            camShake.Shake(0.65f, 0.2f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, ultimateRadius, enemyLayer);

        foreach (var hit in hits)
        {
            Debug.Log("Ultimate hit: " + hit.name);

            // EnemyHealth hp = hit.GetComponent<EnemyHealth>();
            // if (hp != null) hp.TakeDamage(ultimateDamage);
        }
    }

    // Animation Event
    public void EndUltimate()
    {
        action.SetBusy(false);
    }

    public float GetCooldownPercent()
    {
        return Mathf.Clamp01(timer / cooldown);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, ultimateRadius);
    }

}
