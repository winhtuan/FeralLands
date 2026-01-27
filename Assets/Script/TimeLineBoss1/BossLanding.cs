using UnityEngine;
using UnityEngine.Playables; // Thư viện Timeline

public class BossLanding : MonoBehaviour
{
    public PlayableDirector bossTimeline; // Kéo Timeline vào đây

    private bool daTiepDat = false;

    // Hàm này tự chạy khi va chạm mạnh
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Kiểm tra xem có đập vào đất không?
        if (collision.gameObject.CompareTag("Ground") && !daTiepDat)
        {
            daTiepDat = true;
            LandingAction();
        }
    }

    void LandingAction()
    {
        Debug.Log("Rầm! Boss đã tiếp đất");

        // Chạy Timeline Camera
        if (bossTimeline != null)
        {
            bossTimeline.Play();
        }
    }
}