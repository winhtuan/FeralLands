using UnityEngine;

public class CameraShake : MonoBehaviour
{
    float shakeTime;
    float strength;

    Vector3 originalLocalPos;

    void Awake()
    {
        originalLocalPos = transform.localPosition;
    }

    void LateUpdate()
    {
        if (shakeTime > 0)
        {
            Vector2 offset = Random.insideUnitCircle * strength;
            transform.localPosition = originalLocalPos + new Vector3(offset.x, offset.y, 0);

            shakeTime -= Time.deltaTime;
        }
        else
        {
            transform.localPosition = originalLocalPos;
        }
    }

    public void Shake(float duration, float power)
    {
        shakeTime = duration;
        strength = power;
    }
}
