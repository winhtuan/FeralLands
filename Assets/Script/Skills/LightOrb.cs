using UnityEngine;

public class LightOrb : MonoBehaviour
{
    public float speed = 12f;
    public float lifeTime = 2.5f;
    public int damage = 10;

    Rigidbody2D rb;

    public void Launch(Vector2 direction, Transform shooter, int dmg)
    {
        rb = GetComponent<Rigidbody2D>();
        damage = dmg;

        rb.linearVelocity = direction.normalized * speed;

        // Ignore player collision
        Collider2D shooterCol = shooter.GetComponent<Collider2D>();
        Collider2D orbCol = GetComponent<Collider2D>();
        if (shooterCol && orbCol)
            Physics2D.IgnoreCollision(orbCol, shooterCol);

        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        IDamageable dmgTarget = other.GetComponent<IDamageable>();
        if (dmgTarget != null)
        {
            dmgTarget.TakeDamage(damage);

            // TODO: Screen Shake / HitStop here
            Destroy(gameObject);
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
