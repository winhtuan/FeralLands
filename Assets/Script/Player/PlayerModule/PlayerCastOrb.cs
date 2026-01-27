using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCastOrb : MonoBehaviour
{
    public GameObject lightOrbPrefab;

    [Header("Orb Settings")]
    public float orbSpeed = 25f;
    public int orbDamage = 10;
    public float orbScale = 1.5f;

    [Header("Spawn Offset")]
    public Vector2 fireOffset = new Vector2(0.4f, -0.1f);

    public float fireCooldown = 0.5f;
    public float fireTimer;

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
        HandleCast();
    }

    void HandleCast()
    {
        if (action.IsBusy) return;

        if (fireTimer > 0)
            fireTimer -= Time.deltaTime;

        if (Keyboard.current.jKey.wasPressedThisFrame && fireTimer <= 0)
        {
            action.SetBusy(true);
            fireTimer = fireCooldown;
            animator.SetTrigger("CastOrb");
        }
    }

    // Animation Event
    public void SpawnOrb()
    {
        if (!lightOrbPrefab) return;

        float dir = movement.Facing;

        Vector2 spawnPos = (Vector2)transform.position +
                           new Vector2(fireOffset.x * dir, fireOffset.y);

        GameObject orbObj = Instantiate(lightOrbPrefab, spawnPos, Quaternion.identity);

        Vector3 scale = Vector3.one * orbScale;
        scale.x *= dir; // lật khi quay trái
        orbObj.transform.localScale = scale;


        LightOrb orb = orbObj.GetComponent<LightOrb>();
        if (orb == null) return;

        orb.speed = orbSpeed;
        orb.Launch(new Vector2(dir, 0), transform, orbDamage);
    }

    public void EndCast()
    {
        action.SetBusy(false);
    }

    void OnDrawGizmosSelected()
    {
        PlayerMovement m = GetComponent<PlayerMovement>();
        if (m == null) return;

        float dir = m != null ? m.Facing : 1;

        Vector2 pos = (Vector2)transform.position + new Vector2(fireOffset.x * dir, fireOffset.y);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(pos, 0.05f);
    }

    public float GetCooldownPercent()
    {
        return Mathf.Clamp01(fireTimer / fireCooldown);
    }

}
