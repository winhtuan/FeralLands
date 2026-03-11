using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// AugmentUI — Hiển thị panel chọn 3 Lõi với khung đẹp + icon.
/// Gắn lên AugmentPanel trong Canvas.
/// </summary>
public class AugmentUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel;          // Panel chứa toàn bộ UI chọn lõi
    public TMP_Text titleText;        // Tiêu đề "CHỌN LÕI"

    [Header("Card Frame — Kéo sprite khung lõi vào đây")]
    public Sprite cardFrameSprite;    // Sprite khung card (AugmentCardFrame)

    [Header("Button 1")]
    public Button btn1;
    public Image btn1Icon;            // Icon lõi 1
    public TMP_Text btn1Name;
    public TMP_Text btn1Desc;

    [Header("Button 2")]
    public Button btn2;
    public Image btn2Icon;            // Icon lõi 2
    public TMP_Text btn2Name;
    public TMP_Text btn2Desc;

    [Header("Button 3")]
    public Button btn3;
    public Image btn3Icon;            // Icon lõi 3
    public TMP_Text btn3Name;
    public TMP_Text btn3Desc;

    private Action<AugmentData> onSelectedCallback;
    private AugmentData[] currentAugments;

    /// <summary>
    /// Hiện UI chọn Lõi với 3 augment, truyền callback khi chọn xong.
    /// </summary>
    public void Show(AugmentData[] augments, Action<AugmentData> onSelected)
    {
        onSelectedCallback = onSelected;
        currentAugments = augments;

        // Set tiêu đề
        if (titleText != null)
            titleText.text = "⚡ CHỌN LÕI ⚡";

        // Gán data lên từng button
        SetButton(btn1, btn1Icon, btn1Name, btn1Desc, augments.Length > 0 ? augments[0] : null, 0);
        SetButton(btn2, btn2Icon, btn2Name, btn2Desc, augments.Length > 1 ? augments[1] : null, 1);
        SetButton(btn3, btn3Icon, btn3Name, btn3Desc, augments.Length > 2 ? augments[2] : null, 2);

        // Hiện panel
        if (panel != null)
            panel.SetActive(true);
    }

    void SetButton(Button btn, Image iconImg, TMP_Text nameText, TMP_Text descText, AugmentData data, int index)
    {
        if (btn == null) return;

        if (data == null)
        {
            btn.gameObject.SetActive(false);
            return;
        }

        btn.gameObject.SetActive(true);

        // Set khung card nếu có
        if (cardFrameSprite != null)
        {
            Image btnImage = btn.GetComponent<Image>();
            if (btnImage != null)
            {
                btnImage.sprite = cardFrameSprite;
                btnImage.type = Image.Type.Sliced;
            }
        }

        // Set icon
        if (iconImg != null && data.icon != null)
        {
            iconImg.sprite = data.icon;
            iconImg.enabled = true;
        }
        else if (iconImg != null)
        {
            iconImg.enabled = false; // Ẩn icon nếu chưa gán
        }

        if (nameText != null)
            nameText.text = data.augmentName;

        if (descText != null)
            descText.text = data.description;

        // Xóa listener cũ, thêm listener mới
        btn.onClick.RemoveAllListeners();
        int capturedIndex = index; // Capture cho closure
        btn.onClick.AddListener(() =>
        {
            OnButtonClicked(capturedIndex);
        });
    }

    void OnButtonClicked(int index)
    {
        if (currentAugments != null && index < currentAugments.Length)
        {
            onSelectedCallback?.Invoke(currentAugments[index]);
        }
    }

    /// <summary>
    /// Ẩn panel chọn Lõi
    /// </summary>
    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
    }
}
