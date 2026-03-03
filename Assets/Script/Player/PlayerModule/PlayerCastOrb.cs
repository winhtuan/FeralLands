using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCastOrb : MonoBehaviour
{
    public GameObject lightOrbPrefab;

    [Header("Orb Settings")]
    public float orbSpeed = 10f;
    public int orbDamage = 10;
    public float orbScale = 1.5f;

    [Header("Spawn Offset")]
    public Vector2 fireOffset = new Vector2(0.4f, -0.1f);

    public float fireCooldown = 0.5f;
    public float fireTimer;

    Animator animator;
    PlayerMovement movement;
    PlayerActionState action;
    PlayerEnergy mana;

    void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
        action = GetComponent<PlayerActionState>();
        mana = GetComponent<PlayerEnergy>();

    }

    void Update()
    {
        HandleCast();
    }

    void HandleCast()
    {
        // Luôn giảm thời gian hồi chiêu độc lập với action.IsBusy
        if (fireTimer > 0)
            fireTimer -= Time.deltaTime;

        if (Keyboard.current.jKey.wasPressedThisFrame && fireTimer <= 0)
        {
            Debug.Log($"[PlayerCastOrb] Pressed J. Mana: {mana.currentEnergy}");
            if (!mana.UseEnergy(15f))
            {
                Debug.Log("[PlayerCastOrb] Not enough mana!");
                return;
            }

            Debug.Log("[PlayerCastOrb] Casting Orb directly!");

            action.SetBusy(true);
            fireTimer = fireCooldown;
            animator.SetTrigger("CastOrb");

            // Gọi trực tiếp SpawnOrb và cởi trói IsBusy ngay để đảm bảo chắc chắn chạy
            SpawnOrb();
            Invoke(nameof(EndCast), 0.3f);
        }
    }


    float lastSpawnTime = -1f;

    // Animation Event hoặc được gọi trực tiếp từ code
    public void SpawnOrb()
    {
        Debug.Log($"[PlayerCastOrb] SpawnOrb called! lightOrbPrefab: {lightOrbPrefab != null}");
        
        // Tránh tình trạng bắn đúp do gọi 1 lần từ code và 1 lần từ Animation Event
        if (Time.time - lastSpawnTime < 0.2f) 
        {
            Debug.Log("[PlayerCastOrb] Blocked double-fire");
            return;
        }
        lastSpawnTime = Time.time;

        if (!lightOrbPrefab) 
        {
            Debug.LogError("[PlayerCastOrb] LỖI: lightOrbPrefab bị NULL! Chưa kéo prefab vào Inspector.");
            return;
        }

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
