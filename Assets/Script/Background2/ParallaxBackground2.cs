using UnityEngine;

public class ParallaxBackground2 : MonoBehaviour
{
    private float length, startpos;
    public GameObject cam;
    public float parallaxEffect; // Mức độ cuộn nhanh hay chậm

    void Start()
    {
        // Lưu lại vị trí ban đầu và chiều rộng của bức ảnh
        startpos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        // Tính toán khoảng cách camera đã di chuyển
        float temp = (cam.transform.position.x * (1 - parallaxEffect));
        float dist = (cam.transform.position.x * parallaxEffect);

        // Di chuyển layer dựa trên vị trí camera và hệ số parallax
        transform.position = new Vector3(startpos + dist, transform.position.y, transform.position.z);

        // Nếu camera đi quá chiều dài bức ảnh, reset lại vị trí để tạo vòng lặp vô tận
        if (temp > startpos + length) startpos += length;
        else if (temp < startpos - length) startpos -= length;
    }
}