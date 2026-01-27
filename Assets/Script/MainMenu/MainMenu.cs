using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    public float delayBeforeLoad = 0.25f; // thời gian cho click sound phát

    public void StartGame()
    {
        StartCoroutine(LoadSceneDelay());
    }

    IEnumerator LoadSceneDelay()
    {
        yield return new WaitForSeconds(delayBeforeLoad);
        SceneManager.LoadScene("MapBeach");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}
