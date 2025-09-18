using UnityEngine;

public abstract class InteractableBase : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == StringLiterals.PLAYER_TAG)
        {
            other.GetComponent<InteractionManager>().OnItemTriggered(true, PickedUp);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == StringLiterals.PLAYER_TAG)
        {
            other.GetComponent<InteractionManager>().OnPlayerLeaveTrigger(false);
        }
    }

    public virtual GameObject PickedUp()
    {
        return gameObject;
    }

    
}
