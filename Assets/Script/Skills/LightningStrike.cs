using UnityEngine;

public class LightningStrike : MonoBehaviour
{
    public float lifeTime = 0.5f;
    public int damage = 30;
    public float radius = 0.5f;
    public LayerMask enemyLayer;

    void Start()
    {
        Invoke(nameof(DoDamage), 0.05f);
        Destroy(gameObject, lifeTime);
    }

    void DoDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);
        foreach (var hit in hits)
        {
            Debug.Log("Lightning hit: " + hit.name);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
