using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    public LayerMask enemyLayer;

    int damage;
    float lifeTime;
    float dir;

    PlayerEnergy playerMana;
    bool hasHit; // tránh hồi mana nhiều lần

    public void Init(float direction, int dmg, float time)
    {
        dir = direction;
        damage = dmg;
        lifeTime = time;

        // Flip hitbox
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * direction;
        transform.localScale = s;

        playerMana = GetComponentInParent<PlayerEnergy>();

        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        if (((1 << other.gameObject.layer) & enemyLayer) == 0) return;

        hasHit = true;

        Debug.Log("Melee hit: " + other.name);

        // EnemyHealth hp = other.GetComponent<EnemyHealth>();
        // if (hp != null) hp.TakeDamage(damage);

        // Hồi mana khi trúng enemy
        if (playerMana != null)
        {
            playerMana.GainEnergy(5f);
        }

        // Optional: phá hitbox ngay khi trúng
        Destroy(gameObject);
    }
}
