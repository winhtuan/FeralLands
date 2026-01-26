//using UnityEngine;

//public class ParallaxController : MonoBehaviour
//{
//    Transform cam;
//    Vector3 camStartPos;
//    float distance;

//    GameObject[] backgrounds;
//    Material[] mat;
//    float[] backSpeed;

//    float farthestBack;

//    [Range(0.01f, 0.05f)]
//    public float parallaxSpeed;
//    void Start()
//    {
//        cam = Camera.main.transform;
//        camStartPos = cam.position;

//        int backCount = transform.childCount;
//        mat = new Material[backCount];
//        backSpeed = new float[backCount];
//        backgrounds = new GameObject[backCount];

//        for (int i = 0; i < backCount; i++)
//        {
//            backgrounds[i] = transform.GetChild(i).gameObject;
//            mat[i] = backgrounds[i].GetComponent<Renderer>().material;
//        }

//        BackSpeedCalculate(backCount);

//        void BackSpeedCalculate(int backCount)
//        {
//            for (int i = 0; i < backCount; i++) // find the farthest background
//            {
//                if ((backgrounds[i].transform.position.z - cam.position.z) > farthestBack)
//                {
//                    farthestBack = backgrounds[i].transform.position.z - cam.position.z;
//                }
//                // THÊM DÒNG NÀY: Cộng thêm 5 đơn vị để vật xa nhất vẫn có tốc độ trôi nhẹ
//                farthestBack += 5f;
//            }

//            for (int i = 0; i < backCount; i++)
//            {
//                backSpeed[i] = 1 - (backgrounds[i].transform.position.z - cam.position.z) / farthestBack;
//            }
//        }

//    }

//    //private void LateUpdate()
//    //{
//    //    distance = cam.position.x - camStartPos.x;
//    //    transform.position = new Vector3(cam.position.x, transform.position.y, 0);

//    //    for (int i = 0; i < backgrounds.Length; i++)
//    //    {
//    //        float speed = backSpeed[i] * parallaxSpeed;
//    //        mat[i].SetTextureOffset("_MainTex", new Vector2(distance, 0) * speed);
//    //        // Đổi "_MainTex" thành "_BaseMap" để tương thích với URP
//    //        //mat[i].SetTextureOffset("_BaseMap", new Vector2(distance, 0) * speed);
//    //    }
//    //}

//    private void LateUpdate()
//    {
//        distance = cam.position.x - camStartPos.x;
//        transform.position = new Vector3(cam.position.x, transform.position.y, 0);

//        for (int i = 0; i < backgrounds.Length; i++)
//        {
//            float speed = backSpeed[i] * parallaxSpeed;

//            // SỬA Ở ĐÂY: Thêm dấu trừ (-) trước chữ distance
//            // Mục đích: Đảo ngược hướng trôi lại cho đúng chiều
//            mat[i].SetTextureOffset("_MainTex", new Vector2(-distance, 0) * speed);
//        }
//    }

//}




//using UnityEngine;

//public class InfiniteParallax : MonoBehaviour
//{
//    [Header("Settings")]
//    public float scrollSpeed = 0.5f; // Tốc độ trôi của map

//    private Transform cam;
//    private Vector3 lastCamPos;

//    // Lưu trữ các lớp background
//    private Renderer[] backgrounds;
//    private float[] zDepths;

//    void Start()
//    {
//        cam = Camera.main.transform;
//        lastCamPos = cam.position;

//        // Tự động lấy tất cả các lớp con (Sky, Sea, Sand...)
//        int childCount = transform.childCount;
//        backgrounds = new Renderer[childCount];
//        zDepths = new float[childCount];

//        for (int i = 0; i < childCount; i++)
//        {
//            Transform child = transform.GetChild(i);
//            if (child.GetComponent<Renderer>() != null)
//            {
//                backgrounds[i] = child.GetComponent<Renderer>();
//                // Lưu lại vị trí Z để tính độ xa gần (Z càng cao trôi càng chậm)
//                zDepths[i] = child.position.z;
//            }
//        }
//    }

//    void LateUpdate()
//    {
//        // 1. Tính quãng đường Camera vừa di chuyển
//        float deltaX = cam.position.x - lastCamPos.x;

//        // 2. KỸ THUẬT MÁY CHẠY BỘ:
//        // Di chuyển cả cụm Background (gồm cả đất cứng Collider) đi theo Camera
//        // Giúp nhân vật không bao giờ bị "hết đường" hay rơi xuống vực
//        transform.position = new Vector3(cam.position.x, transform.position.y, transform.position.z);

//        // 3. CUỘN HÌNH ẢNH (TEXTURE OFFSET):
//        // Di chuyển vân bề mặt để tạo cảm giác map đang trôi
//        for (int i = 0; i < backgrounds.Length; i++)
//        {
//            if (backgrounds[i] != null)
//            {
//                // Công thức: Vật càng gần (Z nhỏ) thì trôi càng nhanh
//                // Layer_Sand (Z=0) sẽ trôi nhanh nhất
//                float parallaxFactor = 1f / (Mathf.Abs(zDepths[i]) + 1f);

//                // Tính toán độ lệch pha
//                float offset = deltaX * scrollSpeed * parallaxFactor;

//                // Cộng dồn vào Offset hiện tại của vật liệu
//                // Lưu ý: Code này hoạt động tốt với cả Mirror và Repeat
//                Vector2 currentOffset = backgrounds[i].material.mainTextureOffset;
//                currentOffset.x += offset;

//                backgrounds[i].material.mainTextureOffset = currentOffset;
//            }
//        }

//        // Cập nhật vị trí cũ
//        lastCamPos = cam.position;
//    }
//}




using UnityEngine;

public class InfiniteParallax : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Tăng giảm số này để mặt đất đứng yên. Nếu đất trôi về trước -> Tăng lên. Nếu đất trôi lùi quá nhanh -> Giảm xuống.")]
    public float scrollSpeed = 0.5f;

    [Tooltip("Đảo chiều cuộn nếu cần (Ví dụ: -1 hoặc 1)")]
    public float direction = 1f;

    private Transform cam;
    private Renderer[] backgrounds;
    private float[] zDepths;

    void Start()
    {
        cam = Camera.main.transform;

        int childCount = transform.childCount;
        backgrounds = new Renderer[childCount];
        zDepths = new float[childCount];

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            Renderer r = child.GetComponent<Renderer>();
            if (r != null)
            {
                backgrounds[i] = r;
                zDepths[i] = child.position.z;
            }
        }
    }

    void LateUpdate()
    {
        // 1. Kéo cả cụm Map đi theo Camera (Cơ chế băng chuyền)
        transform.position = new Vector3(cam.position.x, transform.position.y, transform.position.z);

        // 2. TÍNH TOÁN OFFSET DỰA TRÊN VỊ TRÍ TUYỆT ĐỐI CỦA CAMERA
        // Cách này mượt hơn và không bị lỗi cộng dồn sai số
        for (int i = 0; i < backgrounds.Length; i++)
        {
            if (backgrounds[i] != null)
            {
                // Z=0 (Đất) -> Factor = 1 (Trôi nhanh nhất)
                // Z=Xa -> Factor nhỏ dần
                float parallaxFactor = 1f / (Mathf.Abs(zDepths[i]) + 1f);

                // Tính toán offset mới
                // Nếu nhân vật đi sang phải (Cam X tăng) -> Offset tăng -> Texture trôi sang trái
                float offset = cam.position.x * scrollSpeed * parallaxFactor * direction;

                // --- HỖ TRỢ URP & WRAP MODE MIRROR/REPEAT ---
                int propertyID = backgrounds[i].material.HasProperty("_BaseMap")
                                 ? Shader.PropertyToID("_BaseMap")
                                 : Shader.PropertyToID("_MainTex");

                // Lấy offset hiện tại (Y giữ nguyên, chỉ thay đổi X)
                Vector2 currentOffset = backgrounds[i].material.GetTextureOffset(propertyID);

                // Gán trực tiếp giá trị offset mới (Thay vì cộng dồn +=)
                Vector2 newOffset = new Vector2(offset, currentOffset.y);

                backgrounds[i].material.SetTextureOffset(propertyID, newOffset);
            }
        }
    }
}