using UnityEngine;
using UnityEngine.Playables;
using System.Collections;

public class BossLanding : MonoBehaviour
{
    [Header("Setup")]
    public PlayableDirector bossTimeline;  // Timeline Camera
    public Animator bossCanvasAnimator;    // Kéo BossCanvas vào đây
    public GameObject bossCanvasObject;    // Kéo BossCanvas vào đây (để bật/tắt cả GameObject)
    public Dreamshaper player;             // Kéo nhân vật (Dreamshaper) vào đây
    public Rigidbody2D playerRb;           // Rigidbody của nhân vật
    public GameObject bossObject;          // Kéo con Boss vào đây
    // (BossCanvas phải bị tắt trong Inspector từ đầu)

    private bool daTiepDat = false;

    private void OnEnable()
    {
        // Tự động tìm Dreamshaper nếu chưa kéo vào Inspector
        if (player == null)
            player = FindFirstObjectByType<Dreamshaper>();

        if (playerRb == null && player != null)
            playerRb = player.GetComponent<Rigidbody2D>();

        // Khoá ngay khi boss kích hoạt (phòng trường hợp BossWakeUp bị null)
        if (player != null)
        {
            player.SetAllModulesEnabled(false);
            Debug.Log("[BossLanding] OnEnable: Đã khóa Player modules.");
        }
        else
        {
            Debug.LogWarning("[BossLanding] CHƯA tìm được Dreamshaper! Kéo vào field 'Player' trong Inspector.");
        }

        // Dừng vận tốc nhân vật
        if (playerRb != null) playerRb.linearVelocity = Vector2.zero;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") && !daTiepDat)
        {
            Debug.Log("[BossLanding] Boss chạm đất → Bắt đầu cutscene!");
            daTiepDat = true;
            StartCoroutine(QuyTrinhBossRaMat());
        }
    }

    IEnumerator QuyTrinhBossRaMat()
    {
        // 1. Chạy Timeline Camera
        if (bossTimeline != null)
        {
            Debug.Log($"[BossLanding] Chạy Timeline, duration = {bossTimeline.duration:F2}s");
            bossTimeline.Play();
            yield return new WaitForSeconds((float)bossTimeline.duration);
            Debug.Log("[BossLanding] Timeline xong!");
        }
        else
        {
            Debug.LogWarning("[BossLanding] bossTimeline là null, bỏ qua!");
        }

        // 2. Chạy Banner sau khi Timeline xong
        if (bossCanvasObject != null)
            bossCanvasObject.SetActive(true);

        if (bossCanvasAnimator != null)
        {
            bossCanvasAnimator.enabled = true;
            bossCanvasAnimator.Play("BossBannerAnim", 0, 0);
            Debug.Log("[BossLanding] Banner đang chạy (3s)...");
            yield return new WaitForSeconds(3f);
            Debug.Log("[BossLanding] Banner xong!");
        }

        // 3. MỞ KHÓA – BẮT ĐẦU ĐÁNH NHAU
        if (player != null)
        {
            player.SetAllModulesEnabled(true);
            Debug.Log("[BossLanding] ✅ Đã mở khóa toàn bộ Player modules → FIGHT!");
        }
        else
        {
            Debug.LogError("[BossLanding] ❌ player vẫn null → KHÔNG mở khóa được! Kéo Dreamshaper vào Inspector.");
        }

        // Reset velocity sau cutscene
        if (playerRb != null) playerRb.linearVelocity = Vector2.zero;
    }
}
