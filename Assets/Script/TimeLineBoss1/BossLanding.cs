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
        // ... (giữ nguyên đoạn tìm Player)
        if (player == null) player = FindFirstObjectByType<Dreamshaper>();
        if (playerRb == null && player != null) playerRb = player.GetComponent<Rigidbody2D>();

        if (player != null) player.SetAllModulesEnabled(false);
        if (playerRb != null) playerRb.linearVelocity = Vector2.zero;

        // Đảm bảo boss không đánh khi đang intro (Tìm qua Interface)
        IBossController boss = GetComponent<IBossController>() ?? GetComponentInChildren<IBossController>();
        if (boss != null) boss.isBattleStarted = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") && !daTiepDat)
        {
            daTiepDat = true;
            StartCoroutine(QuyTrinhBossRaMat());
        }
    }

    IEnumerator QuyTrinhBossRaMat()
    {
        // ... (giữ nguyên đoạn Timeline và Banner)
        if (bossTimeline != null)
        {
            bossTimeline.Play();
            yield return new WaitForSeconds((float)bossTimeline.duration);
        }

        if (bossCanvasObject != null) bossCanvasObject.SetActive(true);

        if (bossCanvasAnimator != null)
        {
            bossCanvasAnimator.enabled = true;
            bossCanvasAnimator.Play("BossBannerAnim", 0, 0);
            yield return new WaitForSeconds(3f);
        }

        // MỞ KHÓA PLAYER
        if (player == null) player = FindFirstObjectByType<Dreamshaper>();
        if (player != null) player.SetAllModulesEnabled(true);

        // KÍCH HOẠT BOSS QUA INTERFACE
        if (bossObject != null)
        {
            IBossController boss = bossObject.GetComponent<IBossController>() ?? bossObject.GetComponentInChildren<IBossController>();
            if (boss != null)
            {
                boss.isBattleStarted = true;
                boss.enabled = true;
                Debug.Log($"[BossLanding] ✅ Đã kích hoạt Boss: {bossObject.name}");
            }
            else
            {
                Debug.LogError("[BossLanding] ❌ KHÔNG tìm thấy thành phần IBossController trên bossObject!");
            }
        }
        
        if (playerRb != null) playerRb.linearVelocity = Vector2.zero;
        yield break;
    }
}
