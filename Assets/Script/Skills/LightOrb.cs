using UnityEngine;

public class LightOrb : MonoBehaviour
{
    public float speed = 10f;
    public float maxDistance = 3f;
    public int damage = 10; 

    private Rigidbody2D rb;
    private Vector2 shooterStartPos;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 direction, Transform shooter, int dmg)
    {
        shooterStartPos = shooter.position;
        damage = dmg;
        rb.linearVelocity = direction.normalized * speed;
    }


    void Update()
    {
        if (Vector2.Distance(shooterStartPos, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //Enemy enemy = other.GetComponent<Enemy>();
        //if (enemy != null)
        //{
        //    enemy.TakeDamage(damage);
        //    Destroy(gameObject);
        //}
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }
}
