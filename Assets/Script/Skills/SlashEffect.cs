using UnityEngine;

public class SlashEffect : MonoBehaviour
{
    public float lifeTime = 0.3f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}
