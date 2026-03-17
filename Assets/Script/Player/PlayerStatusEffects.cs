using UnityEngine;
using System.Collections;

public class PlayerStatusEffects : MonoBehaviour
{
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement; // Điều chỉnh theo đường dẫn script của bạn
    private SpriteRenderer sr;
    private Color originalColor;
    private float originalSpeed;

    void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerMovement = GetComponent<PlayerMovement>();
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;
        if (playerMovement != null) originalSpeed = playerMovement.moveSpeed;
    }

    public void ApplyBurn(int dmg, float dur)
    {
        StartCoroutine(BurnRoutine(dmg, dur));
    }

    private IEnumerator BurnRoutine(int dmg, float dur)
    {
        float elapsed = 0;
        if (sr != null) sr.color = new Color(1f, 0.4f, 0f); // Màu cháy

        while (elapsed < dur)
        {
            if (playerHealth != null) playerHealth.TakeDamage(dmg);
            yield return new WaitForSeconds(0.5f);
            elapsed += 0.5f;
        }
        if (sr != null) sr.color = originalColor;
    }

    public void ApplySlow(float percent, float dur)
    {
        StartCoroutine(SlowRoutine(percent, dur));
    }

    private IEnumerator SlowRoutine(float percent, float dur)
    {
        if (playerMovement != null) playerMovement.moveSpeed = originalSpeed * (1f - percent);
        if (sr != null) sr.color = new Color(0.5f, 0.5f, 1f); // Màu lạnh

        yield return new WaitForSeconds(dur);

        if (playerMovement != null) playerMovement.moveSpeed = originalSpeed;
        if (sr != null) sr.color = originalColor;
    }
}
