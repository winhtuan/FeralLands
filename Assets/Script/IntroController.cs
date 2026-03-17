using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class IntroController : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Kéo component Video Player vào đây. Nếu để trống, script sẽ tự tìm trên GameObject.")]
    public VideoPlayer videoPlayer;

    [Tooltip("Tên chính xác của Scene muốn chuyển đến sau khi Intro kết thúc.")]
    public string nextSceneName = "MapBeach 1";

    void Start()
    {
        // Tự động tìm VideoPlayer nếu chưa gán
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer != null)
        {
            // Đăng ký sự kiện: Video hết thì chuyển cảnh
            videoPlayer.loopPointReached += OnVideoFinished;
        }
        else
        {
            Debug.LogError("IntroController: Không tìm thấy Video Player trên GameObject này!");
        }
    }

    void Update()
    {
        // Nhấn phím bất kỳ để bỏ qua Intro
        if (Input.anyKeyDown)
        {
            LoadNextScene();
        }
    }

    // Sự kiện được gọi tự động khi Video chạy hết
    void OnVideoFinished(VideoPlayer vp)
    {
        LoadNextScene();
    }

    void LoadNextScene()
    {
        // Hủy đăng ký để tránh lỗi memory leak hoặc gọi 2 lần
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoFinished;

        // Kiểm tra xem scene có tồn tại trong Build Settings không trước khi load (tránh crash)
        if (Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError($"IntroController: Không tìm thấy Scene '{nextSceneName}'. Hãy kiểm tra tên hoặc thêm vào Build Settings.");
            // Fallback: Load về lại MainMenu nếu lỗi
            SceneManager.LoadScene("MainMenu");
        }
    }
}
