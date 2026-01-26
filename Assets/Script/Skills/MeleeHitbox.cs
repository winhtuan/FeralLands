using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    public int damage = 15;
    public LayerMask enemyLayer;

    Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        col.enabled = false;
    }

    public void EnableHitbox() => col.enabled = true;
    public void DisableHitbox() => col.enabled = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) == 0) return;

        Debug.Log("MELEE HIT: " + other.name);
        other.GetComponent<IDamageable>()?.TakeDamage(damage);
    }
}
