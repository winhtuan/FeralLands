using UnityEngine;

public class SlashEffect : MonoBehaviour
{
    public float lifeTime = 0.2f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}
