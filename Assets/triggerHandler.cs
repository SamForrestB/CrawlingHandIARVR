using UnityEngine;

public class TriggerHandler : MonoBehaviour
{
    public MyGrab myGrab; // Reference to the MyGrab script

    void OnTriggerEnter(Collider other)
    {
        if (!myGrab)
        {
            Debug.LogWarning("MyGrab reference is not assigned in TriggerHandler!");
            return;
        }

        Debug.Log($"TriggerEnter detected with: {other.gameObject.name} (Tag: {other.gameObject.tag})");


        if (other.gameObject.CompareTag("objectT"))
        {
            Debug.Log("objectT tag detected - Setting isInCollider to true");
            myGrab.isInCollider = true;
            myGrab.selectedObj = other.gameObject;
            Debug.Log($"Selected object set to: {myGrab.selectedObj.name}");
        }
        else if (other.gameObject.CompareTag("selectionTaskStart"))
        {
            Debug.Log("selectionTaskStart tag detected");
            if (!myGrab.selectionTaskMeasure.isCountdown)
            {
                myGrab.selectionTaskMeasure.isTaskStart = true;
                myGrab.selectionTaskMeasure.StartOneTask();
            }
        }
        else if (other.gameObject.CompareTag("done") && myGrab.isSelected != true)
        {
            Debug.Log("done tag detected - Ending task");
            myGrab.selectionTaskMeasure.isTaskStart = false;
            myGrab.selectionTaskMeasure.EndOneTask();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!myGrab) return;

        if (other.gameObject.CompareTag("objectT"))
        {
            myGrab.isInCollider = false;
            myGrab.selectedObj = null;
        }
    }
}