using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    // Bỏ private, đổi thành public để nhập tay trong Inspector
    public float length;
    private float startpos;

    public GameObject cam;
    public float parallaxEffect;

    void Start()
    {
        startpos = transform.position.x;

        // NẾU bạn quên nhập length, code sẽ tự tính (dự phòng)
        if (length == 0)
        {
            length = GetComponent<SpriteRenderer>().bounds.size.x;
        }
    }

    void Update()
    {
        float temp = (cam.transform.position.x * (1 - parallaxEffect));
        float dist = (cam.transform.position.x * parallaxEffect);

        transform.position = new Vector3(startpos + dist, transform.position.y, transform.position.z);

        // Logic lặp vô hạn
        if (temp > startpos + length) startpos += length;
        else if (temp < startpos - length) startpos -= length;
    }
}