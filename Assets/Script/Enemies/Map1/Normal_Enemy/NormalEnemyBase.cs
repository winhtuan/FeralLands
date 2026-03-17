using UnityEngine;
using System;

public abstract class NormalEnemyBase : MonoBehaviour, IDamageable
{
    protected Animator animator;
    protected Rigidbody2D rb;

    [Header("Stats")]
    public int maxHP = 50;
    public int currentHP;
    public bool IsDead { get; protected set; }

    public event Action OnDeath;
    public event Action<int> OnDamaged;

    [Header("Attack Settings")]
    public AttackHitboxController hitbox; // KÉO TỪ ATTACKHITBOX

    [Header("Save System")]
    [SerializeField] private string enemyID; // Tự động tạo nếu để trống
    public string EnemyID => enemyID;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currentHP = maxHP;

        // Auto-generate a unique ID from scene + name + position if not set in Inspector
        if (string.IsNullOrEmpty(enemyID))
            enemyID = $"{gameObject.scene.name}_{gameObject.name}_{transform.position.x:F0}_{transform.position.y:F0}";

        if (hitbox == null)
        {
            // Chỉ cảnh báo nếu là quái đánh gần (không phải quái bắn đạn)
            EnemyMovement em = GetComponent<EnemyMovement>();
            bool ranged = em != null && em.isRanged;
            if (!ranged)
                Debug.LogWarning($"{name}: Không có AttackHitboxController!");
        }
    }

    protected virtual void Start()
    {
        if (SaveManager.Instance == null) return;

        // Killed in a previous session — remove immediately
        if (SaveManager.Instance.WasEnemyKilled(enemyID))
        {
            Destroy(gameObject);
            return;
        }

        // Restore position from last save
        GameData data = SaveManager.Instance.GetCachedData();
        if (data?.enemyPositions != null)
        {
            EnemyPositionData saved = data.enemyPositions.Find(e => e.enemyID == enemyID);
            if (saved != null)
                transform.position = new Vector3(saved.posX, saved.posY, transform.position.z);
        }
    }

    public virtual void TakeDamage(int dmg)
    {
        if (IsDead) return;

        currentHP -= dmg;
        OnDamaged?.Invoke(dmg);

        if (currentHP <= 0)
            Die();
    }

    public void SetSpeed(float speed)
    {
        if (animator != null)
            animator.SetFloat("Speed", speed);
    }

    public void Attack()
    {
        if (animator != null)
            animator.SetTrigger("Attack");
    }

    public virtual void Die()
    {
        if (IsDead) return;
        IsDead = true;

        // Register as killed so it won't respawn on next load
        SaveManager.Instance?.MarkEnemyKilled(enemyID);

        OnDeath?.Invoke();
        if (animator != null)
            animator.SetTrigger("Die");
        
        // Disable collider to stop physical interactions
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Freeze Rigidbody if exists
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // Hủy quái khỏi map sau 1.5 giây để animation kịp chạy xong
        Destroy(gameObject, 1.5f);
    }

    // 👉 ANIMATION EVENT GỌI HÀM NÀY
    public void EnableHitbox()
    {
        if (hitbox != null)
            hitbox.EnableHitbox();
    }

    public void DisableHitbox()
    {
        if (hitbox != null)
            hitbox.DisableHitbox();
    }

    private void OnDestroy()
    {
        Debug.LogWarning($"[DESTROY] {gameObject.name} đã bị biến mất khỏi Hierarchy!");
    }
}
