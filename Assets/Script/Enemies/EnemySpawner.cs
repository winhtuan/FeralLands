using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SpawnWave
{
    public string waveName = "Đợt quái";
    public GameObject enemyPrefab;      // Loại quái cho đợt này
    public int count = 3;               // Số lượng
    [Header("Khu vực xuất hiện")]
    public float minX;
    public float maxX;
    public float spawnY;                // Độ cao (Chỉnh cao lên cho quái đứng trên bệ)
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Cấu hình các đợt quái")]
    public List<SpawnWave> waves;       // Danh sách các đợt quái

    [Header("Tùy chọn")]
    public bool spawnOnEnable = true;   

    private List<GameObject> spawnedEnemies = new List<GameObject>();

    void OnEnable()
    {
        if (spawnOnEnable)
        {
            SpawnAllWaves();
        }
    }

    public void SpawnAllWaves()
    {
        ClearEnemies();

        if (waves == null) return;

        foreach (SpawnWave wave in waves)
        {
            if (wave.enemyPrefab == null) continue;

            // Tính toán độ rộng của mỗi "ô" để rải quái cho đều
            float totalWidth = wave.maxX - wave.minX;
            float slotWidth = totalWidth / Mathf.Max(1, wave.count);

            for (int i = 0; i < wave.count; i++)
            {
                // Chia phạm vi thành i ô, mỗi ô chứa 1 con quái
                float slotMinX = wave.minX + (i * slotWidth);
                float slotMaxX = slotMinX + slotWidth;

                // Lấy vị trí ngẫu nhiên nhưng chỉ TRONG PHẠM VI CỦA Ô ĐÓ thôi
                float randomX = Random.Range(slotMinX, slotMaxX);
                Vector3 spawnPos = new Vector3(randomX, wave.spawnY, 0);

                GameObject enemy = Instantiate(wave.enemyPrefab, spawnPos, Quaternion.identity);
                enemy.transform.SetParent(this.transform);
                spawnedEnemies.Add(enemy);
            }
        }
        Debug.Log("[Spawner] Đã tạo xong tất cả các đợt quái và rải đều khoảng cách.");
    }

    public void ClearEnemies()
    {
        foreach (GameObject e in spawnedEnemies)
        {
            if (e != null) Destroy(e);
        }
        spawnedEnemies.Clear();
    }

    // Vẽ nhiều màu khác nhau để bạn phân biệt các đợt trong Scene
    void OnDrawGizmosSelected()
    {
        if (waves == null) return;
        
        for (int i = 0; i < waves.Count; i++)
        {
            // Mỗi đợt một màu cho dễ nhìn
            Gizmos.color = Color.HSVToRGB((float)i / waves.Count, 1, 1);
            Vector3 start = new Vector3(waves[i].minX, waves[i].spawnY, 0);
            Vector3 end = new Vector3(waves[i].maxX, waves[i].spawnY, 0);
            Gizmos.DrawLine(start, end);
            Gizmos.DrawWireCube(new Vector3((waves[i].minX + waves[i].maxX)/2, waves[i].spawnY, 0), new Vector3(waves[i].maxX - waves[i].minX, 0.2f, 0));
        }
    }
}
