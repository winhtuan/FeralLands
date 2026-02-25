using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrbCooldownUI : MonoBehaviour
{
    public Image cooldownFill;
    public Image skillIcon;
    public TextMeshProUGUI cooldownText;

    public PlayerCastOrb orb;

    void Update()
    {
        float timer = orb.fireTimer;
        float max = orb.fireCooldown;

        float percent = timer / max;
        cooldownFill.fillAmount = percent;

        if (timer > 0)
        {
            skillIcon.color = Color.gray;
            cooldownText.text = Mathf.Ceil(timer).ToString();
        }
        else
        {
            skillIcon.color = Color.white;
            cooldownText.text = "";
        }
    }
}
