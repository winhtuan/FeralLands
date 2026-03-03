using UnityEngine;
using UnityEngine.UI;

public class BossUIController : MonoBehaviour
{
    [Header("Boss Reference")]
    public BossHealth bossHealth;

    [Header("UI Components")]
    public Image bgImage;
    public Image fillImage;

    [Header("Health >= 50% (Blue)")]
    public Sprite blueBG;
    public Sprite blueFill;

    [Header("Health < 50% (Red)")]
    public Sprite redBG;
    public Sprite redFill;

    private bool isPhase2 = false;

    private void OnEnable()
    {
        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged += UpdateHealthBar;
            // Khởi tạo giá trị ban đầu và ép cập nhật Sprite
            isPhase2 = (float)bossHealth.GetCurrentHP() / bossHealth.GetMaxHP() < 0.5f;
            UpdateHealthBar(bossHealth.GetCurrentHP(), bossHealth.GetMaxHP());
        }
    }

    private void OnDisable()
    {
        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged -= UpdateHealthBar;
        }
    }

    private void UpdateHealthBar(int currentHP, int maxHP)
    {
        if (maxHP <= 0) return;

        float healthPercent = (float)currentHP / maxHP;
        
        // Cập nhật giá trị thanh fill
        if (fillImage != null)
        {
            fillImage.fillAmount = healthPercent;
        }

        // Kiểm tra ngưỡng 50% để đổi Sprite
        if (healthPercent >= 0.5f)
        {
            if (isPhase2 || bgImage.sprite != blueBG) // Chỉ đổi khi cần thiết
            {
                isPhase2 = false;
                ApplySpriteSet(blueBG, blueFill);
            }
        }
        else
        {
            if (!isPhase2 || bgImage.sprite != redBG)
            {
                isPhase2 = true;
                ApplySpriteSet(redBG, redFill);
            }
        }
    }

    private void ApplySpriteSet(Sprite bg, Sprite fill)
    {
        if (bgImage == null || fillImage == null) return;

        // Lưu lại thông số căn chỉnh (Left, Top, Right, Bottom) hiện tại
        RectTransform bgRect = bgImage.rectTransform;
        RectTransform fillRect = fillImage.rectTransform;

        Vector2 bgOffsetMin = bgRect.offsetMin;
        Vector2 bgOffsetMax = bgRect.offsetMax;
        Vector2 fillOffsetMin = fillRect.offsetMin;
        Vector2 fillOffsetMax = fillRect.offsetMax;

        // Thay đổi Sprite
        if (bg != null) bgImage.sprite = bg;
        if (fill != null) fillImage.sprite = fill;

        // Ép Unity cập nhật Layout ngay lập tức để tránh việc tự động reset sau khi đổi Sprite
        Canvas.ForceUpdateCanvases();

        // Ép Unity dùng lại thông số cũ
        bgRect.offsetMin = bgOffsetMin;
        bgRect.offsetMax = bgOffsetMax;
        fillRect.offsetMin = fillOffsetMin;
        fillRect.offsetMax = fillOffsetMax;
    }
}
