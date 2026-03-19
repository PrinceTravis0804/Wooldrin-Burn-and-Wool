using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10);

    [Header("Zoom Settings")]
    public float minSize = 2f;        // How close you can zoom in
    public float maxSize = 15f;       // How far you can see the map
    public float zoomSensitivity = 2f;
    public float zoomSmoothSpeed = 10f;

    private Camera cam;
    private float targetSize;

    void Start()
    {
        cam = GetComponent<Camera>();
        targetSize = cam.orthographicSize;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. FOLLOW LOGIC
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // 2. ZOOM LOGIC
        // Get the mouse scroll input
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            targetSize -= scroll * zoomSensitivity;
            // Clamp ensures we don't zoom too far in or out
            targetSize = Mathf.Clamp(targetSize, minSize, maxSize);
        }

        // Smoothly transition the camera's size
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.deltaTime * zoomSmoothSpeed);
    }
}