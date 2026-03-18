using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Lưu vị trí của từng enemy trong scene (dùng để biết enemy nào còn sống, ở đâu)
[System.Serializable]
public class EnemyPositionData
{
    public string enemyID; // ID duy nhất của enemy
    public float posX; // Tọa độ X
    public float posY; // Tọa độ Y
}

// Lưu thông tin một augment (nâng cấp) mà player đã chọn
[System.Serializable]
public class AugmentSaveEntry
{
    public int augmentID; // ID số của loại augment (ép kiểu từ enum AugmentType)
    public string statType; // Tên loại augment: "Health", "Damage", "Speed", "Skin"...
    public float value; // Giá trị bonus của augment (ví dụ: +20 HP, +5 damage)

    // Màu skin — chỉ dùng khi statType == "Skin"
    public float skinR,
        skinG,
        skinB,
        skinA;
}

// Toàn bộ dữ liệu của một lần save game
[System.Serializable]
public class GameData
{
    // --- Trạng thái player ---
    public int level; // Cấp độ hiện tại
    public int score; // Exp hiện tại (dùng tên "score" để tương thích cũ)
    public float playerHealth; // Máu hiện tại
    public float playerPosX; // Tọa độ X của player
    public float playerPosY; // Tọa độ Y của player
    public string currentSceneName; // Tên scene đang ở (dùng để kiểm tra khi load vị trí)

    // --- Augment đã áp dụng ---
    // Danh sách đầy đủ các augment player đã chọn (thay thế mảng int cũ)
    public List<AugmentSaveEntry> appliedAugments = new List<AugmentSaveEntry>();

    // --- Trạng thái enemy ---
    public List<string> killedEnemyIDs = new List<string>(); // ID các enemy đã chết
    public List<EnemyPositionData> enemyPositions = new List<EnemyPositionData>(); // Vị trí enemy còn sống

    // --- Cài đặt âm thanh / hiển thị ---
    // Bản sao từ PlayerPrefs để lưu vào file, bản gốc vẫn nằm trong PlayerPrefs
    public float musicVolume; // Âm lượng nhạc nền
    public float sfxVolume; // Âm lượng hiệu ứng
    public bool isFullscreen; // Toàn màn hình hay không
}

public class SaveManager : MonoBehaviour
{
    // Singleton — truy cập toàn cục qua SaveManager.Instance
    public static SaveManager Instance;

    // Đường dẫn file save: thư mục persistent của Unity + tên file
    // Ví dụ trên Windows: C:/Users/[tên]/AppData/LocalLow/[Company]/[Game]/save.json
    private static string SavePath => Application.persistentDataPath + "/save.json";

    // --- Cache trong RAM (không cần đọc file mỗi lần) ---
    private HashSet<string> _killedThisSession = new HashSet<string>(); // Tập hợp ID enemy đã chết trong phiên này
    private List<AugmentSaveEntry> _appliedAugments = new List<AugmentSaveEntry>(); // Augment đã chọn trong phiên này
    private GameData _cachedData; // GameData lần cuối đọc/ghi

    // Tự động tạo SaveManager TRƯỚC KHI scene đầu tiên load
    // Đảm bảo luôn tồn tại kể cả khi bắt đầu từ bất kỳ scene nào (trong Editor)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreate()
    {
        if (Instance != null)
            return; // Đã tồn tại rồi thì thôi
        var go = new GameObject("SaveManager");
        Instance = go.AddComponent<SaveManager>();
        DontDestroyOnLoad(go); // Không bị xóa khi chuyển scene
    }

    private void Awake()
    {
        // Singleton pattern: chỉ cho phép 1 instance tồn tại
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // Xóa bản thừa nếu có
        }
    }

    // ── Theo dõi enemy đã chết ──────────────────────────────────────────────────

    // Đánh dấu enemy đã chết (gọi từ EnemyBase khi chết)
    public void MarkEnemyKilled(string id) => _killedThisSession.Add(id);

    // Kiểm tra enemy có đã chết chưa (gọi từ NormalEnemyBase.Start() để tự hủy nếu đã chết)
    public bool WasEnemyKilled(string id) => _killedThisSession.Contains(id);

    // ── Theo dõi augment đã chọn ────────────────────────────────────────────────

    /// <summary>Gọi từ AugmentManager mỗi khi player chọn một augment.</summary>
    public void AddAppliedAugment(AugmentData augData)
    {
        // Chuyển AugmentData (ScriptableObject) thành entry đơn giản để lưu file
        _appliedAugments.Add(
            new AugmentSaveEntry
            {
                augmentID = (int)augData.type,
                statType = augData.type.ToString(),
                value = augData.value,
                skinR = augData.skinColor.r,
                skinG = augData.skinColor.g,
                skinB = augData.skinColor.b,
                skinA = augData.skinColor.a,
            }
        );
    }

    // Trả về danh sách augment đã chọn (dùng khi load để áp lại stat)
    public List<AugmentSaveEntry> GetAppliedAugments() => _appliedAugments;

    // ── Lưu / Tải ───────────────────────────────────────────────────────────────

    public void SaveGame(GameData data)
    {
        // Gộp dữ liệu từ cache RAM vào data trước khi ghi
        data.killedEnemyIDs = new List<string>(_killedThisSession);
        data.appliedAugments = new List<AugmentSaveEntry>(_appliedAugments);

        // Ghi cài đặt vào PlayerPrefs (registry/plist — nhanh, không cần file)
        PlayerPrefs.SetFloat("Setting_Music", data.musicVolume);
        PlayerPrefs.SetFloat("Setting_SFX", data.sfxVolume);
        PlayerPrefs.SetInt("Setting_Fullscreen", data.isFullscreen ? 1 : 0);
        PlayerPrefs.Save(); // Flush xuống đĩa ngay

        // Chuyển GameData thành chuỗi JSON rồi ghi ra file save.json
        string json = JsonUtility.ToJson(data, true); // true = pretty print (dễ đọc)
        File.WriteAllText(SavePath, json);

        _cachedData = data; // Cập nhật cache để GetCachedData() không trả dữ liệu cũ
        Debug.Log("[SaveManager] Saved to: " + SavePath);
    }

    public GameData LoadGame()
    {
        // Không có file save → trả về null (game sẽ dùng giá trị mặc định)
        if (!File.Exists(SavePath))
        {
            Debug.Log("[SaveManager] No save file found at: " + SavePath);
            return null;
        }

        try
        {
            // Đọc JSON từ file và parse thành GameData
            string json = File.ReadAllText(SavePath);
            GameData data = JsonUtility.FromJson<GameData>(json);

            // Nạp lại cache RAM từ dữ liệu vừa đọc
            _killedThisSession.Clear();
            if (data.killedEnemyIDs != null)
                foreach (string id in data.killedEnemyIDs)
                    _killedThisSession.Add(id); // HashSet tự loại trùng

            _appliedAugments.Clear();
            if (data.appliedAugments != null)
                _appliedAugments.AddRange(data.appliedAugments);

            _cachedData = data;
            Debug.Log(
                $"[SaveManager] Loaded — {_killedThisSession.Count} killed enemies, "
                    + $"{_appliedAugments.Count} augments, {data.enemyPositions?.Count ?? 0} enemy positions"
            );
            return data;
        }
        catch (Exception e)
        {
            // File bị hỏng / sai format → báo lỗi, không crash game
            Debug.LogWarning("[SaveManager] Failed to read save file: " + e.Message);
            return null;
        }
    }

    /// <summary>Trả về GameData đã load lần cuối mà KHÔNG đọc lại file.
    /// Dùng khi cần data ngay trong Awake() mà không muốn đọc đĩa lần nữa.</summary>
    public GameData GetCachedData() => _cachedData;

    // Kiểm tra file save có tồn tại không (dùng ở MainMenu để hiện nút "Continue")
    public static bool HasSave() => File.Exists(SavePath);

    public void DeleteSave()
    {
        // Xóa file save.json
        if (File.Exists(SavePath))
            File.Delete(SavePath);

        // Xóa toàn bộ cache RAM
        _killedThisSession.Clear();
        _appliedAugments.Clear();
        _cachedData = null;

        // Xóa cài đặt trong PlayerPrefs
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[SaveManager] Save deleted.");
    }
}
