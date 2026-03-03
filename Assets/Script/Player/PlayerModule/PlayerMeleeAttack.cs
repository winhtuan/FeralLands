using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMeleeAttack : MonoBehaviour
{
    public GameObject meleeHitboxPrefab;

    [Header("Melee Settings")]
    public float attackCooldown = 0.4f;
    public int meleeDamage = 15;
    public float hitboxLifeTime = 0.15f;

    [Header("Hitbox Offset")]
    public Vector2 attackOffset = new Vector2(0.4f, -0.2f);
    public Vector2 hitboxSize = new Vector2(1.0f, 0.8f);

    float timer;
    bool isAttacking;

    Animator animator;
    PlayerMovement movement;
    PlayerActionState action;

    void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
        action = GetComponent<PlayerActionState>();
    }

    void Update()
    {
        HandleMelee();
    }

    void HandleMelee()
    {
        if (action.IsBusy) return;

        timer -= Time.deltaTime;

        if (Keyboard.current.hKey.wasPressedThisFrame && timer <= 0)
        {
            action.SetBusy(true);
            timer = attackCooldown;
            animator.SetTrigger("Melee");
            
            if (AudioManager.Instance != null) AudioManager.Instance.PlayMeleeSFX();
        }
    }

    // Animation Event
    public void SpawnHitbox()
    {
        if (!meleeHitboxPrefab) return;

        float dir = movement.Facing;

        Vector2 spawnPos = (Vector2)transform.position +
                           new Vector2(attackOffset.x * dir, attackOffset.y);

        GameObject hb = Instantiate(meleeHitboxPrefab, spawnPos, Quaternion.identity);
        hb.transform.parent = transform;

        MeleeHitbox hit = hb.GetComponent<MeleeHitbox>();
        if (hit != null)
            hit.Init(dir, meleeDamage, hitboxLifeTime);
    }

    public void EndMelee()
    {
        action.SetBusy(false);
    }

    void OnDrawGizmosSelected()
    {
        PlayerMovement m = GetComponent<PlayerMovement>();
        if (m == null) return;

        float dir = Application.isPlaying ? m.Facing : 1f;
        Vector2 center = (Vector2)transform.position + new Vector2(attackOffset.x * dir, attackOffset.y);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, hitboxSize);
    }
}
