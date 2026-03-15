using System.Collections;
using UnityEngine;
using UnityEngine.Playables; // Nếu dùng Timeline

public class ChaseManager : MonoBehaviour
{
    [Header("References")]
    public GameObject bossObj;
    public Transform player;
    public float escapeTime = 30f; // Thời gian rượt đuổi (30s)

    [Header("Camera Shake")]
    public CameraShake cameraShake; // Script Rung màn hình của bạn

    [Header("Cutscene End")]
    public PlayableDirector endCutscene; // Timeline chiếu cảnh trốn bụi cỏ
    public GameObject normalEnemySpawner; // Spawner quái thường sau khi boss đi

    [Header("Boss Spawn Settings")]
    [Tooltip("Nếu BẬT: Boss sẽ xuất hiện ở vị trí ban đầu trong scene. Nếu TẮT: Boss sẽ được tính toán tự động từ camera.")]
    public bool useInitialPosition = true; // Sử dụng vị trí ban đầu trong scene
    public float bossSpawnOffsetX = -8f; // Khoảng cách từ camera để spawn boss (bên trái) - chỉ dùng khi useInitialPosition = false

    private bool isChasing = false;
    private Camera mainCamera;
    private Vector3 bossInitialPosition; // Lưu vị trí ban đầu của boss

    void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = FindObjectOfType<Camera>();
        
        // Lưu vị trí ban đầu của boss khi Awake
        if (bossObj != null)
        {
            bossInitialPosition = bossObj.transform.position;
        }
    }

    // Hàm này sẽ được gọi bởi ChaseTrigger
    public void StartChase()
    {
        if (isChasing) return;
        isChasing = true;

        // 1. Rung màn hình
        if (cameraShake != null) 
            cameraShake.Shake(0.5f, 2f); // Rung 0.5 giây với độ mạnh 2

        // 2. Kích hoạt Boss và đặt vị trí
        if (bossObj != null)
        {
            bossObj.SetActive(true);
            
            // Chọn cách đặt vị trí boss
            if (useInitialPosition)
            {
                // Sử dụng vị trí ban đầu trong scene
                bossObj.transform.position = bossInitialPosition;
            }
            else
            {
                // Tính toán tự động từ camera (cách cũ)
                if (mainCamera != null && player != null)
                {
                    Vector3 cameraLeftEdge = mainCamera.ViewportToWorldPoint(new Vector3(0, 0.5f, mainCamera.nearClipPlane));
                    Vector3 bossSpawnPos = new Vector3(
                        cameraLeftEdge.x + bossSpawnOffsetX, 
                        player.position.y, 
                        bossObj.transform.position.z
                    );
                    bossObj.transform.position = bossSpawnPos;
                }
            }
        }

        // 3. Bắt đầu đếm ngược thời gian rượt đuổi
        StartCoroutine(ChaseTimerRoutine());
    }

    private IEnumerator ChaseTimerRoutine()
    {
        float timer = escapeTime;
        while (timer > 0 && isChasing)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        // Chỉ EndChase nếu vẫn đang chase (không bị dừng bởi player die)
        if (isChasing)
        {
            EndChase();
        }
    }

    private void EndChase()
    {
        isChasing = false;

        // 1. Dừng boss
        if (bossObj != null)
        {
            BossChaseController bossController = bossObj.GetComponent<BossChaseController>();
            if (bossController != null)
            {
                bossController.StopChasing();
            }
        }

        // 2. Chạy Video / Timeline cảnh trốn bụi cỏ
        if (endCutscene != null)
        {
            endCutscene.Play();
            
            // Đợi cutscene kết thúc rồi mới ẩn boss
            StartCoroutine(WaitForCutsceneEnd());
        }
        else
        {
            // Nếu không có cutscene, ẩn boss ngay
            HideBossAndContinue();
        }
    }

    private IEnumerator WaitForCutsceneEnd()
    {
        // Đợi cutscene chạy xong
        if (endCutscene != null)
        {
            while (endCutscene.state == PlayState.Playing)
            {
                yield return null;
            }
        }
        
        // Sau khi cutscene kết thúc
        HideBossAndContinue();
    }

    private void HideBossAndContinue()
    {
        // 3. Ẩn Boss đi
        if (bossObj != null)
        {
            bossObj.SetActive(false);
        }

        // 4. Bật hệ thống Spawn quái thường (nếu có)
        if (normalEnemySpawner != null)
        {
            normalEnemySpawner.SetActive(true);
        }
    }

    // Hàm để dừng chase khi player die (gọi từ BossChaseController)
    public void StopChase()
    {
        isChasing = false;
        StopAllCoroutines();
    }
}
