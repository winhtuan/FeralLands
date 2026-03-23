using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Component này gắn trên GameObject của Player (cùng chỗ với PlayerHealth, PlayerLevel...)
// Nhiệm vụ:
//   - KHI LOAD: đọc dữ liệu từ SaveManager rồi áp vào các module của player
//   - KHI SAVE: thu thập dữ liệu từ các module rồi đẩy vào SaveManager để ghi file
public class PlayerSaveLoad : MonoBehaviour
{
    public static PlayerSaveLoad Instance { get; private set; }

    private PlayerHealth health; // Module máu của player
    private PlayerLevel level; // Module cấp độ / exp của player

    // Lưu tạm dữ liệu đọc từ file để dùng tiếp trong Start()
    // (không thể áp hết trong Awake vì các module khác chưa Awake xong)
    private GameData _loadedData;

    void Awake()
    {
        Instance = this;

        // Lấy tham chiếu đến các module trên cùng GameObject
        health = GetComponent<PlayerHealth>();
        level = GetComponent<PlayerLevel>();

        // Nếu SaveManager chưa tồn tại thì bỏ qua (game mới, chưa có save)
        if (SaveManager.Instance == null)
            return;

        // Ưu tiên lấy từ cache RAM (nhanh hơn đọc file),
        // nếu cache trống mới đọc file save.json
        _loadedData = SaveManager.Instance.GetCachedData() ?? SaveManager.Instance.LoadGame();

        if (_loadedData == null)
            return; // Không có save → dùng giá trị mặc định, thôi không làm gì

        // Áp bonus augment (tăng maxHP, damage, speed...) NGAY TRONG AWAKE
        // vì bước tiếp theo (Start) sẽ dùng maxHealth đã augment để clamp currentHealth
        ApplyAugmentStats(_loadedData.appliedAugments);
    }

    void Start()
    {
        // Tất cả Awake() của mọi script đã chạy xong → an toàn để ghi đè currentHealth
        // (PlayerHealth.Awake() đã reset currentHealth = maxHealth trước đó rồi)

        if (_loadedData == null)
            return;

        // 1. Khôi phục máu hiện tại — không được vượt quá maxHealth (đã tính augment)
        if (health != null)
            health.currentHealth = Mathf.Clamp((int)_loadedData.playerHealth, 1, health.maxHealth);

        // 2. Khôi phục cấp độ và exp
        if (level != null)
        {
            level.currentLevel = Mathf.Max(1, _loadedData.level); // Tối thiểu cấp 1
            level.currentExp = _loadedData.score;
        }

        // 3. Khôi phục vị trí player
        // Điều kiện: phải đang ở đúng scene đã lưu VÀ tọa độ khác (0,0)
        // Nếu tọa độ = (0,0) nghĩa là save khi chuyển scene → dùng spawn point thay thế
        if (
            _loadedData.currentSceneName == SceneManager.GetActiveScene().name
            && (_loadedData.playerPosX != 0f || _loadedData.playerPosY != 0f)
        )
        {
            transform.position = new Vector3(
                _loadedData.playerPosX,
                _loadedData.playerPosY,
                transform.position.z // Giữ nguyên Z (không ảnh hưởng 2D)
            );
            Debug.Log(
                $"[Load] Khôi phục vị trí: ({_loadedData.playerPosX}, {_loadedData.playerPosY})"
            );
        }
        else
        {
            // Scene khác hoặc tọa độ sentinel (0,0) → đứng ở spawn point mặc định
            Debug.Log($"[Load] Scene '{SceneManager.GetActiveScene().name}' — dùng spawn point.");
        }

        // 4. Khôi phục âm lượng nhạc và hiệu ứng
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(_loadedData.musicVolume);
            AudioManager.Instance.SetSFXVolume(_loadedData.sfxVolume);
        }

        // 5. Báo cho AugmentManager bỏ qua các mốc augment đã unlock
        // (ví dụ player cấp 6, thì mốc level 2 và 5 không được kích hoạt lại)
        if (AugmentManager.Instance != null)
            AugmentManager.Instance.SkipTriggersUpToLevel(_loadedData.level);

        Debug.Log(
            $"[Load] Hoàn tất — Cấp {_loadedData.level}, "
                + $"HP {health?.currentHealth}/{health?.maxHealth}, "
                + $"Augments: {_loadedData.appliedAugments?.Count ?? 0}"
        );

        _loadedData = null; // Giải phóng bộ nhớ, không cần nữa
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // ── Áp lại stat từ augment đã lưu ─────────────────────────────────────────

    private void ApplyAugmentStats(List<AugmentSaveEntry> entries)
    {
        if (entries == null || entries.Count == 0)
            return;

        // Lấy tham chiếu các module cần chỉnh
        PlayerMeleeAttack melee = GetComponent<PlayerMeleeAttack>();
        PlayerMovement movement = GetComponent<PlayerMovement>();
        PlayerEnergy energy = GetComponent<PlayerEnergy>();
        PlayerCastOrb castOrb = GetComponent<PlayerCastOrb>();
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        // Reset về giá trị gốc (trong Inspector) trước khi cộng augment
        // → tránh cộng 2 lần nếu hàm này bị gọi nhiều lần
        if (health != null)
            health.maxHealth = health.baseMaxHealth;
        if (melee != null)
            melee.meleeDamage = melee.baseMeleeDamage;
        if (movement != null)
            movement.moveSpeed = movement.baseMoveSpeed;

        // Duyệt từng augment đã lưu và cộng bonus tương ứng
        foreach (var e in entries)
        {
            switch (e.statType)
            {
                case "Health": // Tăng máu tối đa
                    if (health != null)
                        health.maxHealth += (int)e.value;
                    break;

                case "Damage": // Tăng sát thương cận chiến
                    if (melee != null)
                        melee.meleeDamage += (int)e.value;
                    break;

                case "Speed": // Tăng tốc độ di chuyển
                    if (movement != null)
                        movement.moveSpeed += e.value;
                    break;

                case "Skin": // Đổi màu nhân vật
                    if (sr != null)
                        sr.color = new Color(e.skinR, e.skinG, e.skinB, e.skinA);
                    break;

                case "ManaRegen": // Tăng tốc độ hồi năng lượng
                    if (energy != null)
                        energy.regenRate += e.value;
                    break;

                case "AttackSpeed": // Giảm thời gian hồi chiêu cận chiến (e.value là % giảm)
                    if (melee != null)
                        melee.attackCooldown *= (1f - e.value);
                    break;

                case "TripleOrb": // Mở khóa kỹ năng bắn 3 orb cùng lúc
                    if (castOrb != null)
                        castOrb.isTripleOrbActive = true;
                    break;
            }
        }
    }

    // ── Thu thập dữ liệu player để chuẩn bị save ──────────────────────────────

    private GameData CollectGameData()
    {
        var data = new GameData
        {
            currentSceneName = SceneManager.GetActiveScene().name, // Tên scene hiện tại
            playerPosX = transform.position.x,
            playerPosY = transform.position.y,
            playerHealth = health != null ? health.currentHealth : 0,
            level = level != null ? level.currentLevel : 1,
            score = level != null ? level.currentExp : 0,

            // Đọc cài đặt từ PlayerPrefs (nguồn chính xác nhất)
            musicVolume = PlayerPrefs.GetFloat("Setting_Music", 0.75f),
            sfxVolume = PlayerPrefs.GetFloat("Setting_SFX", 0.75f),
            isFullscreen = PlayerPrefs.GetInt("Setting_Fullscreen", 1) == 1,
        };

        // Thu thập vị trí tất cả enemy còn sống trong scene
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

    // ── Public save methods ────────────────────────────────────────────────────

    /// <summary>Lưu tại chỗ — scene name = scene hiện tại, vị trí = vị trí hiện tại.</summary>
    public void SaveGame()
    {
        if (SaveManager.Instance == null)
            return;
        SaveManager.Instance.SaveGame(CollectGameData());
    }

    /// <summary>
    /// Lưu trước khi chuyển scene.
    /// Ghi destinationScene vào currentSceneName và đặt tọa độ = (0,0) làm sentinel
    /// → khi load ở scene mới sẽ dùng spawn point thay vì tọa độ cũ.
    /// </summary>
    public void SaveGameForScene(string destinationScene)
    {
        if (SaveManager.Instance == null)
            return;

        GameData data = CollectGameData();
        data.currentSceneName = destinationScene; // Ghi tên scene SẼ ĐẾN, không phải scene hiện tại
        data.playerPosX = 0f; // Sentinel: "không có vị trí cụ thể"
        data.playerPosY = 0f;
        data.enemyPositions = new List<EnemyPositionData>(); // Vị trí enemy cũ không còn dùng được

        SaveManager.Instance.SaveGame(data);
        Debug.Log($"[Save] Chuyển sang '{destinationScene}' — đã lưu stat, xóa vị trí cũ.");
    }

    // Gọi từ nút Save trên UI
    public void OnSaveButtonClicked() => SaveGame();
}
