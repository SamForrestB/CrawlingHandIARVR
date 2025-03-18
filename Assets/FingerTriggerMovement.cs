using UnityEngine;

public class FingerTriggerMovement : MonoBehaviour
{
    public GameObject hand; // Reference to the hand GameObject
    public float moveDistance = 0.1f; // Distance to move the hand forward
    public float cooldownTime = 0.2f; // Cooldown time in seconds
    public float acceleration = 2f; // How quickly the hand speeds up
    public float maxSpeed = 1f; // Maximum speed of the hand
    public float deceleration = 2f; // How quickly the hand slows down

    private float _lastMoveTime; // Time when the hand last moved forward
    private BoxCollider _boxCollider; // Reference to the Box Collider
    private Vector3 _targetPosition; // Target position for smooth movement
    private Vector3 _velocity; // Current velocity of the hand

    void Start()
    {
        _lastMoveTime = -cooldownTime; // Initialize to allow immediate movement
        _boxCollider = GetComponent<BoxCollider>(); // Get the Box Collider component
        _targetPosition = hand.transform.position; // Initialize target position
        _velocity = Vector3.zero; // Initialize velocity
    }

    void Update()
    {
        // Calculate the world position and size of the Box Collider
        Vector3 boxCenter = transform.position + transform.TransformVector(_boxCollider.center); // Convert local center to world space
        Vector3 boxSize = Vector3.Scale(_boxCollider.size, transform.lossyScale); // Adjust size for scale

        // Detect colliders within the Box Collider's bounds
        Collider[] colliders = Physics.OverlapBox(
            boxCenter,
            boxSize / 2,
            transform.rotation
        );

        bool fingerDetected = false;

        // Check if any of the colliders are finger tips
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("FingerTip"))
            {
                Debug.Log("FingerTip detected: " + collider.name);
                fingerDetected = true;
                break; // Exit the loop as soon as a finger tip is detected
            }
        }

        // Update the target position if a finger tip is detected and the cooldown has passed
        if (fingerDetected && Time.time - _lastMoveTime >= cooldownTime)
        {
            _targetPosition += hand.transform.forward * moveDistance; // Move the target position forward
            _lastMoveTime = Time.time;
        }

        // Smoothly move the hand toward the target position
        MoveHandSmoothly();
    }

    void MoveHandSmoothly()
    {
        // Calculate the direction to the target position
        Vector3 direction = (_targetPosition - hand.transform.position).normalized;

        // Accelerate the hand toward the target position
        if (hand.transform.position != _targetPosition)
        {
            _velocity += direction * acceleration * Time.deltaTime;

            // Clamp the velocity to the maximum speed
            if (_velocity.magnitude > maxSpeed)
            {
                _velocity = _velocity.normalized * maxSpeed;
            }
        }
        else
        {
            // Decelerate the hand when it reaches the target position
            _velocity = Vector3.Lerp(_velocity, Vector3.zero, deceleration * Time.deltaTime);
        }

        // Move the hand using the calculated velocity
        hand.transform.position += _velocity * Time.deltaTime;

        // Snap to the target position if very close
        if (Vector3.Distance(hand.transform.position, _targetPosition) < 0.01f)
        {
            hand.transform.position = _targetPosition;
            _velocity = Vector3.zero;
        }
    }

    // Draw the Box Collider's bounds in the Scene view for debugging
    void OnDrawGizmos()
    {
        if (_boxCollider != null)
        {
            Gizmos.color = Color.green;
            Vector3 boxCenter = transform.position + transform.TransformVector(_boxCollider.center); // Convert local center to world space
            Vector3 boxSize = Vector3.Scale(_boxCollider.size, transform.lossyScale); // Adjust size for scale
            Gizmos.matrix = Matrix4x4.TRS(boxCenter, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, boxSize);
        }
    }
}