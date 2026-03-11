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

    public enum AugmentType
    {
        Health,     // Tăng máu tối đa
        Damage,     // Tăng sát thương cận chiến
        Speed       // Tăng tốc độ di chuyển
    }
}
