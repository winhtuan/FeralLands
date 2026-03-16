using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

// Removed namespace to avoid complexity and conflicts
public class SettingsController : MonoBehaviour
{
    [Header("--- CONNECT AUDIO ---")]
    [Tooltip("Kéo AudioMixer vào đây (Bắt buộc)")]
    public AudioMixer mainMixer;

    [Header("--- UI REFERENCES ---")]
    [Tooltip("Kéo Slider chỉnh nhạc vào đây")]
    public Slider musicSlider;

    [Tooltip("Kéo Slider chỉnh sfx vào đây")]
    public Slider sfxSlider;

    [Tooltip("Kéo Toggle chỉnh Fullscreen vào đây")]
    public Toggle fullscreenToggle;

    [Header("--- INPUT SETTINGS ---")]
    public Button controlSchemeButton;
    public TextMeshProUGUI controlSchemeText;

    private const string PREF_CONTROL = "ControlScheme"; // 0 = WASD, 1 = Arrows
    private const string MIXER_MUSIC = "MusicVol";
    private const string MIXER_SFX = "SFXVol";

    private bool listenersAdded = false;

    void Start()
    {
        LoadSettings();
        RegisterListeners();
    }

    /// <summary>
    /// Gọi từ nút mở Settings — KHÔNG dùng SetActive(true) trực tiếp.
    /// </summary>
    public void OpenPanel()
    {
        gameObject.SetActive(true);
        LoadSettings();
        UpdateControlText(PlayerPrefs.GetInt(PREF_CONTROL, 0));
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Gọi từ nút Close — KHÔNG dùng SetActive(false) trực tiếp.
    /// </summary>
    public void ClosePanel()
    {
        // Chỉ resume nếu AugmentManager không đang mở panel của nó
        bool augmentOpen = AugmentManager.Instance != null && AugmentManager.Instance.IsPanelVisible;
        if (!augmentOpen)
            Time.timeScale = 1f;

        gameObject.SetActive(false);
    }

    private void RegisterListeners()
    {
        if (listenersAdded)
            return;
        listenersAdded = true;

        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);

        // TỰ ĐỘNG tìm nút CHANGE và SAVE bằng chữ hiển thị, không dựa vào Inspector
        Button[] allButtons = GetComponentsInChildren<Button>(true);
        foreach (Button btn in allButtons)
        {
            // Kiểm tra text (UI cũ)
            Text btnText = btn.GetComponentInChildren<Text>();
            if (btnText != null && btnText.text.Trim().ToUpper() == "CHANGE")
            {
                btn.onClick.AddListener(DoToggleControlScheme);
                Debug.Log(
                    "[SettingsController] Đã tự động gán DoToggle vào nút: " + btn.gameObject.name
                );
                break;
            }

            // Kiểm tra TextMeshPro (UI mới)
            TextMeshProUGUI tmpText = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null && tmpText.text.Trim().ToUpper() == "CHANGE")
            {
                btn.onClick.AddListener(DoToggleControlScheme);
                Debug.Log(
                    "[SettingsController] Đã tự động gán DoToggle vào nút TMP: "
                        + btn.gameObject.name
                );
                break;
            }
        }

        // Auto-find the SAVE button
        foreach (Button btn in allButtons)
        {
            Text btnText = btn.GetComponentInChildren<Text>();
            if (btnText != null && btnText.text.Trim().ToUpper() == "SAVE")
            {
                btn.onClick.AddListener(OnSaveButtonClicked);
                Debug.Log(
                    "[SettingsController] Đã tự động gán Save vào nút: " + btn.gameObject.name
                );
                break;
            }

            TextMeshProUGUI tmpText = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null && tmpText.text.Trim().ToUpper() == "SAVE")
            {
                btn.onClick.AddListener(OnSaveButtonClicked);
                Debug.Log(
                    "[SettingsController] Đã tự động gán Save vào nút TMP: " + btn.gameObject.name
                );
                break;
            }
        }

        // Auto-find the CLOSE button — dùng ClosePanel() để đảm bảo timeScale được restore đúng
        foreach (Button btn in allButtons)
        {
            Text btnText = btn.GetComponentInChildren<Text>();
            if (btnText != null && btnText.text.Trim().ToUpper() == "CLOSE")
            {
                btn.onClick.AddListener(ClosePanel);
                Debug.Log(
                    "[SettingsController] Đã tự động gán ClosePanel vào nút: " + btn.gameObject.name
                );
                break;
            }

            TextMeshProUGUI tmpText = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null && tmpText.text.Trim().ToUpper() == "CLOSE")
            {
                btn.onClick.AddListener(ClosePanel);
                Debug.Log(
                    "[SettingsController] Đã tự động gán ClosePanel vào nút TMP: " + btn.gameObject.name
                );
                break;
            }
        }
    }

    /// <summary>
    /// Called by the SAVE button: flushes current slider/toggle values then triggers a full game save.
    /// </summary>
    public void OnSaveButtonClicked()
    {
        if (musicSlider != null)
            SetMusicVolume(musicSlider.value);
        if (sfxSlider != null)
            SetSFXVolume(sfxSlider.value);
        if (fullscreenToggle != null)
            SetFullscreen(fullscreenToggle.isOn);

        PlayerSaveLoad.Instance?.SaveGame();

        Debug.Log("[SettingsController] Settings and game saved. Returning to Main Menu.");

        Time.timeScale = 1f; // Must restore before scene transition
        gameObject.SetActive(false);

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.TeleportToMap("MainMenu");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    // Hàm public này CỐ TÌNH để trống
    // Nếu nút Close (hoặc bất kỳ nút nào) gọi hàm này qua Inspector => KHÔNG LÀM GÌ CẢ
    public void ToggleControlScheme()
    {
        // Không làm gì. Logic thật nằm ở DoToggleControlScheme().
    }

    // Hàm private - CHỈ được gọi từ nút Change qua AddListener
    private void DoToggleControlScheme()
    {
        int current = PlayerPrefs.GetInt(PREF_CONTROL, 0);
        int newValue = (current == 0) ? 1 : 0;

        PlayerPrefs.SetInt(PREF_CONTROL, newValue);
        PlayerPrefs.Save();

        UpdateControlText(newValue);
        Debug.Log($"[SettingsController] DoToggle: {current} -> {newValue}");
    }

    private void UpdateControlText(int scheme)
    {
        if (controlSchemeText != null)
        {
            controlSchemeText.text = (scheme == 0) ? "CONTROLS: WASD" : "CONTROLS: ARROWS";
        }
    }

    public void SetMusicVolume(float value)
    {
        float db = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
        if (mainMixer != null)
            mainMixer.SetFloat(MIXER_MUSIC, db);

        // Gọi AudioManager để đồng bộ (nếu không dùng Mixer thì code này sẽ chỉnh volume source)
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(value);

        PlayerPrefs.SetFloat("Setting_Music", value);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        float db = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
        if (mainMixer != null)
            mainMixer.SetFloat(MIXER_SFX, db);

        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);

        PlayerPrefs.SetFloat("Setting_SFX", value);
        PlayerPrefs.Save();
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;

        if (isFullscreen)
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }

        PlayerPrefs.SetInt("Setting_Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log("Fullscreen set to: " + isFullscreen);
    }

    private void LoadSettings()
    {
        // Load Fullscreen
        bool isFull = PlayerPrefs.GetInt("Setting_Fullscreen", 1) == 1;
        // Don't call SetFullscreen here to avoid heavy operations on startup, just set state
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = isFull;
        Screen.fullScreen = isFull;

        // Load Volume
        float musicVal = PlayerPrefs.GetFloat("Setting_Music", 0.75f);
        float sfxVal = PlayerPrefs.GetFloat("Setting_SFX", 0.75f);

        if (musicSlider != null)
            musicSlider.value = musicVal;
        if (sfxSlider != null)
            sfxSlider.value = sfxVal;

        SetMusicVolume(musicVal);
        SetSFXVolume(sfxVal);

        // Load Control Scheme
        int scheme = PlayerPrefs.GetInt(PREF_CONTROL, 0);
        UpdateControlText(scheme);
    }
}
