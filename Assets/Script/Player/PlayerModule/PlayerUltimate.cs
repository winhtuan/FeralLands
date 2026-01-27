using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerUltimate : MonoBehaviour
{
    [Header("Ultimate Settings")]
    public float ultimateRadius = 1.2f;
    public int ultimateDamage = 50;
    public float cooldown = 10f;
    public LayerMask enemyLayer;

    float timer;
    bool isUltimate;

    Animator animator;
    PlayerActionState action;
    CameraShake camShake;
    void Awake()
    {
        animator = GetComponent<Animator>();
        action = GetComponent<PlayerActionState>();
        camShake = Camera.main.GetComponent<CameraShake>();
    }

    void Update()
    {
        if (action.IsBusy) return;

        timer -= Time.deltaTime;

        if (Keyboard.current.uKey.wasPressedThisFrame && timer <= 0)
        {
            action.SetBusy(true);
            timer = cooldown;
            animator.SetTrigger("Ultimate");
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, ultimateRadius);
    }

}
