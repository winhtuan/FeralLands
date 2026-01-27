using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro; // Nếu bạn dùng TextMeshPro
using System.Collections.Generic;

public class SettingsMenu : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioMixer audioMixer; // Kéo AudioMixer vào đây
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Graphics Settings")]
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    private Resolution[] resolutions;

    void Start()
    {
        // 1. Kiểm tra AudioMixer trước khi dùng
        if (audioMixer == null)
        {
            Debug.LogError("Chưa gắn AudioMixer vào SettingsMenu! Hãy kéo AudioMixer vào ô Audio Mixer trong Inspector.");
            // Vẫn cứ chạy tiếp để Load UI, nhưng chặn gọi set volume
        }
        else
        {
            // Apply ngay lập tức nếu có Mixer
            float savedMusicVol = PlayerPrefs.GetFloat("MusicVol", 0.75f);
            float savedSFXVol = PlayerPrefs.GetFloat("SFXVol", 0.75f);
            SetMusicVolume(savedMusicVol);
            SetSFXVolume(savedSFXVol);
        }

        // Load Slider UI (Làm riêng ra để không bị null UI)
        if (musicSlider != null) musicSlider.value = PlayerPrefs.GetFloat("MusicVol", 0.75f);
        if (sfxSlider != null) sfxSlider.value = PlayerPrefs.GetFloat("SFXVol", 0.75f);

        // 2. Setup Fullscreen
        if (fullscreenToggle != null) fullscreenToggle.isOn = Screen.fullScreen;

        // 3. Setup Resolution (Tự động lấy độ phân giải máy tính)
        if (resolutionDropdown != null)
        {
            resolutions = Screen.resolutions;
            resolutionDropdown.ClearOptions();

            List<string> options = new List<string>();
            int currentResolutionIndex = 0;

            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = resolutions[i].width + " x " + resolutions[i].height;
                options.Add(option);

                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (audioMixer == null) return;

        // Mixer thường dùng scale Logarithmic (-80dB to 0dB)
        // Công thức chuyển từ Slider (0-1) sang dB: Mathf.Log10(volume) * 20
        // Đoạn này check > 0 để tránh lỗi Log(0)
        float dbVolume = (volume > 0.0001f) ? Mathf.Log10(volume) * 20 : -80f;
        
        audioMixer.SetFloat("MusicVol", dbVolume);
        
        // Lưu lại
        PlayerPrefs.SetFloat("MusicVol", volume);
        PlayerPrefs.Save(); // Lưu xuống ổ cứng luôn cho chắc
    }

    public void SetSFXVolume(float volume)
    {
        if (audioMixer == null) return;

        float dbVolume = (volume > 0.0001f) ? Mathf.Log10(volume) * 20 : -80f;
        audioMixer.SetFloat("SFXVol", dbVolume);
        
        PlayerPrefs.SetFloat("SFXVol", volume);
        PlayerPrefs.Save();
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        // Lưu lại
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetResolution(int resolutionIndex)
    {
        if (resolutions == null || resolutionIndex >= resolutions.Length) return;

        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        
        // Lưu lại index
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
        PlayerPrefs.Save();
    }
}
