using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    public float delayBeforeLoad = 0.25f;

    [Header("Buttons")]
    public Button continueButton; // Kéo nút Continue vào đây

    void Start()
    {
        // Fallback: find by GameObject name if not assigned in Inspector
        if (continueButton == null)
            continueButton = GameObject.Find("Continue")?.GetComponent<Button>();

        // Always use File.Exists directly — most reliable regardless of SaveManager state
        bool hasSave = System.IO.File.Exists(
            Application.persistentDataPath + "/save.json");

        if (continueButton != null)
        {
            continueButton.interactable = hasSave;

            if (!hasSave)
            {
                // Keep button frame visible but darkened (high alpha so image still shows)
                ColorBlock colors = continueButton.colors;
                colors.disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);
                continueButton.colors = colors;

                // Directly darken the button Image so the frame stays visible
                Image img = continueButton.GetComponent<Image>();
                if (img != null)
                    img.color = new Color(0.4f, 0.4f, 0.4f, 0.6f);

                // Gray out all text children (Legacy Text)
                foreach (Text t in continueButton.GetComponentsInChildren<Text>(true))
                    t.color = Color.gray;

                // Gray out all text children (TextMeshPro)
                foreach (TMPro.TextMeshProUGUI t in continueButton.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))
                    t.color = Color.gray;

                Debug.Log("[MainMenu] No save found — Continue button disabled.");
            }
        }
        else
        {
            Debug.LogWarning("[MainMenu] continueButton not found! Kéo nút CONTINIUE vào Inspector.");
        }
    }

    /// <summary>
    /// Nút NEW GAME — xóa save cũ và bắt đầu từ đầu.
    /// </summary>
    public void NewGame()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.DeleteSave();

        StartCoroutine(LoadSceneDelay("IntroScene"));
    }

    /// <summary>
    /// Nút CONTINUE — vào lại đúng scene đã lưu, PlayerSaveLoad tự restore trạng thái.
    /// </summary>
    public void ContinueGame()
    {
        if (!SaveManager.HasSave())
        {
            Debug.LogWarning("[MainMenu] Không có save để load! Path: " + Application.persistentDataPath + "/save.json");
            return;
        }

        GameData data = SaveManager.Instance.LoadGame();
        if (data == null) return;

        string targetScene = data.currentSceneName;

        // Guard against corrupted saves that wrote "MainMenu" as the scene name
        if (string.IsNullOrEmpty(targetScene) || targetScene == "MainMenu")
            targetScene = "MapBeach 1";

        Debug.Log($"[MainMenu] Continuing to scene: {targetScene}");
        StartCoroutine(LoadSceneDelay(targetScene));
    }

    /// <summary>
    /// Giữ lại cho các nút cũ đã gán trong Inspector.
    /// </summary>
    public void StartGame()
    {
        NewGame();
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }

    IEnumerator LoadSceneDelay(string sceneName)
    {
        yield return new WaitForSeconds(delayBeforeLoad);

        try
        {
            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.TeleportToMap(sceneName);
            else
                SceneManager.LoadScene(sceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MainMenu] Scene load failed for '{sceneName}': {e.Message}");
            SceneManager.LoadScene(sceneName); // fallback
        }
    }
}
