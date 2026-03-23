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
    public int extraOrbCount = 0; // Số lượng orb bắn thêm theo lượt (nếu có)
    public bool isTripleOrbActive = false; // Flag cho chế độ 3 tia (Augment Lv8)

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
        if (fireTimer > 0)
            fireTimer -= Time.deltaTime;

        if (Keyboard.current.jKey.wasPressedThisFrame && fireTimer <= 0)
        {
            if (!mana.UseEnergy(15f))
                return;

            action.SetBusy(true);
            fireTimer = fireCooldown;
            animator.SetTrigger("CastOrb");

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayPlasmaOrbSFX();

            SpawnOrb();
            Invoke(nameof(EndCast), 0.3f);
        }
    }

    float lastSpawnTime = -1f;

    public void SpawnOrb()
    {
        if (Time.time - lastSpawnTime < 0.1f)
            return;
        lastSpawnTime = Time.time;

        if (!lightOrbPrefab)
            return;

        float dir = movement.Facing;

        // CHẾ ĐỘ 3 TIA (Triple Orb)
        if (isTripleOrbActive)
        {
            FireSingleOrb(new Vector2(dir, 0)); // Thẳng
            FireSingleOrb(new Vector2(dir, 0.4f)); // Xiên lên
            FireSingleOrb(new Vector2(dir, -0.4f)); // Xiên xuống
        }
        else
        {
            int totalOrbs = 1 + extraOrbCount;
            StartCoroutine(FireMultipleOrbs(totalOrbs));
        }
    }

    void FireSingleOrb(Vector2 launchDir)
    {
        Vector2 spawnPos =
            (Vector2)transform.position + new Vector2(fireOffset.x * movement.Facing, fireOffset.y);

        GameObject orbObj = Instantiate(lightOrbPrefab, spawnPos, Quaternion.identity);

        Vector3 scale = Vector3.one * orbScale;
        scale.x *= movement.Facing;
        orbObj.transform.localScale = scale;

        LightOrb orb = orbObj.GetComponent<LightOrb>();
        if (orb != null)
        {
            orb.speed = orbSpeed;
            orb.Launch(launchDir, transform, orbDamage);
        }
    }

    System.Collections.IEnumerator FireMultipleOrbs(int count)
    {
        float dir = movement.Facing;
        for (int i = 0; i < count; i++)
        {
            FireSingleOrb(new Vector2(dir, 0));
            if (count > 1)
                yield return new WaitForSeconds(0.08f);
        }
    }

    public void EndCast()
    {
        action.SetBusy(false);
    }

    public float GetCooldownPercent()
    {
        return Mathf.Clamp01(fireTimer / fireCooldown);
    }
}
