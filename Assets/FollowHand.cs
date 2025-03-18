using UnityEngine;

public class FollowHand : MonoBehaviour
{
    public Transform hand; // Reference to the hand GameObject
    public Transform wrist; // Reference to the wrist GameObject
    public float distance = 1f; // Distance behind the hand
    public float height = 0.5f; // Height above the wrist
    public float followSpeed = 5f; // How quickly the camera follows the hand
    public float rotationSpeed = 5f; // How quickly the camera rotates with the hand
    public LocomotionTechnique locomotionTechnique; // Reference to LocomotionTechnique

    private Vector3 _targetPosition; // Target position for the camera
    private Quaternion _targetRotation; // Target rotation for the camera

    void LateUpdate()
    {
        // Check if LocomotionTechnique is assigned and if the mode is not Selection
        if (locomotionTechnique != null && locomotionTechnique.currentMode != LocomotionTechnique.LocomotionMode.Selection)
        {
            // Correct the forward vector using the hand's local orientation
            Vector3 correctedForward = GetCorrectedHandForward();
            Vector3 correctedUp = hand.rotation * Vector3.up;

            // Offset the camera behind the hand and above the wrist
            Vector3 offset = -correctedForward * distance + correctedUp * height;
            _targetPosition = hand.position + offset;

            // Smoothly move the camera toward the target position
            transform.position = Vector3.Lerp(transform.position, _targetPosition, followSpeed * Time.deltaTime);

            // Calculate the camera's target rotation (look at the wrist)
            Vector3 directionToWrist = wrist.position - transform.position;
            _targetRotation = Quaternion.LookRotation(directionToWrist, correctedUp);

            // Smoothly rotate the camera toward the target rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private Vector3 GetCorrectedHandForward()
    {
        Quaternion rotationCorrection = Quaternion.Euler(0, -90, 0);
        return rotationCorrection * hand.forward;
    }
    private void OnDrawGizmos()
    {
        if (hand != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(hand.position, hand.forward * 0.5f); // Visualize the forward direction
        }
    }
}