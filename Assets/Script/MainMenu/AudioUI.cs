using UnityEngine;

public class AudioUI : MonoBehaviour
{
    public static AudioUI Instance;

    public AudioSource source;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void PlayHover()
    {
        source.PlayOneShot(hoverSound);
    }

    public void PlayClick()
    {
        source.PlayOneShot(clickSound);
    }
}
