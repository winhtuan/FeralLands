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
        while (elapsed < dur)
        {
            // Nháy màu ĐỎ khi bị đốt
            if (sr != null) sr.color = Color.red;
            
            // Gây sát thương mỗi nhịp
            if (playerHealth != null) playerHealth.TakeDamage(dmg);
            
            yield return new WaitForSeconds(0.15f); // Thời gian nháy màu
            if (sr != null) sr.color = originalColor;
            
            yield return new WaitForSeconds(0.35f); // Thời gian chờ trước nhịp tiếp theo (tổng ~0.5s)
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
        float elapsed = 0;
        if (playerMovement != null) playerMovement.moveSpeed = originalSpeed * (1f - percent);

        while (elapsed < dur)
        {
            // Nháy màu XANH khi bị chậm (Sử dụng Cyan - xanh lơ/xanh dương nhạt)
            if (sr != null) sr.color = Color.cyan;
            
            yield return new WaitForSeconds(0.15f);
            if (sr != null) sr.color = originalColor;
            
            yield return new WaitForSeconds(0.35f);
            elapsed += 0.5f;
        }

        if (playerMovement != null) playerMovement.moveSpeed = originalSpeed;
        if (sr != null) sr.color = originalColor;
    }
}
