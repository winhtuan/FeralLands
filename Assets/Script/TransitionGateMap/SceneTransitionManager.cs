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
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        screenHalfHeight = Screen.height / 2f;
    }

    public void TeleportToMap(string sceneName)
    {
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
            t = t * t * (3f - 2f * t); // Làm mượt chuyển động

            topBar.anchoredPosition = new Vector2(0, Mathf.Lerp(screenHalfHeight, 0, t));
            bottomBar.anchoredPosition = new Vector2(0, Mathf.Lerp(-screenHalfHeight, 0, t));
            yield return null;
        }

        // 2. Load Scene Boss
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
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

            topBar.anchoredPosition = new Vector2(0, Mathf.Lerp(0, screenHalfHeight, t));
            bottomBar.anchoredPosition = new Vector2(0, Mathf.Lerp(0, -screenHalfHeight, t));
            yield return null;
        }
        // --- THÊM DÒNG NÀY VÀO ĐÂY ---
        // Cách 1: Tắt ẩn Canvas đi (Khuyên dùng, an toàn nhất)
        gameObject.SetActive(false);

        // Cách 2: Hoặc nếu bạn muốn xóa sổ nó hoàn toàn khỏi Hierarchy cho sạch sẽ:
        // Destroy(gameObject);
    }
}