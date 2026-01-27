using UnityEngine;
using UnityEngine.UI;

public class CooldownUI : MonoBehaviour
{
    public Image orbFill;
    public Image ultimateFill;

    public PlayerCastOrb orb;
    public PlayerUltimate ult;

    void Update()
    {
        if (orb != null)
            orbFill.fillAmount = orb.GetCooldownPercent();

        if (ult != null)
            ultimateFill.fillAmount = ult.GetCooldownPercent();
    }
}
