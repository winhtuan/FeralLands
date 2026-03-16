using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gắn script này vào một Empty GameObject trong scene.
/// Kéo Gate_Attack_0 (portal) vào ô "Portal Object" trong Inspector.
/// Portal sẽ bị ẩn khi bắt đầu, và chỉ hiện lại khi tất cả quái bị tiêu diệt.
/// </summary>
public class EnemyClearManager : MonoBehaviour
{
    [Header("Kéo Gate_Attack_0 vào đây")]
    public GameObject portalObject;

    [Header("Thông tin (chỉ để xem, không cần điền)")]
    [SerializeField] private int totalEnemies = 0;
    [SerializeField] private int enemiesRemaining = 0;

    private void Start()
    {
        // Ẩn cổng ngay khi bắt đầu
        if (portalObject != null)
            portalObject.SetActive(false);
        else
            Debug.LogWarning("[EnemyClearManager] Chưa gán Portal Object trong Inspector!");

        // Tìm tất cả EnemyBase bao gồm cả những đối tượng đang ẩn (vd: Boss khi chưa kích hoạt)
        List<EnemyBase> enemyBases = new List<EnemyBase>(
            FindObjectsByType<EnemyBase>(FindObjectsInactive.Include, FindObjectsSortMode.None)
        );

        // Tìm tất cả NormalEnemyBase bao gồm cả ẩn
        List<NormalEnemyBase> normalEnemyBases = new List<NormalEnemyBase>(
            FindObjectsByType<NormalEnemyBase>(FindObjectsInactive.Include, FindObjectsSortMode.None)
        );

        // Tìm tất cả BossHealth cụ thể (phòng trường hợp Boss không kế thừa đúng hoặc cần đếm riêng)
        List<BossHealth> bossHealths = new List<BossHealth>(
            FindObjectsByType<BossHealth>(FindObjectsInactive.Include, FindObjectsSortMode.None)
        );

        // Lọc trùng lặp vì BossHealth kế thừa từ EnemyBase
        HashSet<MonoBehaviour> uniqueEnemies = new HashSet<MonoBehaviour>();
        foreach (var e in enemyBases) uniqueEnemies.Add(e);
        foreach (var e in normalEnemyBases) uniqueEnemies.Add(e);
        foreach (var b in bossHealths) uniqueEnemies.Add(b);

        totalEnemies = uniqueEnemies.Count;
        enemiesRemaining = totalEnemies;

        if (totalEnemies == 0)
        {
            Debug.LogWarning("[EnemyClearManager] Không tìm thấy quái hoặc Boss nào trong scene! Portal sẽ mở ngay.");
            ShowPortal();
            return;
        }

        Debug.Log($"[EnemyClearManager] Tổng số mục tiêu (bao gồm Boss): {totalEnemies}. Portal bị ẩn cho đến khi tiêu diệt hết.");

        // Đăng ký sự kiện OnDeath cho từng đối tượng
        foreach (var target in uniqueEnemies)
        {
            if (target is EnemyBase eb) eb.OnDeath += OnEnemyDied;
            else if (target is NormalEnemyBase neb) neb.OnDeath += OnEnemyDied;
        }
    }

    private void OnEnemyDied()
    {
        enemiesRemaining--;
        Debug.Log($"[EnemyClearManager] Quái chết! Còn lại: {enemiesRemaining}/{totalEnemies}");

        if (enemiesRemaining <= 0)
        {
            ShowPortal();
        }
    }

    private void ShowPortal()
    {
        if (portalObject != null)
        {
            portalObject.SetActive(true);
            Debug.Log("[EnemyClearManager] Tất cả quái đã bị tiêu diệt! Portal xuất hiện.");
        }

        // Auto-save progress when the area is cleared
        PlayerSaveLoad.Instance?.SaveGame();
    }
}
