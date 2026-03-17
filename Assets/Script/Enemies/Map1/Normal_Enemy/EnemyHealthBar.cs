using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("References")]
    public Slider slider;           // Kéo Slider vào đây
    public Transform targetEnemy;   // Kéo GameObject quái vào (để thanh máu đứng yên theo quái)

    private NormalEnemyBase enemyBase;
    private Vector3 offset;

    void Start()
    {
        if (targetEnemy != null)
        {
            enemyBase = targetEnemy.GetComponent<NormalEnemyBase>();
            offset = transform.position - targetEnemy.position;

            if (enemyBase != null)
            {
                // Gán max value
                slider.maxValue = enemyBase.maxHP;
                slider.value = enemyBase.maxHP;

                // Đăng ký event khi bị damage
                enemyBase.OnDamaged += UpdateBar;
                enemyBase.OnDeath += HideBar;
            }
        }
    }

    void LateUpdate()
    {
        // Thanh máu đi theo quái, luôn ở phía trên đầu
        if (targetEnemy != null)
            transform.position = targetEnemy.position + offset;
    }

    void UpdateBar(int dmg)
    {
        if (slider != null && enemyBase != null)
            slider.value = enemyBase.currentHP;
    }

    void HideBar()
    {
        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (enemyBase != null)
        {
            enemyBase.OnDamaged -= UpdateBar;
            enemyBase.OnDeath -= HideBar;
        }
    }
}
