using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject gameOverCanvas;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Nhịp 3: Lời Phán Xét (Hiện Canvas & Đóng băng)
    /// </summary>
    public void ShowGameOver()
    {
        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(true);
        }
        
        // Đóng băng thế giới (Cực kỳ quan trọng)
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Nút "MENU": Rã đông thời gian -> Chuyển cảnh về Main Menu.
    /// </summary>
    public void OnMenuButtonClicked()
    {
        // ĐẦU TIÊN: Rã đông thời gian
        Time.timeScale = 1f;

        // Chuyển về Main Menu (Đảm bảo scene MainMenu đã được add trong Build Settings)
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Nút "TRY AGAIN": Rã đông thời gian -> Hệ thống sẽ load lại Map này một lần nữa.
    /// </summary>
    public void OnTryAgainButtonClicked()
    {
        // ĐẦU TIÊN: Rã đông thời gian
        Time.timeScale = 1f;

        // Reload lại Map hiện tại
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
