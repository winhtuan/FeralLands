using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerUltimate : MonoBehaviour
{
    [Header("Ultimate Settings")]
    public float ultimateRadius = 1.2f;
    public int ultimateDamage = 50;
    public LayerMask enemyLayer;

    [Header("Directional Lightning Settings")]
    public GameObject lightningPrefab;
    public int lightningCount = 5;
    public float lightningSpacing = 1.8f;   // khoảng cách giữa các tia
    public float lightningDelay = 1.5f;     // delay giữa từng tia

    public float cooldown = 10f;
    public float timer;

    Animator animator;
    PlayerActionState action;
    CameraShake camShake;
    PlayerEnergy mana;
    PlayerMovement movement;

    void Awake()
    {
        animator = GetComponent<Animator>();
        action = GetComponent<PlayerActionState>();
        camShake = Camera.main.GetComponent<CameraShake>();
        mana = GetComponent<PlayerEnergy>();
        movement = GetComponent<PlayerMovement>();
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

        // Damage trung tâm
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, ultimateRadius, enemyLayer);
        foreach (var hit in hits)
        {
            Debug.Log("Ultimate hit: " + hit.name);
        }

        // BẮN SÉT THEO HƯỚNG NHÂN VẬT
        StartCoroutine(SpawnDirectionalLightning());
    }

    System.Collections.IEnumerator SpawnDirectionalLightning()
    {
        float dir = movement.Facing; // 1 = phải, -1 = trái

        // chân nhân vật (hạ xuống một chút)
        Vector3 basePos = transform.position + Vector3.up * 0.7f;

        for (int i = 1; i <= lightningCount; i++)
        {
            Vector3 pos = basePos + Vector3.right * dir * lightningSpacing * i;

            Instantiate(lightningPrefab, pos, Quaternion.identity);

            yield return new WaitForSeconds(lightningDelay);
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
