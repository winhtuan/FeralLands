using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSaveLoad : MonoBehaviour
{
    public static PlayerSaveLoad Instance { get; private set; }

    private PlayerHealth health;
    private PlayerLevel level;

    void Awake()
    {
        Instance = this;
        health = GetComponent<PlayerHealth>();
        level  = GetComponent<PlayerLevel>();

        // Load in Awake so the killed-enemy cache is populated before any
        // NormalEnemyBase.Start() calls check WasEnemyKilled().
        // All Awake() calls complete before any Start() calls begin in Unity.
        RestoreFromSave();
    }

    private void RestoreFromSave()
    {
        if (SaveManager.Instance == null)
            return;

        GameData data = SaveManager.Instance.LoadGame();
        if (data == null)
            return;

        // Only restore position if we're loading back into the same scene that was saved
        if (data.currentSceneName == SceneManager.GetActiveScene().name)
            transform.position = new Vector3(data.playerPosX, data.playerPosY, transform.position.z);

        if (health != null)
            health.currentHealth = Mathf.Clamp((int)data.playerHealth, 1, health.maxHealth);

        if (level != null)
        {
            level.currentLevel = Mathf.Max(1, data.level);
            level.currentExp   = data.score;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(data.musicVolume);
            AudioManager.Instance.SetSFXVolume(data.sfxVolume);
        }
    }

    /// <summary>
    /// Collects current player state and persists via SaveManager.
    /// Call from UI buttons or auto-save triggers.
    /// </summary>
    public void SaveGame()
    {
        if (SaveManager.Instance == null)
            return;

        AugmentManager aug = FindFirstObjectByType<AugmentManager>();

        GameData data = new GameData
        {
            currentSceneName  = SceneManager.GetActiveScene().name,
            playerPosX        = transform.position.x,
            playerPosY        = transform.position.y,
            playerHealth      = health != null ? health.currentHealth : 0,
            level             = level  != null ? level.currentLevel   : 1,
            score             = level  != null ? level.currentExp     : 0,
            musicVolume       = PlayerPrefs.GetFloat("Setting_Music", 0.75f),
            sfxVolume         = PlayerPrefs.GetFloat("Setting_SFX",   0.75f),
            isFullscreen      = PlayerPrefs.GetInt("Setting_Fullscreen", 1) == 1,
            unlockedAugments  = aug != null ? aug.appliedAugmentTypes.ToArray() : Array.Empty<int>(),
        };

        SaveManager.Instance.SaveGame(data);
        Debug.Log("[PlayerSaveLoad] Saved to: " + Application.persistentDataPath + "/save.json");
    }

    /// <summary>Wired to the Save button in the UI Inspector.</summary>
    public void OnSaveButtonClicked()
    {
        SaveGame();
    }
}
