using UnityEngine;

public class DummyHealth : MonoBehaviour, IDamageable
{
    public int hp = 5;

    public void TakeDamage(int damage)
    {
        hp -= damage;
        Debug.Log("Dummy bị đánh, HP = " + hp);

        if (hp <= 0)
        {
            Debug.Log("Dummy chết");
            Destroy(gameObject);
        }
    }
}
