using UnityEngine;

public class LightOrb : MonoBehaviour
{
    public float speed = 25f;   
    public float lifeTime = 3f;   
    public int damage = 10;

    Rigidbody2D rb;

    public void Launch(Vector2 direction, Transform shooter, int dmg)
    {
        damage = dmg;
        rb = GetComponent<Rigidbody2D>();

        rb.linearVelocity = direction.normalized * speed;

        // tránh va chạm với player
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), shooter.GetComponent<Collider2D>());

        // auto destroy
        Destroy(gameObject, lifeTime);
    }
}
