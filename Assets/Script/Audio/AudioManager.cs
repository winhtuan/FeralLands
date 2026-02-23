using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("--- CONFIGURATION ---")]
    [Tooltip("Kéo AudioMixerGroup Music vào đây (nếu có)")]
    public AudioMixerGroup musicGroup;
    
    [Tooltip("Kéo AudioMixerGroup SFX vào đây (nếu có)")]
    public AudioMixerGroup sfxGroup;

    [Header("--- MUSIC TRACKS ---")]
    [Tooltip("Kéo file 'intense_boss_battle.mp3' vào đây")]
    public AudioClip bossBattleMusic;

    // Audio Sources
    private AudioSource musicSource;
    private AudioSource sfxSource;

    private void Awake()
    {
        // Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ AudioManager sống qua các màn chơi
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeAudioSources();
    }

    private void InitializeAudioSources()
    {
        // Tạo AudioSource cho Music
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.outputAudioMixerGroup = musicGroup;
        musicSource.loop = true; // Nhạc nền phải lặp
        musicSource.playOnAwake = false;

        // Tạo AudioSource cho SFX
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.outputAudioMixerGroup = sfxGroup;
        sfxSource.playOnAwake = false;
    }

    private void Start()
    {
        // 1. Đăng ký sự kiện chuyển scene
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // 2. Load Volume cài đặt từ trước (Backup nếu không dùng Mixer)
        float savedMusicVol = PlayerPrefs.GetFloat("Setting_Music", 0.75f);
        float savedSFXVol = PlayerPrefs.GetFloat("Setting_SFX", 0.75f);
        SetMusicVolume(savedMusicVol);
        SetSFXVolume(savedSFXVol);

        // 3. Kiểm tra nhạc nền cho scene hiện tại
        PlayMusicForCurrentScene();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForCurrentScene();
    }

    private void PlayMusicForCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        // Debug.Log("Scene loaded: " + currentScene);

        // Logic chọn nhạc theo Scene
        if (currentScene == "MapBeachTestQuan")
        {
            if (bossBattleMusic != null)
            {
                PlayMusic(bossBattleMusic);
            }
        }
        // Thêm các scene khác tại đây
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // --- VOLUME CONTROL ---
    // Được gọi từ SettingsController
    public void SetMusicVolume(float value)
    {
        if (musicSource == null) return;

        // Nếu đã gắn Mixer Group, ta để Mixer lo việc to nhỏ (giữ volume source = 1)
        // Nếu CHƯA gắn Mixer Group, ta chỉnh trực tiếp volume của source
        if (musicGroup != null)
        {
            musicSource.volume = 1f; 
        }
        else
        {
            musicSource.volume = value;
        }
    }

    public void SetSFXVolume(float value)
    {
        if (sfxSource == null) return;

        if (sfxGroup != null)
        {
            sfxSource.volume = 1f;
        }
        else
        {
            sfxSource.volume = value;
        }
    }
}
