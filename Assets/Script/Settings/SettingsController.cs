using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro; 
using System.Collections.Generic;

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

    void Start()
    {
        // 1. Tự động load cài đặt cũ ngay khi game bật lên
        LoadSettings();

        // 2. Gán sự kiện cho các nút
        if (musicSlider != null) musicSlider.onValueChanged.AddListener(SetMusicVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        
        if (controlSchemeButton != null) controlSchemeButton.onClick.AddListener(ToggleControlScheme);
    }

    public void ToggleControlScheme()
    {
        int current = PlayerPrefs.GetInt(PREF_CONTROL, 0);
        int newValue = (current == 0) ? 1 : 0;
        
        PlayerPrefs.SetInt(PREF_CONTROL, newValue);
        PlayerPrefs.Save();
        
        UpdateControlText(newValue);
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
        if (mainMixer != null) mainMixer.SetFloat(MIXER_MUSIC, db);
        PlayerPrefs.SetFloat("Setting_Music", value);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        float db = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
        if (mainMixer != null) mainMixer.SetFloat(MIXER_SFX, db);
        PlayerPrefs.SetFloat("Setting_SFX", value);
        PlayerPrefs.Save();
    }

    public void SetFullscreen(bool isFullscreen)
    {
        // Safety check for resolutions
        if (Screen.resolutions == null || Screen.resolutions.Length == 0)
        {
            Debug.LogWarning("SettingsController: Could not find valid screen resolutions.");
            Screen.fullScreen = isFullscreen; // Fallback basic
            return;
        }

        if (isFullscreen)
        {
            Resolution maxRes = Screen.resolutions[Screen.resolutions.Length - 1];
            Screen.SetResolution(maxRes.width, maxRes.height, FullScreenMode.FullScreenWindow);
        }
        else
        {
            Screen.SetResolution(Screen.width, Screen.height, FullScreenMode.Windowed);
        }
        
        PlayerPrefs.SetInt("Setting_Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        // Load Fullscreen
        bool isFull = PlayerPrefs.GetInt("Setting_Fullscreen", 1) == 1;
        // Don't call SetFullscreen here to avoid heavy operations on startup, just set state
        if (fullscreenToggle != null) fullscreenToggle.isOn = isFull;
        Screen.fullScreen = isFull; 

        // Load Volume
        float musicVal = PlayerPrefs.GetFloat("Setting_Music", 0.75f);
        float sfxVal = PlayerPrefs.GetFloat("Setting_SFX", 0.75f);

        if (musicSlider != null) musicSlider.value = musicVal;
        if (sfxSlider != null) sfxSlider.value = sfxVal;

        SetMusicVolume(musicVal);
        SetSFXVolume(sfxVal);
        
        // Load Control Scheme
        int scheme = PlayerPrefs.GetInt(PREF_CONTROL, 0);
        UpdateControlText(scheme);
    }
}
