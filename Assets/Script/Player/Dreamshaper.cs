using UnityEngine;

public class Dreamshaper : MonoBehaviour
{
    [Header("Modules")]
    [HideInInspector] public PlayerMovement movement;
    [HideInInspector] public PlayerJump jump;
    [HideInInspector] public PlayerCastOrb castOrb;
    [HideInInspector] public PlayerMeleeAttack melee;

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        jump = GetComponent<PlayerJump>();
        castOrb = GetComponent<PlayerCastOrb>();
        melee = GetComponent<PlayerMeleeAttack>();
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
