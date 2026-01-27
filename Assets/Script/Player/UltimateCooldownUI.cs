using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UltimateCooldownUI : MonoBehaviour
{
    public Image cooldownFill;          
    public Image skillIcon;            
    public TextMeshProUGUI cooldownText;

    public PlayerUltimate ultimate;

    void Update()
    {
        if (ultimate == null) return;

        float timer = ultimate.timer;
        float max = ultimate.cooldown;

        // % cooldown (0 -> 1)
        float percent = timer / max;
        cooldownFill.fillAmount = percent;

        if (timer > 0)
        {
            // Đang cooldown → xám
            skillIcon.color = Color.gray;

            // Hiện số giây còn lại
            cooldownText.text = Mathf.Ceil(timer).ToString();
        }
        else
        {
            // Sẵn sàng → sáng
            skillIcon.color = Color.white;
            cooldownText.text = "";
        }
    }
}
