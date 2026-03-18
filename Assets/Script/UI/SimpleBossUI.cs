using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Thanh máu Boss đơn giản, dễ cài đặt cho Map 2.
/// </summary>
public class SimpleBossUI : MonoBehaviour
{
    [Header("References")]
    public BossHealth bossHealth;
    public Slider healthSlider;
    public TextMeshProUGUI bossNameText;

    [Header("Settings")]
    public string bossName = "GRYM - THE ANCIENT TIGER";
    public Color phase1Color = Color.green;
    public Color phase2Color = Color.red;

    private Image fillImage;

    void Awake()
    {
        if (healthSlider != null)
        {
            fillImage = healthSlider.fillRect.GetComponent<Image>();
        }
    }

    void OnEnable()
    {
        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged += UpdateUI;
        }
        
        if (bossNameText != null)
        {
            bossNameText.text = bossName;
        }
    }

    void OnDisable()
    {
        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged -= UpdateUI;
        }
    }

    void Start()
    {
        if (bossHealth != null)
        {
            UpdateUI(bossHealth.GetCurrentHP(), bossHealth.GetMaxHP());
        }
    }

    public void UpdateUI(int currentHP, int maxHP)
    {
        if (healthSlider == null) return;

        float ratio = (float)currentHP / maxHP;
        healthSlider.value = ratio;

        // Tự đổi màu theo Phase
        if (fillImage != null)
        {
            fillImage.color = (ratio > 0.5f) ? phase1Color : phase2Color;
        }
    }
}
