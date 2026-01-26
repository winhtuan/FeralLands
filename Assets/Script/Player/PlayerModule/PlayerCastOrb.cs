using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCastOrb : MonoBehaviour
{
    public GameObject lightOrbPrefab;

    [Header("Orb Settings")]
    public float orbSpeed = 25f;
    public float fireCooldown = 0.3f;
    public int orbDamage = 10;
    public float orbScale = 1.5f;

    [Header("Spawn Offset")]
    public Vector2 fireOffset = new Vector2(0.4f, -0.1f);

    float fireTimer;
    bool isCasting;

    Animator animator;
    PlayerMovement movement;

    void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        HandleCast();
    }

    void HandleCast()
    {
        fireTimer -= Time.deltaTime;

        if (Keyboard.current.jKey.wasPressedThisFrame && fireTimer <= 0 && !isCasting)
        {
            fireTimer = fireCooldown;
            isCasting = true;
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
        orbObj.transform.localScale = Vector3.one * orbScale;

        LightOrb orb = orbObj.GetComponent<LightOrb>();
        if (orb == null) return;

        orb.speed = orbSpeed;
        orb.Launch(new Vector2(dir, 0), transform, orbDamage);
    }

    public void EndCast()
    {
        isCasting = false;
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

}
