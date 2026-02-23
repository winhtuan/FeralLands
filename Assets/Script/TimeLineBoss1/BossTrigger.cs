//using UnityEngine;
//using UnityEngine.Playables; // Thư viện Timeline

//public class BossIntroTrigger : MonoBehaviour
//{
//    public PlayableDirector bossTimeline; // Kéo BossDirector vào đây
//    public GameObject uiThanhMauBoss;     // Kéo UI máu boss vào (nếu có)

//    // Biến để chặn việc chạy lại 2 lần
//    private bool daChayIntro = false;

//    private void OnTriggerEnter2D(Collider2D other)
//    {
//        // Chỉ chạy khi Player chạm vào VÀ chưa chạy lần nào
//        if (other.CompareTag("Player") && !daChayIntro)
//        {
//            daChayIntro = true; // Đánh dấu là đã chạy xong

//            // Chạy Timeline
//            bossTimeline.Play();

//            // Hiện máu boss (nếu muốn hiện ngay lúc camera quay boss)
//            // if (uiThanhMauBoss != null) uiThanhMauBoss.SetActive(true);
//        }
//    }
//}


using UnityEngine;
using UnityEngine.Playables; // Thư viện Timeline

public class BossTrigger : MonoBehaviour
{
    [Header("Cài đặt Cutscene")]
    public PlayableDirector bossTimeline; // Kéo BossDirector vào đây
    public GameObject player;             // Kéo nhân vật Dreamshaper vào đây
    public GameObject uiThanhMauBoss;     // Kéo UI máu vào (nếu có)

    private bool daKichHoat = false;

    // Thay 'Dreamshaper' bằng tên script di chuyển thật của bạn (VD: Movement, PlayerController...)
    // Nếu bạn không biết tên, hãy chụp ảnh Inspector của nhân vật Dreamshaper gửi tôi xem.
    private MonoBehaviour scriptDiChuyen;
    private Rigidbody2D rbPlayer;

    void Start()
    {
        // Tự động tìm script di chuyển trên người nhân vật (Thay Dreamshaper bằng tên đúng)
        // Ví dụ: scriptDiChuyen = player.GetComponent<Dreamshaper>();
        // Tạm thời tôi dùng Rigidbody để phanh nhân vật lại
        if (player != null)
            rbPlayer = player.GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !daKichHoat)
        {
            daKichHoat = true;
            BatDauCutscene();
        }
    }

    void BatDauCutscene()
    {
        // 1. Khóa di chuyển
        // Nếu bạn có script di chuyển, hãy uncomment dòng dưới:
        // if (scriptDiChuyen != null) scriptDiChuyen.enabled = false;

        // Phanh nhân vật lại ngay lập tức (tránh trượt)
        if (rbPlayer != null) rbPlayer.linearVelocity = Vector2.zero;

        // 2. Chạy Timeline
        if (bossTimeline != null)
        {
            bossTimeline.Play();
            // Đăng ký sự kiện: Khi Timeline chạy xong thì gọi hàm KetThucCutscene
            bossTimeline.stopped += KetThucCutscene;
        }
    }

    // Hàm này tự động chạy khi Timeline kết thúc
    void KetThucCutscene(PlayableDirector director)
    {
        // 1. Mở khóa di chuyển
        // if (scriptDiChuyen != null) scriptDiChuyen.enabled = true;

        // 2. Hiện máu Boss
        if (uiThanhMauBoss != null) uiThanhMauBoss.SetActive(true);

        Debug.Log("Đánh Boss thôi!");
    }
}