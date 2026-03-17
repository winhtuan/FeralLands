using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSaveLoad : MonoBehaviour
{
    public static PlayerSaveLoad Instance { get; private set; }

    private PlayerHealth      health;
    private PlayerLevel       level;
    private GameData          _loadedData; // cached for Start() augment + skip-trigger step

    void Awake()
    {
        Instance = this;
        health = GetComponent<PlayerHealth>();
        level  = GetComponent<PlayerLevel>();

        // Load in Awake so the killed-enemy cache is populated before
        // any NormalEnemyBase.Start() calls WasEnemyKilled().
        if (SaveManager.Instance == null) return;
        _loadedData = SaveManager.Instance.LoadGame();
        if (_loadedData == null) return;

        // 1. Restore augment stat boosts FIRST so maxHealth is correct before clamping currentHealth.
        ApplyAugmentStats(_loadedData.appliedAugments);

        // 2. Restore position (only if re-entering the same scene that was saved).
        if (_loadedData.currentSceneName == SceneManager.GetActiveScene().name)
            transform.position = new Vector3(_loadedData.playerPosX, _loadedData.playerPosY, transform.position.z);

        // 3. Restore health (clamp against augmented maxHealth).
        if (health != null)
            health.currentHealth = Mathf.Clamp((int)_loadedData.playerHealth, 1, health.maxHealth);

        // 4. Restore level & exp.
        if (level != null)
        {
            level.currentLevel = Mathf.Max(1, _loadedData.level);
            level.currentExp   = _loadedData.score;
        }

        // 5. Restore audio settings.
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(_loadedData.musicVolume);
            AudioManager.Instance.SetSFXVolume(_loadedData.sfxVolume);
        }
    }

    void Start()
    {
        if (_loadedData == null) return;

        // AugmentManager.Awake() is guaranteed done by the time Start() runs.
        // Mark all augment-trigger levels already seen so they don't fire again.
        AugmentManager.Instance?.SkipTriggersUpToLevel(_loadedData.level);

        _loadedData = null; // release reference
    }

    // ── Augment restoration ────────────────────────────────────────────────────

    private void ApplyAugmentStats(List<AugmentSaveEntry> entries)
    {
        if (entries == null || entries.Count == 0) return;

        PlayerMeleeAttack melee    = GetComponent<PlayerMeleeAttack>();
        PlayerMovement    movement = GetComponent<PlayerMovement>();
        PlayerEnergy      energy   = GetComponent<PlayerEnergy>();
        PlayerCastOrb     castOrb  = GetComponent<PlayerCastOrb>();
        SpriteRenderer    sr       = GetComponent<SpriteRenderer>();

        foreach (var e in entries)
        {
            switch (e.statType)
            {
                case "Health":
                    if (health != null)
                    {
                        health.maxHealth     += (int)e.value;
                        // currentHealth will be set afterwards from saved value
                    }
                    break;

                case "Damage":
                    if (melee != null) melee.meleeDamage += (int)e.value;
                    break;

                case "Speed":
                    if (movement != null) movement.moveSpeed += e.value;
                    break;

                case "Skin":
                    if (sr != null) sr.color = new Color(e.skinR, e.skinG, e.skinB, e.skinA);
                    break;

                case "ManaRegen":
                    if (energy != null) energy.regenRate += e.value;
                    break;

                case "AttackSpeed":
                    if (melee != null) melee.attackCooldown *= (1f - e.value);
                    break;

                case "TripleOrb":
                    if (castOrb != null) castOrb.isTripleOrbActive = true;
                    break;
            }
        }
    }

    // ── Save ───────────────────────────────────────────────────────────────────

    public void SaveGame()
    {
        if (SaveManager.Instance == null) return;

        GameData data = new GameData
        {
            currentSceneName = SceneManager.GetActiveScene().name,
            playerPosX       = transform.position.x,
            playerPosY       = transform.position.y,
            playerHealth     = health != null ? health.currentHealth : 0,
            level            = level  != null ? level.currentLevel   : 1,
            score            = level  != null ? level.currentExp     : 0,
            musicVolume      = PlayerPrefs.GetFloat("Setting_Music",       0.75f),
            sfxVolume        = PlayerPrefs.GetFloat("Setting_SFX",         0.75f),
            isFullscreen     = PlayerPrefs.GetInt  ("Setting_Fullscreen",  1) == 1,
        };
        // Collect alive enemy positions
        data.enemyPositions = new List<EnemyPositionData>();
        foreach (var enemy in FindObjectsByType<NormalEnemyBase>(FindObjectsSortMode.None))
        {
            data.enemyPositions.Add(new EnemyPositionData
            {
                enemyID = enemy.EnemyID,
                posX    = enemy.transform.position.x,
                posY    = enemy.transform.position.y,
            });
        }

        // appliedAugments and killedEnemyIDs are merged inside SaveManager.SaveGame()
        SaveManager.Instance.SaveGame(data);
    }

    public void OnSaveButtonClicked() => SaveGame();
}
