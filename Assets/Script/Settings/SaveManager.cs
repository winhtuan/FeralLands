using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class EnemyPositionData
{
    public string enemyID;
    public float posX;
    public float posY;
}

[System.Serializable]
public class AugmentSaveEntry
{
    public int augmentID;       // AugmentData.AugmentType cast to int
    public string statType;     // AugmentData.AugmentType.ToString()
    public float value;         // augment.value
    // Skin color fields (only used when statType == "Skin")
    public float skinR, skinG, skinB, skinA;
}

[System.Serializable]
public class GameData
{
    // Player state
    public int level;
    public int score;           // currentExp
    public float playerHealth;
    public float playerPosX;
    public float playerPosY;
    public string currentSceneName;

    // Augments — full entries replace old int[] unlockedAugments
    public List<AugmentSaveEntry> appliedAugments = new List<AugmentSaveEntry>();

    // Enemy state
    public List<string> killedEnemyIDs = new List<string>();
    public List<EnemyPositionData> enemyPositions = new List<EnemyPositionData>();

    // Settings — mirrored here for portability, authoritative copy stays in PlayerPrefs
    public float musicVolume;
    public float sfxVolume;
    public bool isFullscreen;
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private static string SavePath => Application.persistentDataPath + "/save.json";

    // Runtime caches
    private HashSet<string>        _killedThisSession = new HashSet<string>();
    private List<AugmentSaveEntry> _appliedAugments   = new List<AugmentSaveEntry>();
    private GameData               _cachedData;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreate()
    {
        if (Instance != null) return;
        var go = new GameObject("SaveManager");
        Instance = go.AddComponent<SaveManager>();
        DontDestroyOnLoad(go);
    }

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else if (Instance != this) { Destroy(gameObject); }
    }

    // ── Enemy tracking ─────────────────────────────────────────────────────────

    public void MarkEnemyKilled(string id) => _killedThisSession.Add(id);
    public bool WasEnemyKilled(string id)  => _killedThisSession.Contains(id);

    // ── Augment tracking ───────────────────────────────────────────────────────

    /// <summary>Called by AugmentManager each time the player picks an augment.</summary>
    public void AddAppliedAugment(AugmentData augData)
    {
        _appliedAugments.Add(new AugmentSaveEntry
        {
            augmentID = (int)augData.type,
            statType  = augData.type.ToString(),
            value     = augData.value,
            skinR = augData.skinColor.r,
            skinG = augData.skinColor.g,
            skinB = augData.skinColor.b,
            skinA = augData.skinColor.a,
        });
    }

    public List<AugmentSaveEntry> GetAppliedAugments() => _appliedAugments;

    // ── Save / Load ────────────────────────────────────────────────────────────

    public void SaveGame(GameData data)
    {
        data.killedEnemyIDs  = new List<string>(_killedThisSession);
        data.appliedAugments = new List<AugmentSaveEntry>(_appliedAugments);

        PlayerPrefs.SetFloat("Setting_Music",      data.musicVolume);
        PlayerPrefs.SetFloat("Setting_SFX",        data.sfxVolume);
        PlayerPrefs.SetInt  ("Setting_Fullscreen",  data.isFullscreen ? 1 : 0);
        PlayerPrefs.Save();

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log("[SaveManager] Saved to: " + SavePath);
    }

    public GameData LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("[SaveManager] No save file found at: " + SavePath);
            return null;
        }

        try
        {
            string json   = File.ReadAllText(SavePath);
            GameData data = JsonUtility.FromJson<GameData>(json);

            // Restore runtime caches
            _killedThisSession.Clear();
            if (data.killedEnemyIDs != null)
                foreach (string id in data.killedEnemyIDs)
                    _killedThisSession.Add(id);

            _appliedAugments.Clear();
            if (data.appliedAugments != null)
                _appliedAugments.AddRange(data.appliedAugments);

            Debug.Log($"[SaveManager] Loaded — {_killedThisSession.Count} killed enemies, {_appliedAugments.Count} augments");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogWarning("[SaveManager] Failed to read save file: " + e.Message);
            return null;
        }
    }

    /// <summary>Returns the last GameData loaded from disk without re-reading the file.</summary>
    public GameData GetCachedData() => _cachedData;

    public static bool HasSave() => File.Exists(SavePath);

    public void DeleteSave()
    {
        if (File.Exists(SavePath)) File.Delete(SavePath);
        _killedThisSession.Clear();
        _appliedAugments.Clear();
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[SaveManager] Save deleted.");
    }
}
