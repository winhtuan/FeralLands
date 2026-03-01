using UnityEngine;
using System;

public class PlayerLevel : MonoBehaviour
{
    [Header("Level Info")]
    public int currentLevel = 1;
    public int currentExp = 0;

    [Header("Exp Required For Next Levels")]
    public int[] expToNextLevel = { 10, 25, 50, 100, 200, 400, 800, 1600, 3200, 6400 };

    // Events để UI có thể lắng nghe và cập nhật
    public event Action<int, int> OnExpChanged;
    public event Action<int> OnLevelUp;

    void Start()
    {
        // Khởi tạo UI lần đầu sau khi chớp nhoáng (delay nhỏ hoặc cập nhật tay)
        UpdateUI();
    }

    public void AddExp(int amount)
    {
        currentExp += amount;
        
        Debug.Log("Player gained " + amount + " EXP! Current EXP: " + currentExp);

        // Kiểm tra lên cấp (có thể lên nhiều cấp cùng lúc nếu nhặt exp to)
        while (currentLevel <= expToNextLevel.Length && currentExp >= GetMaxExpForCurrentLevel())
        {
            currentExp -= GetMaxExpForCurrentLevel();
            currentLevel++;
            
            Debug.Log("Player Leveled Up! New Level: " + currentLevel);
            OnLevelUp?.Invoke(currentLevel);
            
            // Ở đây bạn có thể gọi hàm tăng Stats cho nhân vật
            // ví dụ: GetComponent<PlayerStats>().IncreaseStatsForLevelUp();
        }

        UpdateUI();
    }

    private int GetMaxExpForCurrentLevel()
    {
        if (currentLevel <= expToNextLevel.Length)
        {
            return expToNextLevel[currentLevel - 1];
        }
        return 0; // Đã đạt cấp tối đa
    }

    private void UpdateUI()
    {
        OnExpChanged?.Invoke(currentExp, GetMaxExpForCurrentLevel());
    }
}
