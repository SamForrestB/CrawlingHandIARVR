using UnityEngine;

public class MyGrab : MonoBehaviour
{
    public OVRHand hand;
    public bool isInCollider;
    public bool isSelected;
    public GameObject selectedObj;
    public GameObject anchor;
    public SelectionTaskMeasure selectionTaskMeasure;
    private bool isThumbTouchingFinger = false;
    private Transform originalParent;

    public LocomotionTechnique locomotionTechnique;

    void Update()
    {
        if (locomotionTechnique != null && locomotionTechnique.currentMode == LocomotionTechnique.LocomotionMode.Selection)
        {
            if (isInCollider && selectedObj != null)
            {
                if (!isSelected && hand.GetFingerIsPinching(OVRHand.HandFinger.Index))
                {
                    isSelected = true;
                    selectedObj.transform.SetParent(anchor.transform, true);
                }
                else if (isSelected && !hand.GetFingerIsPinching(OVRHand.HandFinger.Index))
                {
                    isSelected = false;
                    selectedObj.transform.SetParent(null, true);
                }
            }
        }
        // If not in Selection mode and object is selected, release it
        else if (isSelected)
        {
            isSelected = false;
            selectedObj.transform.SetParent(null, true);
            selectedObj = null; 
        }
    }
}


//    void OnTriggerEnter(Collider other)
//    {
//        if (other.gameObject.CompareTag("objectT"))
//        {
//            isInCollider = true;
//            selectedObj = other.gameObject;
//        }
//        else if (other.gameObject.CompareTag("selectionTaskStart"))
//        {
//            if (!selectionTaskMeasure.isCountdown)
//            {
//                selectionTaskMeasure.isTaskStart = true;
//                selectionTaskMeasure.StartOneTask();
//            }
//        }
//        else if (other.gameObject.CompareTag("done"))
//        {
//            selectionTaskMeasure.isTaskStart = false;
//            selectionTaskMeasure.EndOneTask();
//        }
//    }

//    void OnTriggerExit(Collider other)
//    {
//        if (other.gameObject.CompareTag("objectT"))
//        {
//            isInCollider = false;
//            selectedObj = null;
//        }
//    }
//}