using UnityEngine;

public class PlayerEnergy : MonoBehaviour
{
    public float maxEnergy = 250;
    public float currentEnergy;

    [Header("Regen Settings")]
    public float regenRate = 6f; 
    public float regenDelay = 1.5f;
    float regenTimer;

    void Awake()
    {
        currentEnergy = maxEnergy;
    }

    void Update()
    {
        regenTimer -= Time.deltaTime;

        if (regenTimer <= 0)
        {
            currentEnergy += regenRate * Time.deltaTime;
            currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
        }
    }

    public bool UseEnergy(float amount)
    {
        if (currentEnergy < amount) return false;

        currentEnergy -= amount;
        regenTimer = regenDelay;
        return true;
    }

    public void GainEnergy(float amount)
    {
        currentEnergy = Mathf.Clamp(currentEnergy + amount, 0, maxEnergy);
    }
}
