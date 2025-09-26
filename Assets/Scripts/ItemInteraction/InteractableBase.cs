using UnityEngine;

public abstract class InteractableBase : MonoBehaviour
{
    public bool itemShouldBeCameraLocked { get; protected set; } = false;
    public bool IsCollectible { get; protected set; } = false;
    protected bool isActive = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == StringLiterals.PLAYER_TAG)
        {
            other.GetComponent<InteractionManager>().OnItemTriggered(true, PickedUp, itemShouldBeCameraLocked);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == StringLiterals.PLAYER_TAG)
        {
            isActive = false;
            other.GetComponent<InteractionManager>().OnPlayerLeaveTrigger(false);
        }
    }

    public virtual GameObject PickedUp()
    {
        isActive = true;
        return gameObject;
    }

    
}
