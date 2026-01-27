using UnityEngine;
using UnityEngine.UI;

public class EnergyBarUI : MonoBehaviour
{
    public Slider slider;
    public PlayerEnergy playerEnergy;

    void Start()
    {
        if (playerEnergy == null) return;

        slider.maxValue = playerEnergy.maxEnergy;
        slider.value = playerEnergy.currentEnergy;
    }

    void Update()
    {
        if (playerEnergy == null) return;
        slider.value = playerEnergy.currentEnergy;
    }
}
