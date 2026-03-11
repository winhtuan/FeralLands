using UnityEngine;

/// <summary>
/// AugmentManager — Quản lý logic chọn Lõi khi nhân vật lên cấp 2.
/// Gắn lên một GameObject bất kỳ trong Scene (ví dụ: GameManager).
/// </summary>
public class AugmentManager : MonoBehaviour
{
    [Header("Augment Pool — Kéo 3 Lõi ScriptableObject vào đây")]
    public AugmentData[] augmentPool; // Kéo 3 AugmentData vào Inspector

    [Header("References")]
    public PlayerLevel playerLevel;
    public AugmentUI augmentUI;

    private bool hasOfferedAugment = false; // Chỉ cho chọn 1 lần khi lên cấp 2

    void Start()
    {
        // Tự tìm PlayerLevel nếu chưa gán
        if (playerLevel == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerLevel = player.GetComponent<PlayerLevel>();
        }

        if (playerLevel != null)
        {
            playerLevel.OnLevelUp += HandleLevelUp;
        }

        // Ẩn UI ban đầu
        if (augmentUI != null)
            augmentUI.Hide();
    }

    void HandleLevelUp(int newLevel)
    {
        // Chỉ hiện chọn Lõi khi lên cấp 2 và chưa chọn
        if (newLevel == 2 && !hasOfferedAugment)
        {
            hasOfferedAugment = true;
            ShowAugmentSelection();
        }
    }

    void ShowAugmentSelection()
    {
        // Pause game
        Time.timeScale = 0f;

        // Hiện UI với 3 lõi
        if (augmentUI != null)
        {
            augmentUI.Show(augmentPool, OnAugmentSelected);
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

        // Ẩn UI
        if (augmentUI != null)
            augmentUI.Hide();

        // Tiếp tục game
        Time.timeScale = 1f;
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
            case AugmentData.AugmentType.Health:
                PlayerHealth hp = player.GetComponent<PlayerHealth>();
                if (hp != null)
                {
                    hp.maxHealth += (int)augment.value;
                    hp.currentHealth += (int)augment.value; // Hồi thêm luôn
                    Debug.Log($"[Augment] HP tối đa tăng thêm {augment.value} → {hp.maxHealth}");
                }
                break;

            case AugmentData.AugmentType.Damage:
                PlayerMeleeAttack melee = player.GetComponent<PlayerMeleeAttack>();
                if (melee != null)
                {
                    melee.meleeDamage += (int)augment.value;
                    Debug.Log($"[Augment] Sát thương tăng thêm {augment.value} → {melee.meleeDamage}");
                }
                break;

            case AugmentData.AugmentType.Speed:
                PlayerMovement move = player.GetComponent<PlayerMovement>();
                if (move != null)
                {
                    move.moveSpeed += augment.value;
                    Debug.Log($"[Augment] Tốc độ tăng thêm {augment.value} → {move.moveSpeed}");
                }
                break;
        }
    }

    void OnDestroy()
    {
        if (playerLevel != null)
            playerLevel.OnLevelUp -= HandleLevelUp;
    }
}
