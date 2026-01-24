using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Slider slider;
    public PlayerHealth playerHealth;

    void Start()
    {
        if (playerHealth == null) return;

        slider.maxValue = playerHealth.maxHealth;
        slider.value = playerHealth.currentHealth;
    }

    void Update()
    {
        if (playerHealth == null) return;
        slider.value = playerHealth.currentHealth;
    }
}
