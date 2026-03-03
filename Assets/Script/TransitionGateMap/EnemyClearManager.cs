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

        // Tìm tất cả EnemyBase trong scene
        List<EnemyBase> enemyBases = new List<EnemyBase>(
            FindObjectsByType<EnemyBase>(FindObjectsSortMode.None)
        );

        // Tìm tất cả NormalEnemyBase trong scene
        List<NormalEnemyBase> normalEnemyBases = new List<NormalEnemyBase>(
            FindObjectsByType<NormalEnemyBase>(FindObjectsSortMode.None)
        );

        totalEnemies = enemyBases.Count + normalEnemyBases.Count;
        enemiesRemaining = totalEnemies;

        if (totalEnemies == 0)
        {
            Debug.LogWarning("[EnemyClearManager] Không tìm thấy quái nào trong scene! Portal sẽ mở ngay.");
            ShowPortal();
            return;
        }

        Debug.Log($"[EnemyClearManager] Tổng số quái: {totalEnemies}. Portal bị ẩn cho đến khi tiêu diệt hết.");

        // Subscribe vào event OnDeath của từng EnemyBase
        foreach (var enemy in enemyBases)
        {
            enemy.OnDeath += OnEnemyDied;
        }

        // Subscribe vào event OnDeath của từng NormalEnemyBase
        foreach (var enemy in normalEnemyBases)
        {
            enemy.OnDeath += OnEnemyDied;
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
    }
}
