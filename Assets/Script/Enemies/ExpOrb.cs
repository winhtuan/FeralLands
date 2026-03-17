using UnityEngine;

public class ExpOrb : MonoBehaviour
{
    [Header("Orb Settings")]
    public int expValue = 5;
    
    [Header("Animation Settings")]
    public float floatSpeed = 2f;
    public float floatAmplitude = 0.2f;

    private float startY;
    
    void Start()
    {
        startY = transform.position.y;
    }

    void Update()
    {
        // Hiệu ứng lơ lửng cho ngọc EXP
        float newY = startY + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Chú ý nhãn (Tag) của người chơi phải là "Player"
        if (other.CompareTag("Player"))
        {
            // Tìm script PlayerLevel trên player
            PlayerLevel playerLevel = other.GetComponent<PlayerLevel>();
            if (playerLevel != null)
            {
                playerLevel.AddExp(expValue);
                
                // (Tuỳ chọn) Chơi âm thanh nhặt đồ hoặc hiệu ứng hạt tại đây
                // AudioManager.Instance.PlaySFX("PickUpExp");

                Destroy(gameObject);
            }
        }
    }
}
