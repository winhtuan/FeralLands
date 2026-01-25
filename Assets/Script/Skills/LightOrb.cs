using UnityEngine;

public class LightOrb : MonoBehaviour
{
    public float speed = 10f;
    public float maxDistance = 3f;
    public int damage = 10;

    private Vector2 startPos;
    private Vector2 dir;

    public void Launch(Vector2 direction, Transform shooter, int dmg)
    {
        startPos = transform.position;
        dir = direction.normalized;
        damage = dmg;
    }

    void Update()
    {
        transform.position += (Vector3)(dir * speed * Time.deltaTime);

        if (Vector2.Distance(startPos, transform.position) >= maxDistance)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        IDamageable dmg = other.GetComponent<IDamageable>();
        if (dmg != null)
        {
            dmg.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}

