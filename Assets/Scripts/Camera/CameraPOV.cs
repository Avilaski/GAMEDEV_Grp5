using UnityEngine;

public class CameraPOV : MonoBehaviour
{
    public Camera mainCamera;

    private int currentPerspective = 0;

    private Vector3[] cameraPositions =
    {
        new Vector3(6f, 12f, 3.5f),       // Default
        new Vector3(0.01f, 12f, 2.1f),    // Camera 1
        new Vector3(0.01f, 8f, -8f)       // Camera 2
    };

    private Vector3[] cameraRotations =
    {
        new Vector3(64f, -89f, 1f),       // Default
        new Vector3(89.5f, -7f, 83f),     // Camera 1
        new Vector3(40f, 0f, 0f)          // Camera 2
    };

    public void ChangePerspective()
    {
        Debug.Log("Camera button clicked!");

        if (mainCamera == null)
        {
            Debug.LogError("Main Camera is not assigned!");
            return;
        }

        currentPerspective++;

        if (currentPerspective >= cameraPositions.Length)
        {
            currentPerspective = 0;
        }

        mainCamera.transform.position =
            cameraPositions[currentPerspective];

        mainCamera.transform.eulerAngles =
            cameraRotations[currentPerspective];

        Debug.Log("Switched to perspective: " + currentPerspective);
    }
}