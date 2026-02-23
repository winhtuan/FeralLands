using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarUI : MonoBehaviour
{
    [Header("UI References")]
    public Slider healthSlider;
    public BossHealth bossHealth;

    [Header("Optional")]
    public Text healthText; // Hiển thị số HP (optional)

    void Start()
    {
        if (bossHealth == null)
        {
            Debug.LogError("BossHealth is not assigned to BossHealthBarUI!");
            return;
        }

        if (healthSlider == null)
        {
            Debug.LogError("Health Slider is not assigned to BossHealthBarUI!");
            return;
        }

        // Subscribe to health change event
        bossHealth.OnHealthChanged += UpdateHealthBar;

        // Initialize health bar
        healthSlider.maxValue = bossHealth.GetMaxHP();
        healthSlider.value = bossHealth.GetCurrentHP();

        // Update text nếu có
        UpdateHealthText();
    }

    void OnDestroy()
    {
        // Unsubscribe để tránh memory leak
        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged -= UpdateHealthBar;
        }
    }

    void UpdateHealthBar(int currentHP, int maxHP)
    {
        if (healthSlider == null) return;

        healthSlider.maxValue = maxHP;
        healthSlider.value = currentHP;

        UpdateHealthText();
    }

    void UpdateHealthText()
    {
        if (healthText != null && bossHealth != null)
        {
            healthText.text = $"{bossHealth.GetCurrentHP()} / {bossHealth.GetMaxHP()}";
        }
    }
}
