using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSaveLoad : MonoBehaviour
{
    public static PlayerSaveLoad Instance { get; private set; }

    private PlayerHealth health;
    private PlayerLevel level;
    private GameData _loadedData; // cached for Start() augment + skip-trigger step

    void Awake()
    {
        Instance = this;
        health = GetComponent<PlayerHealth>();
        level = GetComponent<PlayerLevel>();

        // Must run in Awake so SaveManager._killedThisSession is populated
        // before any NormalEnemyBase.Start() calls WasEnemyKilled().
        if (SaveManager.Instance == null)
            return;
        _loadedData = SaveManager.Instance.GetCachedData() ?? SaveManager.Instance.LoadGame();
        if (_loadedData == null)
            return;

        // Apply augment STAT bonuses (maxHealth, damage, speed, skin…) here.
        // currentHealth/level restoration is deferred to Start() so that
        // PlayerHealth.Awake() (which resets currentHealth = maxHealth) cannot
        // overwrite it, regardless of Script Execution Order.
        ApplyAugmentStats(_loadedData.appliedAugments);
    }

    void Start()
    {
        if (_loadedData == null)
            return;

        // All Awake()s have run — PlayerHealth.Awake() has already set
        // currentHealth = maxHealth (base). Now we safely override it.

        // 1. Restore health clamped to (now-augmented) maxHealth.
        if (health != null)
            health.currentHealth = Mathf.Clamp((int)_loadedData.playerHealth, 1, health.maxHealth);

        // 2. Restore level & exp.
        if (level != null)
        {
            level.currentLevel = Mathf.Max(1, _loadedData.level);
            level.currentExp = _loadedData.score;
        }

        // 3. Restore position (same scene only, skip (0,0) sentinel from SaveGameForScene).
        if (
            _loadedData.currentSceneName == SceneManager.GetActiveScene().name
            && (_loadedData.playerPosX != 0f || _loadedData.playerPosY != 0f)
        )
        {
            transform.position = new Vector3(
                _loadedData.playerPosX,
                _loadedData.playerPosY,
                transform.position.z
            );
            Debug.Log(
                $"[Load] Position restored: ({_loadedData.playerPosX}, {_loadedData.playerPosY})"
            );
        }
        else
        {
            Debug.Log($"[Load] Scene '{SceneManager.GetActiveScene().name}' — using spawn point.");
        }

        // 4. Restore audio settings.
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(_loadedData.musicVolume);
            AudioManager.Instance.SetSFXVolume(_loadedData.sfxVolume);
        }

        // 5. Mark already-seen augment levels so they don't trigger again.
        if (AugmentManager.Instance != null)
            AugmentManager.Instance.SkipTriggersUpToLevel(_loadedData.level);

        Debug.Log(
            $"[Load] Restored — Level {_loadedData.level}, HP {health?.currentHealth}/{health?.maxHealth}, Augments {_loadedData.appliedAugments?.Count ?? 0}"
        );

        _loadedData = null;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // ── Augment restoration ────────────────────────────────────────────────────

    private void ApplyAugmentStats(List<AugmentSaveEntry> entries)
    {
        if (entries == null || entries.Count == 0)
            return;

        PlayerMeleeAttack melee = GetComponent<PlayerMeleeAttack>();
        PlayerMovement movement = GetComponent<PlayerMovement>();
        PlayerEnergy energy = GetComponent<PlayerEnergy>();
        PlayerCastOrb castOrb = GetComponent<PlayerCastOrb>();
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        // Reset to Inspector base values before adding bonuses.
        // This prevents double-add if ApplyAugmentStats is ever called twice.
        if (health != null)
            health.maxHealth = health.baseMaxHealth;
        if (melee != null)
            melee.meleeDamage = melee.baseMeleeDamage;
        if (movement != null)
            movement.moveSpeed = movement.baseMoveSpeed;

        foreach (var e in entries)
        {
            switch (e.statType)
            {
                case "Health":
                    if (health != null)
                        health.maxHealth += (int)e.value;
                    break;

                case "Damage":
                    if (melee != null)
                        melee.meleeDamage += (int)e.value;
                    break;

                case "Speed":
                    if (movement != null)
                        movement.moveSpeed += e.value;
                    break;

                case "Skin":
                    if (sr != null)
                        sr.color = new Color(e.skinR, e.skinG, e.skinB, e.skinA);
                    break;

                case "ManaRegen":
                    if (energy != null)
                        energy.regenRate += e.value;
                    break;

                case "AttackSpeed":
                    if (melee != null)
                        melee.attackCooldown *= (1f - e.value);
                    break;

                case "TripleOrb":
                    if (castOrb != null)
                        castOrb.isTripleOrbActive = true;
                    break;
            }
        }
    }

    // ── Save ───────────────────────────────────────────────────────────────────

    private GameData CollectGameData()
    {
        var data = new GameData
        {
            currentSceneName = SceneManager.GetActiveScene().name,
            playerPosX = transform.position.x,
            playerPosY = transform.position.y,
            playerHealth = health != null ? health.currentHealth : 0,
            level = level != null ? level.currentLevel : 1,
            score = level != null ? level.currentExp : 0,
            musicVolume = PlayerPrefs.GetFloat("Setting_Music", 0.75f),
            sfxVolume = PlayerPrefs.GetFloat("Setting_SFX", 0.75f),
            isFullscreen = PlayerPrefs.GetInt("Setting_Fullscreen", 1) == 1,
        };

        data.enemyPositions = new List<EnemyPositionData>();
        foreach (var enemy in FindObjectsByType<NormalEnemyBase>(FindObjectsSortMode.None))
        {
            data.enemyPositions.Add(
                new EnemyPositionData
                {
                    enemyID = enemy.EnemyID,
                    posX = enemy.transform.position.x,
                    posY = enemy.transform.position.y,
                }
            );
        }
        return data;
    }

    /// <summary>Save current state. Scene name = current scene.</summary>
    public void SaveGame()
    {
        if (SaveManager.Instance == null)
            return;
        SaveManager.Instance.SaveGame(CollectGameData());
    }

    /// <summary>Save before a scene transition.
    /// Stores destination as currentSceneName so position is NOT
    /// restored in the new scene (different name won't match).</summary>
    public void SaveGameForScene(string destinationScene)
    {
        if (SaveManager.Instance == null)
            return;
        GameData data = CollectGameData();
        data.currentSceneName = destinationScene;
        data.playerPosX = 0f; // sentinel — use spawn point in new scene
        data.playerPosY = 0f;
        data.enemyPositions = new List<EnemyPositionData>(); // old scene positions are irrelevant
        SaveManager.Instance.SaveGame(data);
        Debug.Log(
            $"[Save] Transitioning to '{destinationScene}' — stats saved, positions cleared."
        );
    }

    public void OnSaveButtonClicked() => SaveGame();
}
