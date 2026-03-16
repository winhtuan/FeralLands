using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class GameData
{
    // Player state
    public int level;
    public int score; // currentExp
    public float playerHealth;
    public float playerPosX;
    public float playerPosY;
    public string currentSceneName;

    // Applied augment type indices
    public int[] unlockedAugments;

    // Enemy state — IDs of enemies killed this run
    public List<string> killedEnemyIDs = new List<string>();

    // Settings — mirrored here for portability, authoritative copy stays in PlayerPrefs
    public float musicVolume;
    public float sfxVolume;
    public bool isFullscreen;
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private static string SavePath => Application.persistentDataPath + "/save.json";

    // Runtime cache — tracks killed enemies during this session
    private HashSet<string> _killedThisSession = new HashSet<string>();

    // Auto-create SaveManager before any scene loads — no prefab needed in scene.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreate()
    {
        if (Instance != null)
            return;
        var go = new GameObject("SaveManager");
        Instance = go.AddComponent<SaveManager>();
        DontDestroyOnLoad(go);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>Mark an enemy as killed so it won't respawn when the scene reloads.</summary>
    public void MarkEnemyKilled(string id)
    {
        _killedThisSession.Add(id);
    }

    /// <summary>Returns true if this enemy was already killed in the current session.</summary>
    public bool WasEnemyKilled(string id)
    {
        return _killedThisSession.Contains(id);
    }

    /// <summary>
    /// Writes GameData to disk as JSON. Settings fields are also mirrored to PlayerPrefs.
    /// Caller must supply the data; killed-enemy list is merged from runtime cache.
    /// </summary>
    public void SaveGame(GameData data)
    {
        // Merge runtime killed-enemy cache into the save data
        data.killedEnemyIDs = new List<string>(_killedThisSession);

        // Mirror settings to PlayerPrefs so SettingsController can read them
        PlayerPrefs.SetFloat("Setting_Music", data.musicVolume);
        PlayerPrefs.SetFloat("Setting_SFX", data.sfxVolume);
        PlayerPrefs.SetInt("Setting_Fullscreen", data.isFullscreen ? 1 : 0);
        PlayerPrefs.Save();

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);

        Debug.Log("[SaveManager] Saved to: " + SavePath);
    }

    /// <summary>Returns GameData from disk and restores the killed-enemy cache.
    /// Returns null if no save file exists or file is corrupted.</summary>
    public GameData LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("[SaveManager] No save file found at: " + SavePath);
            return null;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            GameData data = JsonUtility.FromJson<GameData>(json);

            // Restore killed-enemy cache from disk
            _killedThisSession.Clear();
            if (data.killedEnemyIDs != null)
            {
                foreach (string id in data.killedEnemyIDs)
                    _killedThisSession.Add(id);
            }

            Debug.Log(
                $"[SaveManager] Loaded from: {SavePath} ({_killedThisSession.Count} killed enemies restored)"
            );
            return data;
        }
        catch (Exception e)
        {
            Debug.LogWarning("[SaveManager] Failed to read save file: " + e.Message);
            return null;
        }
    }

    public static bool HasSave()
    {
        return File.Exists(SavePath);
    }

    public void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);

        _killedThisSession.Clear();

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("[SaveManager] Save deleted.");
    }
}
