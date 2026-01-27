#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

namespace FeralLands.Tools
{
    public class SettingsUIBuilder : MonoBehaviour
    {
        [ContextMenu("Force Button Into HealthBar")]
        public void BuildUI()
        {
            // 1. TIM CAI CANVAS HEALTHBAR (QUAN TRONG NHAT)
            GameObject healthBarCanvas = GameObject.Find("HealthBar");
            if (healthBarCanvas == null)
            {
                Debug.LogError("KHONG TIM THAY 'HealthBar' TRONG SCENE! HAY MO SCENE CHUA HEALTHBAR LEN.");
                return;
            }

            Debug.Log("Tim thay HealthBar! Dang gan nut vao...");

            // 2. TAO NUT MO SETTING (NGANG HANG VOI THANH MAU)
            GameObject openBtnObj = new GameObject("Nut_Mo_Setting_Moi");
            openBtnObj.transform.SetParent(healthBarCanvas.transform, false); 
            
            // Xoa cac component cu neu co de reset
            // Setup RectTransform de bam chat vao GOC TREN PHAI
            RectTransform openBtnRect = openBtnObj.AddComponent<RectTransform>();
            openBtnRect.anchorMin = new Vector2(1, 1); // Neo vao goc phai tren
            openBtnRect.anchorMax = new Vector2(1, 1);
            openBtnRect.pivot = new Vector2(1, 1);
            openBtnRect.anchoredPosition = new Vector2(-20, -20); // Cach le 20 don vi
            openBtnRect.sizeDelta = new Vector2(60, 60); // Kich thuoc icon

            // Them Hinh Anh
            Image openBtnImg = openBtnObj.AddComponent<Image>();
            string iconPath = "Assets/Prefabs/Settings/pngtree-pixel-art-setting-icon-design-vector-png-image_8528241.png";
            Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            if (iconSprite != null) openBtnImg.sprite = iconSprite;
            else openBtnImg.color = Color.red; // Mau do neu khong thay anh

            // Them Nut Bam
            Button openBtn = openBtnObj.AddComponent<Button>();

            // 3. TIM CAI BANG SETTING DE GAN SU KIEN
            // Tim object ten la "SettingsPanel" (neu ban da tao truoc do)
            Transform existingPanel = healthBarCanvas.transform.Find("SettingsPanel");
            if (existingPanel != null)
            {
                Debug.Log("Da tim thay SettingsPanel cu. Dang ket noi nut bam...");
                UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(openBtn.onClick, existingPanel.gameObject.SetActive, true);
            }
            else
            {
                Debug.LogWarning("Chua thay SettingsPanel trong HealthBar. Tao nut xong nhung chua gan su kien mo bang.");
                Debug.Log("HAY KEO 'SettingsPanel' VAO TRONG 'HealthBar' NGAY LAP TUC.");
            }

            // Select luon cai nut vua tao de ban thay
            Selection.activeGameObject = openBtnObj;
            Debug.Log("DA TAO NUT 'Nut_Mo_Setting_Moi' TRONG HEALTHBAR! NO SE DI THEO CAMERA.");
        }

        private void CreateText(GameObject parent, string content, Vector2 position, float fontSize)
        {
            GameObject textObj = new GameObject("Label_" + content);
            textObj.transform.SetParent(parent.transform, false);
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            
            RectTransform rect = textObj.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(200, 50);
        }

        private Slider CreateSlider(GameObject parent, Vector2 position)
        {
            GameObject sliderObj = new GameObject("Slider");
            sliderObj.transform.SetParent(parent.transform, false);
            Slider slider = sliderObj.AddComponent<Slider>();
            
            RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
            sliderRect.anchoredPosition = position;
            sliderRect.sizeDelta = new Vector2(150, 20);

            // Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(sliderObj.transform, false);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = Color.gray;
            RectTransform bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0.25f); bgRect.anchorMax = new Vector2(1, 0.75f);
            bgRect.offsetMin = Vector2.zero; bgRect.offsetMax = Vector2.zero;
            slider.targetGraphic = bgImg;

            // Fill Area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0, 0.25f); fillAreaRect.anchorMax = new Vector2(1, 0.75f);
            fillAreaRect.offsetMin = new Vector2(5, 0); fillAreaRect.offsetMax = new Vector2(-5, 0);

            // Fill
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = Color.green;
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.offsetMin = Vector2.zero; fillRect.offsetMax = Vector2.zero;
            slider.fillRect = fillRect;

            // Handle Area (Simplification: No handle for now to avoid complexity, user drags bar)
            // Or add simple handle
            GameObject handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(sliderObj.transform, false);
            RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero; handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(10, 0); handleAreaRect.offsetMax = new Vector2(-10, 0);

            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            Image handleImg = handle.AddComponent<Image>();
            handleImg.color = Color.white;
            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 0);
            slider.handleRect = handleRect;

            return slider;
        }

        private Toggle CreateToggle(GameObject parent, Vector2 position)
        {
            GameObject toggleObj = DefaultControls.CreateToggle(new DefaultControls.Resources());
            toggleObj.transform.SetParent(parent.transform, false);
            RectTransform rect = toggleObj.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            return toggleObj.GetComponent<Toggle>();
        }

        private GameObject CreateButton(GameObject parent, string label, Vector2 position)
        {
            GameObject btnObj = DefaultControls.CreateButton(new DefaultControls.Resources());
            btnObj.transform.SetParent(parent.transform, false);
            RectTransform rect = btnObj.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(100, 30);
            
            Text txt = btnObj.GetComponentInChildren<Text>();
            if(txt) txt.text = label; 

            return btnObj;
        }
    }
}
#endif
