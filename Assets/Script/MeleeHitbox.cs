using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    public LayerMask enemyLayer;

    int damage;
    float lifeTime;
    float dir;

    public void Init(float direction, int dmg, float time)
    {
        dir = direction;
        damage = dmg;
        lifeTime = time;

        // Flip hitbox
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * direction;
        transform.localScale = s;

        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) == 0) return;

        //EnemyHealth hp = other.GetComponent<EnemyHealth>();
        //if (hp != null)
        //{
        //    hp.TakeDamage(damage);
        //}
    }
}
