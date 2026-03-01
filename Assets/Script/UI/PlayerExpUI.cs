using UnityEngine;
using UnityEngine.UI;
using TMPro; // Thêm thư viện TextMeshPro

public class PlayerExpUI : MonoBehaviour
{
    [Header("UI References")]
    public Slider expSlider;
    
    [Header("Gắn LevelText vào MỘT trong hai ô dưới đây")]
    public Text legacyText; // Nơi kéo Text thường
    public TMP_Text tmpText; // Nơi kéo TextMeshPro

    [Header("Player Reference")]
    public PlayerLevel playerLevel;

    void Start()
    {
        // Thêm tham chiếu đến script PlayerLevel nếu chưa được kéo vào
        if (playerLevel == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerLevel = player.GetComponent<PlayerLevel>();
            }
            else
            {
                Debug.LogWarning("Không tìm thấy object nào có tag 'Player'. Hãy đảm bảo bạn đã set Tag!");
            }
        }

        if (playerLevel != null)
        {
            // Kết nối vào event
            playerLevel.OnExpChanged += UpdateExpBar;
            playerLevel.OnLevelUp += UpdateLevelText;

            // Chạy mặc định khi bắt đầu game
            UpdateLevelText(playerLevel.currentLevel);

            int maxExp = 0;
            if (playerLevel.currentLevel <= playerLevel.expToNextLevel.Length)
                maxExp = playerLevel.expToNextLevel[playerLevel.currentLevel - 1];

            UpdateExpBar(playerLevel.currentExp, maxExp);
        }
    }

    void UpdateExpBar(int currentExp, int maxExp)
    {
        if (expSlider != null && maxExp > 0)
        {
            expSlider.maxValue = maxExp;
            expSlider.value = currentExp;
        }
    }

    void UpdateLevelText(int newLevel)
    {
        if (legacyText != null)
        {
            legacyText.text = "LV: " + newLevel;
        }
        if (tmpText != null)
        {
            tmpText.text = "LV: " + newLevel;
        }
    }

    void OnDestroy()
    {
        if (playerLevel != null)
        {
            // Gỡ kết nối để trành lỗi memory leak
            playerLevel.OnExpChanged -= UpdateExpBar;
            playerLevel.OnLevelUp -= UpdateLevelText;
        }
    }
}
