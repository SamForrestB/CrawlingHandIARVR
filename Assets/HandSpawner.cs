using UnityEngine;

public class HandSpawner : MonoBehaviour
{
    public GameObject rightHandPrefab; // Assign your right hand prefab in the inspector
    public float palmUpThreshold = 0.8f; // Threshold for palm facing up

    private GameObject spawnedHand;

    void Update()
    {
        // Check if the right hand is tracked
        if (IsRightHandTracked(out Vector3 palmNormal))
        {
            // Check if the palm is facing up
            if (IsPalmFacingUp(palmNormal))
            {
                // Spawn the right hand prefab if it hasn't been spawned yet
                if (spawnedHand == null)
                {
                    spawnedHand = Instantiate(rightHandPrefab, transform.position, transform.rotation);
                }
            }
            else
            {
                // Destroy the spawned hand if the palm is no longer facing up
                if (spawnedHand != null)
                {
                    Destroy(spawnedHand);
                    spawnedHand = null;
                }
            }
        }
    }

    private bool IsRightHandTracked(out Vector3 palmNormal)
    {
        // Replace this with the actual method to get hand tracking data from the SDK
        // For example, if the SDK provides a Hand or HandJoint class, use it here.
        palmNormal = Vector3.zero;

        // Example: Check if the right hand is tracked and get the palm normal
        // var rightHand = SDK.GetHand(HandType.Right);
        // if (rightHand != null && rightHand.IsTracked)
        // {
        //     palmNormal = rightHand.PalmNormal;
        //     return true;
        // }

        // Placeholder: Assume the hand is tracked for demonstration purposes
        palmNormal = Vector3.up; // Replace with actual palm normal from the SDK
        return true;
    }

    private bool IsPalmFacingUp(Vector3 palmNormal)
    {
        // Check if the palm is facing upwards (dot product with world up vector)
        float dotProduct = Vector3.Dot(palmNormal, Vector3.up);

        // If the dot product is greater than the threshold, the palm is facing up
        return dotProduct > palmUpThreshold;
    }
}