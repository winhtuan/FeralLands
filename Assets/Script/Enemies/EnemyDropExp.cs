using UnityEngine;

public class EnemyDropExp : MonoBehaviour
{
    [Header("Drop Settings")]
    public GameObject expOrbPrefab;
    
    // Tỉ lệ phần trăm, ví dụ 100 là 100% rớt
    [Range(0, 100)]
    public float dropChance = 100f; 

    private NormalEnemyBase enemyBase;

    void Awake()
    {
        enemyBase = GetComponent<NormalEnemyBase>();
        
        if (enemyBase != null)
        {
            // Kết nối sự kiện OnDeath từ NormalEnemyBase
            enemyBase.OnDeath += TryDropExp;
        }
    }

    void TryDropExp()
    {
        if (expOrbPrefab != null)
        {
            float randomValue = Random.Range(0f, 100f);
            if (randomValue <= dropChance)
            {
                // Sinh ngọc EXP tại vị trí quái chết
                Instantiate(expOrbPrefab, transform.position, Quaternion.identity);
            }
        }
        else
        {
            Debug.LogWarning("Chưa gán ExpOrbPrefab cho quái vật!");
        }
    }

    void OnDestroy()
    {
        // Gỡ kết nối sự kiện khi component bị hủy để tránh memory leak
        if (enemyBase != null)
        {
            enemyBase.OnDeath -= TryDropExp;
        }
    }
}
