#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro; 

public class SettingsSetupTool : MonoBehaviour
{
    // Đường dẫn ảnh icon
    const string ICON_PATH = "Assets/Prefabs/Settings/pngtree-pixel-art-setting-icon-design-vector-png-image_8528241.png";

    [ContextMenu("1. BUILD SETTINGS SYSTEM")]
    public void BuildCompleteSettings()
    {
        // BƯỚC 1: Tìm Canvas HealthBar
        GameObject parentCanvas = GameObject.Find("HealthBar");
        
        if (parentCanvas == null)
        {
            Canvas overlay = FindObjectOfType<Canvas>();
            if (overlay != null && overlay.renderMode == RenderMode.ScreenSpaceOverlay) 
            {
                parentCanvas = overlay.gameObject;
            }
            else 
            {
                GameObject newCanvas = new GameObject("HealthBar_Canvas");
                Canvas c = newCanvas.AddComponent<Canvas>();
                c.renderMode = RenderMode.ScreenSpaceOverlay;
                newCanvas.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                newCanvas.AddComponent<GraphicRaycaster>();
                parentCanvas = newCanvas;
                Debug.Log("Created new Canvas because HealthBar not found.");
            }
        }

        // BƯỚC 2: Xóa cái cũ đi trước
        Transform oldPanel = parentCanvas.transform.Find("SettingsPanel");
        if (oldPanel != null)
        {
            DestroyImmediate(oldPanel.gameObject);
        }

        GameObject panel = new GameObject("SettingsPanel");
        panel.transform.SetParent(parentCanvas.transform, false);
        
        Image img = panel.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.9f); 

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(500, 420); // Tăng chiều cao xíu cho rộng
        
        // Gắn script logic (Global namespace now)
        SettingsController controller = panel.AddComponent<SettingsController>();

        // BƯỚC 3: Tạo nội dung
        CreateText(panel.transform, "SETTINGS", 40, new Vector2(0, 160));

        CreateText(panel.transform, "MUSIC", 24, new Vector2(-120, 80));
        Slider musicS = CreateSlider(panel.transform, new Vector2(60, 80));
        controller.musicSlider = musicS;

        CreateText(panel.transform, "SFX", 24, new Vector2(-120, 20));
        Slider sfxS = CreateSlider(panel.transform, new Vector2(60, 20));
        controller.sfxSlider = sfxS;

        CreateText(panel.transform, "FULLSCREEN", 24, new Vector2(-100, -50));
        Toggle fullT = CreateToggle(panel.transform, new Vector2(50, -50));
        controller.fullscreenToggle = fullT;

        // --- BUTTON CHANGE CONTROLS ---
        Button ctrlBtn = CreateSimpleButton(panel.transform, "CHANGE", new Vector2(100, -120)); // Button bên Phải
        controller.controlSchemeButton = ctrlBtn;
        
        GameObject ctrlTextObj = new GameObject("Label_Controls");
        ctrlTextObj.transform.SetParent(panel.transform, false);
        TextMeshProUGUI ctrlText = ctrlTextObj.AddComponent<TextMeshProUGUI>();
        ctrlText.fontSize = 20;
        ctrlText.alignment = TextAlignmentOptions.Right;
        ctrlText.color = Color.yellow; 
        ctrlText.text = "CONTROLS: WASD"; 
        RectTransform ctrlRect = ctrlTextObj.GetComponent<RectTransform>();
        ctrlRect.anchoredPosition = new Vector2(-100, -120); // Text dời qua trái thêm để không đè nút
        ctrlRect.sizeDelta = new Vector2(250, 40);
        controller.controlSchemeText = ctrlText;

        // Button Close
        Button closeBtn = CreateSimpleButton(panel.transform, "CLOSE", new Vector2(0, -180));
        UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(closeBtn.onClick, panel.SetActive, false);


        // BƯỚC 4: Tạo Nút Bánh Răng
        // Check xóa nút cũ nếu có
        Transform oldGear = parentCanvas.transform.Find("Button_OpenSettings");
        if (oldGear != null) DestroyImmediate(oldGear.gameObject);

        GameObject gearBtnObj = new GameObject("Button_OpenSettings");
        gearBtnObj.transform.SetParent(parentCanvas.transform, false);

        RectTransform gearRect = gearBtnObj.AddComponent<RectTransform>();
        gearRect.anchorMin = Vector2.one; 
        gearRect.anchorMax = Vector2.one;
        gearRect.pivot = Vector2.one;
        gearRect.anchoredPosition = new Vector2(-20, -20);
        gearRect.sizeDelta = new Vector2(64, 64);

        Image gearImg = gearBtnObj.AddComponent<Image>();
        Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_PATH);
        if (icon != null) gearImg.sprite = icon;
        else gearImg.color = Color.gray;

        Button gearButton = gearBtnObj.AddComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(gearButton.onClick, panel.SetActive, true);

        // BƯỚC 5: Ẩn
        panel.SetActive(false);
        
        Debug.Log("HOÀN TẤT! Đã build Settings System mới nhất.");
    }

    // --- HELPER ---
    private void CreateText(Transform parent, string content, float size, Vector2 Pos)
    {
        GameObject t = new GameObject("Label_" + content);
        t.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = t.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        t.GetComponent<RectTransform>().anchoredPosition = Pos;
    }

    private Slider CreateSlider(Transform parent, Vector2 Pos)
    {
        GameObject root = DefaultControls.CreateSlider(new DefaultControls.Resources());
        root.transform.SetParent(parent, false);
        root.GetComponent<RectTransform>().anchoredPosition = Pos;
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 20);
        return root.GetComponent<Slider>();
    }

    private Toggle CreateToggle(Transform parent, Vector2 Pos)
    {
        GameObject root = DefaultControls.CreateToggle(new DefaultControls.Resources());
        root.transform.SetParent(parent, false);
        root.GetComponent<RectTransform>().anchoredPosition = Pos;
        Text lbl = root.GetComponentInChildren<Text>();
        if (lbl) lbl.text = ""; 
        return root.GetComponent<Toggle>();
    }

    private Button CreateSimpleButton(Transform parent, string label, Vector2 Pos)
    {
        GameObject root = DefaultControls.CreateButton(new DefaultControls.Resources());
        root.transform.SetParent(parent, false);
        root.GetComponent<RectTransform>().anchoredPosition = Pos;
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 35);
        
        Text t = root.GetComponentInChildren<Text>();
        if(t) { t.text = label; t.fontStyle = FontStyle.Bold; }
        return root.GetComponent<Button>();
    }
}
#endif
