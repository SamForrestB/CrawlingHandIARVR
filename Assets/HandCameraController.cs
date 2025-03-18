using UnityEngine;

public class HandCameraController : MonoBehaviour
{
    [Tooltip("The player's head transform (like the VR camera).")]
    public Transform playerHead;

    [Tooltip("The camera transform that should follow the hand.")]
    public Transform gameCamera;

    [Tooltip("How much head rotation triggers hand rotation.")]
    public float rotationThreshold = 60f;

    [Tooltip("Speed at which the hand rotates.")]
    public float handRotationSpeed = 100f;

    private bool rotatingLeft = false;
    private bool rotatingRight = false;

    private float initialHeadYRotation;

    void Start()
    {
        if (playerHead == null || gameCamera == null)
        {
            Debug.LogError("Player head and/or game camera not assigned!");
            enabled = false;
        }

        // Record the initial forward-facing head rotation
        initialHeadYRotation = playerHead.eulerAngles.y;
    }

    void Update()
    {
        HandleHeadTracking();
        UpdateCameraPosition();
    }

    void HandleHeadTracking()
    {
        float currentHeadYRotation = playerHead.eulerAngles.y;
        float deltaY = Mathf.DeltaAngle(initialHeadYRotation, currentHeadYRotation);

        // Detect if the head has moved beyond the threshold
        if (deltaY > rotationThreshold && !rotatingLeft)
        {
            rotatingRight = true;
            rotatingLeft = false;
        }
        else if (deltaY < -rotationThreshold && !rotatingRight)
        {
            rotatingLeft = true;
            rotatingRight = false;
        }
        else if (Mathf.Abs(deltaY) < 5f) // Stop rotating when head looks forward
        {
            rotatingLeft = false;
            rotatingRight = false;
        }

        // Rotate the hand if one of the conditions is active
        if (rotatingLeft)
        {
            RotateHand(-handRotationSpeed);
        }
        else if (rotatingRight)
        {
            RotateHand(handRotationSpeed);
        }
    }

    void RotateHand(float rotationSpeed)
    {
        // Rotate the hand around the Y-axis
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    void UpdateCameraPosition()
    {
        // Keep the camera behind the hand at a fixed offset
        Vector3 offset = new Vector3(0, 1.5f, -2f); // Adjust as needed for height and distance
        gameCamera.position = transform.position + transform.rotation * offset;

        // Ensure the camera always faces the hand
        gameCamera.LookAt(transform.position + transform.forward * 2f);
    }
}
