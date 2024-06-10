using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public float scrollSpeed = 1.0f;
    private Camera mainCamera;
    private float halfCameraWidth;

    private void Start()
    {
        mainCamera = Camera.main;
        CalculateHalfCameraWidth();
    }

    private void Update()
    {
        // Move the background continuously from right to left
        transform.Translate(Vector3.left * scrollSpeed * Time.deltaTime);

        // Check if the background's right side has completely left the camera's view on the left side
        if (transform.position.x + halfCameraWidth < mainCamera.transform.position.x - halfCameraWidth)
        {
            // Respawn the background just at the right edge of the camera
            float respawnX = mainCamera.transform.position.x + (halfCameraWidth *2);
            transform.position = new Vector3(respawnX, transform.position.y, transform.position.z);
        }
    }

    private void CalculateHalfCameraWidth()
    {
        halfCameraWidth = mainCamera.aspect * mainCamera.orthographicSize;
    }
}
