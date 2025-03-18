using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using static UnityEngine.GridBrushBase;

public class LocomotionTechnique : MonoBehaviour
{
    // Please implement your locomotion technique in this script. 
    public GameObject hmd;

    //Public variables
    public GameObject hand;
    public BoxCollider boxCollider; 
    public LayerMask triggerLayer;
    public float cooldownTime = 0.2f; // Cooldown time for a finger
    public float acceleration = 2f; // How much acceration is applied
    public float maxSpeed = 1f; // Maximum speed of the player
    public float deceleration = 2f; // How fast the player slows down
    public float rotationSpeed = 10f; // How fast you rotate
    public float fingerDetectionThreshold = 4; // Number of fingers for stopping
    public Transform ovrRigTransform;
    public float jumpForce = 5f; // How high you jump
    public float jumpTimeThreshold = 0.2f; // How often you can jump
    public float fingerAccelerationTime = 0.5f; // How long acceleration will be applied when a finger is in the collision box
    public Transform secondHand; 
    public enum LocomotionMode { Moving, Selection }
    public LocomotionMode currentMode;
    public GameObject selectionObject;
    public MyGrab myGrab;

    // Private variables
    private float _lastMoveTime; 
    private Vector3 _moveDirection = Vector3.zero; 
    private float _currentSpeed = 0f;
    private bool _isRotatingLeft = false;
    private bool _isRotatingRight = false;
    private Quaternion correctionRotation;
    private Quaternion _handRotationOffset;
    private float _allFingersInTime = -1f;
    private Dictionary<Collider, float> _fingerEntryTimes = new Dictionary<Collider, float>();
    private Vector3 selectionPositionOffset; 


    /////////////////////////////////////////////////////////
    // These are for the game mechanism.
    public ParkourCounter parkourCounter;
    public string stage;
    public SelectionTaskMeasure selectionTaskMeasure;


    void Start()
    {
        _lastMoveTime = -cooldownTime; 
        _currentSpeed = 0f;
        currentMode = LocomotionMode.Moving;
        correctionRotation = Quaternion.Euler(0, -90, 0); 
        _handRotationOffset = hand.transform.rotation * correctionRotation;
    }

    void Update()
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Please implement your LOCOMOTION TECHNIQUE in this script :D.
        
        HandleRotationRaycast();
        HandleModeSwitching();
        HandleFingerMovement();

        ////////////////////////////////////////////////////////////////////////////////
        // These are for the game mechanism.
        if (OVRInput.Get(OVRInput.Button.Two) || OVRInput.Get(OVRInput.Button.Four))
        {
            if (parkourCounter.parkourStart)
            {
                transform.position = parkourCounter.currentRespawnPos;
            }
        }
    }

    void HandleModeSwitching()
    {
        RaycastHit hit;
        Ray ray = new Ray(secondHand.position, -secondHand.up);
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.green);
        

        if (Physics.Raycast(ray, out hit, 100f, triggerLayer))
        {
            if (hit.collider.CompareTag("SelectionTrigger"))
            {
                if (currentMode != LocomotionMode.Selection)
                {
                    hand.SetActive(false);
                    ovrRigTransform.position = hand.transform.position + new Vector3(0f,-0.6f, 0f);
                    currentMode = LocomotionMode.Selection;
                    if (selectionObject != null)
                        selectionObject.SetActive(true);
                    Debug.Log("Selection Mode Activated");
                }
            }
            else if (hit.collider.CompareTag("MovingTrigger"))
            {
                if (currentMode != LocomotionMode.Moving && myGrab.isSelected != true)
                {
                    hand.SetActive(true);
                    currentMode = LocomotionMode.Moving;
                    hand.transform.rotation = Quaternion.Euler(0, hand.transform.rotation.eulerAngles.y, hand.transform.rotation.eulerAngles.z);
                    if (selectionObject != null)
                        selectionObject.SetActive(false);
                    Debug.Log("Moving Mode Activated");
                }
            }
        }
    }

    void HandleRotationRaycast()
    {
        RaycastHit hit;
        Ray ray = new Ray(hmd.transform.position, hmd.transform.forward);

        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red); // For Visualization

        if (currentMode == LocomotionMode.Moving)
        {
            if (Physics.Raycast(ray, out hit, 100f, triggerLayer))
            {
                if (hit.collider.CompareTag("LeftRotationTrigger"))
                {
                    _isRotatingLeft = true;
                    _isRotatingRight = false;
                }
                else if (hit.collider.CompareTag("RightRotationTrigger"))
                {
                    _isRotatingRight = true;
                    _isRotatingLeft = false;
                }
            }
            else
            {
                _isRotatingLeft = false;
                _isRotatingRight = false;
            }

            if (_isRotatingLeft)
            {
                float rotationAmount = -rotationSpeed * Time.deltaTime;
                transform.Rotate(Vector3.up * rotationAmount);
                hand.transform.rotation = _handRotationOffset * Quaternion.Euler(0, transform.eulerAngles.y, 0);
            }
            else if (_isRotatingRight)
            {
                float rotationAmount = rotationSpeed * Time.deltaTime;
                transform.Rotate(Vector3.up * rotationAmount);
                hand.transform.rotation = _handRotationOffset * Quaternion.Euler(0, transform.eulerAngles.y, 0);
            }
        }
    }

    void HandleFingerMovement()
    {
        if (currentMode == LocomotionMode.Moving)
        {
            Vector3 boxCenter = boxCollider.transform.position + boxCollider.transform.TransformVector(boxCollider.center);
            Vector3 boxSize = Vector3.Scale(boxCollider.size, boxCollider.transform.lossyScale);

            Collider[] colliders = Physics.OverlapBox(boxCenter, boxSize / 2, boxCollider.transform.rotation);

            // Track active fingers and their entry times
            HashSet<Collider> activeFingers = new HashSet<Collider>();
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("FingerTip"))
                {
                    activeFingers.Add(collider);

                    // Record the entry time if the finger is new
                    if (!_fingerEntryTimes.ContainsKey(collider))
                    {
                        _fingerEntryTimes[collider] = Time.time;
                    }
                }
            }

            // Remove fingers that are no longer in the trigger area
            List<Collider> fingersToRemove = new List<Collider>();
            foreach (var finger in _fingerEntryTimes.Keys)
            {
                if (!activeFingers.Contains(finger))
                {
                    fingersToRemove.Add(finger);
                }
            }
            foreach (var finger in fingersToRemove)
            {
                _fingerEntryTimes.Remove(finger);
            }

            // Calculate speed based on active fingers within the time limit
            float targetSpeed = 0;
            foreach (var finger in _fingerEntryTimes.Keys)
            {
                if (Time.time - _fingerEntryTimes[finger] <= fingerAccelerationTime)
                {
                    targetSpeed += maxSpeed / fingerDetectionThreshold;
                }
            }
            targetSpeed = Mathf.Clamp(targetSpeed, 0, maxSpeed);

            bool fingerDetected = activeFingers.Count > 0;

            if (fingerDetected && Time.time - _lastMoveTime >= cooldownTime)
            {
                _moveDirection = correctionRotation * hand.transform.forward;
                _lastMoveTime = Time.time;
            }

            if (activeFingers.Count == fingerDetectionThreshold) // All fingers in
            {
                _allFingersInTime = Time.time; // Record the time
            }
            else if (activeFingers.Count == 0 && _allFingersInTime != -1f && Time.time - _allFingersInTime <= jumpTimeThreshold) // All fingers out quickly
            {
                hand.GetComponent<Rigidbody>().AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                _allFingersInTime = -1f; // Reset the timer
            }
            else if (activeFingers.Count == 0)
            {
                _allFingersInTime = -1f; // Reset the timer if fingers are out for too long
            }

            MoveHandSmoothly(targetSpeed, fingerDetected);
        }
        else if (currentMode == LocomotionMode.Selection)
        {
            //// Disable forward movement
            _moveDirection = Vector3.zero;
            _currentSpeed = 0;

            // Apply the stored offset
            hand.transform.position = secondHand.position + selectionPositionOffset;
            hand.transform.rotation = secondHand.rotation;
        }
        
    }

    void MoveHandSmoothly(float targetSpeed, bool fingerDetected)
    {
        if (_moveDirection != Vector3.zero) // Only move if we have a direction
        {
            float speedDifference = targetSpeed - _currentSpeed;

            if (fingerDetected) // Accelerate only if fingers are detected
            {
                _currentSpeed += Mathf.Sign(speedDifference) * acceleration * Time.deltaTime;
                _currentSpeed = Mathf.Clamp(_currentSpeed, 0, maxSpeed); // Clamp to maxSpeed
            }
            else // Decelerate if no fingers are detected
            {
                _currentSpeed += Mathf.Sign(speedDifference) * deceleration * Time.deltaTime;
                _currentSpeed = Mathf.Clamp(_currentSpeed, 0, maxSpeed); // Clamp to maxSpeed
            }

            transform.position += _moveDirection * _currentSpeed * Time.deltaTime;
        }
        else if (!fingerDetected && _currentSpeed > 0)
        {
            float speedDifference = targetSpeed - _currentSpeed;
            _currentSpeed += Mathf.Sign(speedDifference) * deceleration * Time.deltaTime;
            _currentSpeed = Mathf.Clamp(_currentSpeed, 0, maxSpeed); // Clamp to maxSpeed
            transform.position += _moveDirection * _currentSpeed * Time.deltaTime;
        }
        else if (!fingerDetected && _currentSpeed == 0)
        {
            _moveDirection = Vector3.zero;
        }

    }

    // These are for game mechanisms 
    public void UpdateStage(string newStage)
    {
        stage = newStage;
        parkourCounter.isStageChange = true;
    }

    public void StartObjectInteractionTask(Vector3 triggerPosition)
    {
        selectionTaskMeasure.isTaskStart = true;
        selectionTaskMeasure.scoreText.text = "";
        selectionTaskMeasure.partSumErr = 0f;
        selectionTaskMeasure.partSumTime = 0f;

        float tempValueY = triggerPosition.y > 0 ? 12 : 0;
        Vector3 tmpTarget = new Vector3(hmd.transform.position.x, tempValueY, hmd.transform.position.z);
        selectionTaskMeasure.taskUI.transform.LookAt(tmpTarget);
        selectionTaskMeasure.taskUI.transform.Rotate(new Vector3(0, 180f, 0));
        selectionTaskMeasure.taskStartPanel.SetActive(true);
    }

    public void CollectCoin()
    {
        parkourCounter.coinCount += 1;
        GetComponent<AudioSource>().Play();
    }
    // These are for game mechanisms 

}