using UnityEngine;

public class LocomotionOnTrigger : MonoBehaviour
{
    public LocomotionTechnique locomotionTechnique;
    void OnTriggerEnter(Collider other)
    {

        // These are for the game mechanism.

        if (other.CompareTag("banner"))
        {
            locomotionTechnique.UpdateStage(other.gameObject.name);
        }
        else if (other.CompareTag("objectInteractionTask"))
        {
            locomotionTechnique.StartObjectInteractionTask(other.transform.position);
            other.gameObject.SetActive(false);
        }
        else if (other.CompareTag("coin"))
        {
            locomotionTechnique.CollectCoin();
            other.gameObject.SetActive(false); 
        }
        // These are for the game mechanism.
    }
}
