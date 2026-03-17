using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("Kéo TopBar và BottomBar vào đây")]
    public RectTransform topBar;
    public RectTransform bottomBar;

    public float transitionDuration = 1f;
    private float screenHalfHeight;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ Canvas này không bị hủy khi qua map mới
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Đảm bảo timeScale luôn được reset khi scene mới load
        Time.timeScale = 1f;
        screenHalfHeight = Screen.height / 2f;
    }

    private void Start()
    {
        screenHalfHeight = Screen.height / 2f;
    }

    public void TeleportToMap(string sceneName)
    {
        Time.timeScale = 1f; // Đảm bảo không bị stuck ở timeScale=0

        // Save before leaving a gameplay scene so stats survive the transition.
        // Skip when going to MainMenu — the Save button already saved with the
        // correct scene name; overwriting here would set currentSceneName = "MainMenu".
        if (sceneName != "MainMenu" && PlayerSaveLoad.Instance != null)
            PlayerSaveLoad.Instance.SaveGameForScene(sceneName);

        gameObject.SetActive(true);
        StartCoroutine(CinematicTransition(sceneName));
    }

    private IEnumerator CinematicTransition(string sceneName)
    {
        // 1. Khép màn hình lại
        float elapsedTime = 0f;
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;
            t = t * t * (3f - 2f * t);

            if (topBar != null) topBar.anchoredPosition = new Vector2(0, Mathf.Lerp(screenHalfHeight, 0, t));
            if (bottomBar != null) bottomBar.anchoredPosition = new Vector2(0, Mathf.Lerp(-screenHalfHeight, 0, t));
            yield return null;
        }

        // 2. Load Scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        if (asyncLoad == null)
        {
            Debug.LogError($"[SceneTransitionManager] Không thể load scene '{sceneName}'! " +
                           $"Hãy kiểm tra: (1) Tên scene đúng chính xác, (2) Scene đã được thêm vào Build Settings (File → Build Settings).");
            gameObject.SetActive(false);
            yield break; // Dừng coroutine, không crash
        }

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 3. Mở màn hình ra
        elapsedTime = 0f;
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;
            t = t * t * (3f - 2f * t);

            if (topBar != null) topBar.anchoredPosition = new Vector2(0, Mathf.Lerp(0, screenHalfHeight, t));
            if (bottomBar != null) bottomBar.anchoredPosition = new Vector2(0, Mathf.Lerp(0, -screenHalfHeight, t));
            yield return null;
        }

        gameObject.SetActive(false);
    }
}