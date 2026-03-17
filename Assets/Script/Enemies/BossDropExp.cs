using UnityEngine;

public class BossDropExp : MonoBehaviour
{
    [Header("Drop Settings")]
    public GameObject expOrbPrefab;
    public int dropAmount = 30;
    public float dropRadius = 2.0f;
    public float scatterForce = 5.0f;

    private EnemyBase bossBase;

    void Awake()
    {
        bossBase = GetComponent<EnemyBase>();
        
        if (bossBase != null)
        {
            // Kết nối sự kiện OnDeath từ EnemyBase (BossHealth kế thừa EnemyBase)
            bossBase.OnDeath += DropManyExp;
        }
    }

    void DropManyExp()
    {
        if (expOrbPrefab == null)
        {
            Debug.LogWarning("[BossDropExp] Chưa gán ExpOrbPrefab cho Boss!");
            return;
        }

        Debug.Log($"[BossDropExp] Boss chết! Đang rớt {dropAmount} cục EXP...");

        for (int i = 0; i < dropAmount; i++)
        {
            // Tạo vị trí ngẫu nhiên xung quanh boss trong một vòng tròn
            Vector2 randomCircle = Random.insideUnitCircle * dropRadius;
            Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, randomCircle.y + 0.5f, 0);

            GameObject orb = Instantiate(expOrbPrefab, spawnPos, Quaternion.identity);

            // Thử scatter bằng Force nếu có Rigidbody
            Rigidbody2D orbRb = orb.GetComponent<Rigidbody2D>();
            if (orbRb != null)
            {
                Vector2 scatterDir = new Vector2(Random.Range(-1f, 1f), Random.Range(0.5f, 1.5f)).normalized;
                orbRb.AddForce(scatterDir * scatterForce, ForceMode2D.Impulse);
            }
        }
    }

    void OnDestroy()
    {
        if (bossBase != null)
        {
            bossBase.OnDeath -= DropManyExp;
        }
    }
}
