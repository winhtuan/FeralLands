using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// AugmentManager — Quản lý logic chọn Lõi ở nhiều mốc cấp (2, 5, 8, 10).
/// Gắn lên một GameObject bất kỳ trong Scene (ví dụ: AugmentManager).
/// </summary>
public class AugmentManager : MonoBehaviour
{
    public static AugmentManager Instance { get; private set; }

    /// <summary>True khi AugmentUI đang mở (game đang bị pause bởi Augment).</summary>
    public bool IsPanelVisible { get; private set; }

    [Header("Augment Pools — Mỗi level có pool riêng")]
    public AugmentData[] augmentPoolLv2;   // Level 2: HP, DMG, Speed
    public AugmentData[] augmentPoolLv5;   // Level 5: Skin (đổi màu)
    public AugmentData[] augmentPoolLv8;   // Level 8: Mana Regen, Attack Speed, Double Orb
    public AugmentData[] augmentPoolLv10;  // Level 10: HP+, DMG+, Speed+ (mạnh hơn)

    [Header("References")]
    public PlayerLevel playerLevel;
    public AugmentUI augmentUI;

    // Các mốc level cho lõi
    private readonly int[] augmentLevels = { 2, 5, 8, 10 };

    // Track level nào đã cho chọn rồi
    private HashSet<int> offeredLevels = new HashSet<int>();



    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            // New scene has a fresh AugmentManager with updated Inspector references.
            // Transfer those references to the persistent instance, then self-destruct.
            Instance.AbsorbFromNewScene(this);
            Destroy(gameObject);
        }
    }

    /// <summary>Called when a new scene's AugmentManager is absorbed into the persistent one.</summary>
    private void AbsorbFromNewScene(AugmentManager source)
    {
        augmentPoolLv2  = source.augmentPoolLv2;
        augmentPoolLv5  = source.augmentPoolLv5;
        augmentPoolLv8  = source.augmentPoolLv8;
        augmentPoolLv10 = source.augmentPoolLv10;
        augmentUI       = source.augmentUI;

        // Re-wire PlayerLevel from the new scene
        if (playerLevel != null)
            playerLevel.OnLevelUp -= HandleLevelUp;

        playerLevel = source.playerLevel;
        if (playerLevel == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerLevel = player.GetComponent<PlayerLevel>();
        }
        if (playerLevel != null)
            playerLevel.OnLevelUp += HandleLevelUp;

        if (augmentUI != null)
            augmentUI.Hide();
    }

    void Start()
    {
        // Only runs for the FIRST scene (persistent instance).
        // Subsequent scenes call AbsorbFromNewScene instead.
        if (playerLevel == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerLevel = player.GetComponent<PlayerLevel>();
        }

        if (playerLevel != null)
            playerLevel.OnLevelUp += HandleLevelUp;

        if (augmentUI != null)
            augmentUI.Hide();
    }

    void HandleLevelUp(int newLevel)
    {
        // Kiểm tra level này có trong danh sách cho lõi không
        if (!offeredLevels.Contains(newLevel) && IsAugmentLevel(newLevel))
        {
            offeredLevels.Add(newLevel);
            ShowAugmentSelection(newLevel);
        }
    }

    bool IsAugmentLevel(int level)
    {
        foreach (int lv in augmentLevels)
        {
            if (lv == level) return true;
        }
        return false;
    }

    /// <summary>
    /// Lấy pool lõi đúng theo level
    /// </summary>
    AugmentData[] GetPoolForLevel(int level)
    {
        switch (level)
        {
            case 2:  return augmentPoolLv2;
            case 5:  return augmentPoolLv5;
            case 8:  return augmentPoolLv8;
            case 10: return augmentPoolLv10;
            default: return null;
        }
    }

    void ShowAugmentSelection(int level)
    {
        AugmentData[] pool = GetPoolForLevel(level);

        if (pool == null || pool.Length == 0)
        {
            Debug.LogWarning($"[Augment] Không có pool lõi cho level {level}!");
            return;
        }

        // Pause game
        Time.timeScale = 0f;
        IsPanelVisible = true;

        // Hiện UI
        if (augmentUI != null)
        {
            augmentUI.Show(pool, OnAugmentSelected);
        }
    }

    /// <summary>
    /// Callback khi người chơi chọn 1 lõi
    /// </summary>
    void OnAugmentSelected(AugmentData selected)
    {
        Debug.Log($"[Augment] Đã chọn lõi: {selected.augmentName}");

        // Áp dụng buff lên nhân vật
        ApplyAugment(selected);

        // Record augment in SaveManager for next save
        SaveManager.Instance?.AddAppliedAugment(selected);

        // Ẩn UI
        if (augmentUI != null)
            augmentUI.Hide();

        // Tiếp tục game
        IsPanelVisible = false;
        Time.timeScale = 1f;

        // Auto-save after upgrade is applied
        if (PlayerSaveLoad.Instance != null) PlayerSaveLoad.Instance.SaveGame();
    }

    void ApplyAugment(AugmentData augment)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[Augment] Không tìm thấy Player!");
            return;
        }

        switch (augment.type)
        {
            // ===== LEVEL 2 & 10: Stats cơ bản =====
            case AugmentData.AugmentType.Health:
                PlayerHealth hp = player.GetComponent<PlayerHealth>();
                if (hp != null)
                {
                    hp.maxHealth += (int)augment.value;
                    hp.currentHealth += (int)augment.value;
                    Debug.Log($"[Augment] HP tối đa +{augment.value} → {hp.maxHealth}");
                }
                break;

            case AugmentData.AugmentType.Damage:
                PlayerMeleeAttack melee = player.GetComponent<PlayerMeleeAttack>();
                if (melee != null)
                {
                    melee.meleeDamage += (int)augment.value;
                    Debug.Log($"[Augment] Sát thương +{augment.value} → {melee.meleeDamage}");
                }
                break;

            case AugmentData.AugmentType.Speed:
                PlayerMovement move = player.GetComponent<PlayerMovement>();
                if (move != null)
                {
                    move.moveSpeed += augment.value;
                    Debug.Log($"[Augment] Tốc chạy +{augment.value} → {move.moveSpeed}");
                }
                break;

            // ===== LEVEL 5: Đổi Skin (Tint màu) =====
            case AugmentData.AugmentType.Skin:
                SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = augment.skinColor;
                    Debug.Log($"[Augment] Đổi màu nhân vật → {augment.skinColor}");
                }
                break;

            // ===== LEVEL 8: Buff nâng cao =====
            case AugmentData.AugmentType.ManaRegen:
                PlayerEnergy energy = player.GetComponent<PlayerEnergy>();
                if (energy != null)
                {
                    energy.regenRate += augment.value;
                    Debug.Log($"[Augment] Hồi mana +{augment.value}/s → {energy.regenRate}/s");
                }
                break;

            case AugmentData.AugmentType.AttackSpeed:
                PlayerMeleeAttack meleeAS = player.GetComponent<PlayerMeleeAttack>();
                if (meleeAS != null)
                {
                    // Giảm cooldown = đánh nhanh hơn (value là % giảm, ví dụ 0.3 = giảm 30%)
                    meleeAS.attackCooldown *= (1f - augment.value);
                    Debug.Log($"[Augment] Tốc đánh nhanh hơn! Cooldown → {meleeAS.attackCooldown:F2}s");
                }
                break;

            case AugmentData.AugmentType.TripleOrb:
                PlayerCastOrb castOrb = player.GetComponent<PlayerCastOrb>();
                if (castOrb != null)
                {
                    castOrb.isTripleOrbActive = true;
                    Debug.Log("[Augment] Kích hoạt bắn Orb 3 tia!");
                }
                break;
        }
    }

    /// <summary>
    /// Called on save-load to prevent re-triggering augment selection
    /// for levels the player already passed.
    /// </summary>
    public void SkipTriggersUpToLevel(int currentLevel)
    {
        foreach (int lv in augmentLevels)
        {
            if (lv <= currentLevel)
                offeredLevels.Add(lv);
        }
    }

    void OnDestroy()
    {
        // Only the persistent instance (Instance == this) should unsubscribe on quit.
        // Absorbed copies are destroyed before Start() runs so playerLevel is null anyway.
        if (Instance == this && playerLevel != null)
            playerLevel.OnLevelUp -= HandleLevelUp;
    }
}
