using UnityEngine;

public class Dreamshaper : MonoBehaviour
{
    [Header("Modules")]
    [HideInInspector] public PlayerMovement movement;
    [HideInInspector] public PlayerJump jump;
    [HideInInspector] public PlayerCastOrb castOrb;
    [HideInInspector] public PlayerMeleeAttack melee;
    [HideInInspector] public PlayerUltimate ulti;
    [HideInInspector] public PlayerActionState action;

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        jump     = GetComponent<PlayerJump>();
        castOrb  = GetComponent<PlayerCastOrb>();
        melee    = GetComponent<PlayerMeleeAttack>();
        ulti     = GetComponent<PlayerUltimate>();
        action   = GetComponent<PlayerActionState>();
    }

    /// <summary>
    /// Khoá (false) hoặc mở khoá (true) toàn bộ module điều khiển nhân vật.
    /// </summary>
    public void SetAllModulesEnabled(bool state)
    {
        // Fallback: lấy lại component nếu null (phòng trường hợp Awake chưa chạy đúng lúc)
        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (jump     == null) jump     = GetComponent<PlayerJump>();
        if (castOrb  == null) castOrb  = GetComponent<PlayerCastOrb>();
        if (melee    == null) melee    = GetComponent<PlayerMeleeAttack>();
        if (ulti     == null) ulti     = GetComponent<PlayerUltimate>();

        Debug.Log($"[Dreamshaper] SetAllModulesEnabled({state}) | " +
                  $"movement={movement != null} | jump={jump != null} | " +
                  $"castOrb={castOrb != null} | melee={melee != null} | ulti={ulti != null}");

        if (movement != null) movement.enabled = state;
        if (jump     != null) jump.enabled     = state;
        if (castOrb  != null) castOrb.enabled  = state;
        if (melee    != null) melee.enabled     = state;
        if (ulti     != null) ulti.enabled      = state;

        // Xác nhận trạng thái sau khi set
        if (movement != null)
            Debug.Log($"[Dreamshaper] Sau khi set: movement.enabled = {movement.enabled}");
    }

    void OnDrawGizmosSelected()
    {
        if (jump == null) jump = GetComponent<PlayerJump>();

        if (jump != null && jump.groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(jump.groundCheck.position, jump.groundRadius);
        }
    }
}
