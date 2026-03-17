using UnityEngine;

/// <summary>
/// Định nghĩa một Lõi (Augment) — ScriptableObject để dễ tạo data trên Inspector.
/// </summary>
[CreateAssetMenu(fileName = "NewAugment", menuName = "FeralLands/Augment")]
public class AugmentData : ScriptableObject
{
    public string augmentName;           // Tên lõi
    [TextArea] public string description; // Mô tả lõi
    public Sprite icon;                   // Icon đại diện cho lõi
    public AugmentType type;              // Loại buff
    public float value;                   // Giá trị buff

    [Header("Skin Settings — Chỉ dùng khi type = Skin")]
    public Color skinColor = Color.white; // Màu tint cho nhân vật

    public enum AugmentType
    {
        Health,         // Tăng máu tối đa
        Damage,         // Tăng sát thương cận chiến
        Speed,          // Tăng tốc độ di chuyển
        Skin,           // Đổi màu nhân vật (tint SpriteRenderer)
        ManaRegen,      // Tăng tốc hồi mana
        AttackSpeed,    // Giảm cooldown đánh cận chiến (tốc đánh nhanh hơn)
        TripleOrb       // Bắn ra 3 tia orb
    }
}
