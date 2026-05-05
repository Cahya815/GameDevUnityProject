using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 20f;
    public float zoomSpeed = 5f;
    public float minSize = 5f;
    public float maxSize = 20f;

    void Update()
    {
        // 1. Logika Pergerakan WASD
        float moveX = Input.GetAxis("Horizontal"); // A dan D
        float moveZ = Input.GetAxis("Vertical");   // W dan S

        // Kita gerakkan kamera berdasarkan arah 'Forward' dan 'Right' kamera itu sendiri
        // Tapi kita nolkan sumbu Y agar kamera tidak terbang ke atas
        Vector3 forward = transform.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 right = transform.right;
        right.y = 0;
        right.Normalize();

        Vector3 moveDirection = (forward * moveZ) + (right * moveX);
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        // 2. Logika Zoom (Scroll Mouse) untuk Kamera Orthographic
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Camera.main.orthographicSize -= scroll * zoomSpeed * 100f * Time.deltaTime;
        
        // Batasi zoom agar tidak terlalu dekat atau terlalu jauh
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, minSize, maxSize);
    }
}