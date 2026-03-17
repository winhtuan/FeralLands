using UnityEngine;

public class BossDropExp : MonoBehaviour
{
    [Header("Drop Settings")]
    public GameObject expOrbPrefab;
    public int dropAmount = 30;
    public float spreadRange = 2f;

    private BossHealth bossHealth;

    void Awake()
    {
        bossHealth = GetComponent<BossHealth>();
        
        if (bossHealth != null)
        {
            // Kết nối sự kiện OnDeath từ BossHealth (kế thừa từ EnemyBase)
            bossHealth.OnDeath += DropExp;
        }
    }

    void DropExp()
    {
        if (expOrbPrefab != null)
        {
            for (int i = 0; i < dropAmount; i++)
            {
                // Toả ngọc EXP ra xung quanh boss
                Vector2 randomOffset = Random.insideUnitCircle * spreadRange;
                Vector3 spawnPos = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);
                
                Instantiate(expOrbPrefab, spawnPos, Quaternion.identity);
            }
        }
        else
        {
            Debug.LogWarning("Chưa gán ExpOrbPrefab cho Boss!");
        }
    }

    void OnDestroy()
    {
        if (bossHealth != null)
        {
            bossHealth.OnDeath -= DropExp;
        }
    }
}
