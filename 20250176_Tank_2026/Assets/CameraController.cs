using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform tank1;
    public Transform tank2;

    public Vector3 offset = new Vector3(0, 15, -10);
    public float smoothSpeed = 0.125f;

    public float minHeight = 15f;
    public float maxHeight = 40f;
    public float zoomLimit = 20f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 centerPoint = (tank1.position + tank2.position) / 2f;
        Vector3 newPosition = centerPoint + offset;
        transform.position = Vector3.Lerp(transform.position, newPosition, smoothSpeed);

        float distance = Vector3.Distance(tank1.position, tank2.position);
        float newZoom = Mathf.Lerp(10f, 30f, distance / zoomLimit);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, newZoom, Time.deltaTime);

        Vector3 camPos = transform.position;
        float newHeight = Mathf.Lerp(minHeight, maxHeight, distance / zoomLimit);
        camPos.y = newHeight;
        transform.position = camPos;
    }


}
