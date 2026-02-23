using UnityEngine;

public class AttackHitboxController : MonoBehaviour
{
    private Collider2D hitbox;

    private void Awake()
    {
        hitbox = GetComponent<Collider2D>();
        hitbox.enabled = false;
    }

    public void EnableHitbox()
    {
        Debug.Log("HITBOX ON");
        hitbox.enabled = true;
        
        // Reset trạng thái đã đánh trúng để có thể đánh tiếp đòn mới
        EnemyHitbox enemyHitbox = GetComponent<EnemyHitbox>();
        if (enemyHitbox != null) enemyHitbox.ResetHitbox();
    }

    public void DisableHitbox()
    {
        Debug.Log("HITBOX OFF");
        hitbox.enabled = false;
    }
}
