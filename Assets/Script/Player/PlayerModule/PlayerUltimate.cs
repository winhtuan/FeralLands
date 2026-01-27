using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerUltimate : MonoBehaviour
{
    [Header("Ultimate Settings")]
    public float ultimateRadius = 1.2f;
    public int ultimateDamage = 50;
    public LayerMask enemyLayer;

    public float cooldown = 10f;
    public float timer;

    Animator animator;
    PlayerActionState action;
    CameraShake camShake;
    PlayerEnergy mana;

    void Awake()
    {
        animator = GetComponent<Animator>();
        action = GetComponent<PlayerActionState>();
        camShake = Camera.main.GetComponent<CameraShake>();
        mana = GetComponent<PlayerEnergy>();
    }

    void Update()
    {
        if (action.IsBusy) return;

        timer = Mathf.Max(0, timer - Time.deltaTime);

        if (Keyboard.current.uKey.wasPressedThisFrame && timer <= 0)
        {
            if (!mana.UseEnergy(40f)) return;

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
