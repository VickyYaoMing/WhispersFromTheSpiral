using UnityEngine;

public abstract class InteractableBase : MonoBehaviour
{
    [Header("Camera Behavior")]
    [SerializeField] public bool itemShouldBeCameraLocked { get; protected set; } = false;
    [SerializeField] public bool zoomFromFront = false;

    //Offsets for front zooming
    //Adjust how close you want it and if it should be slighted tilted from above
    //Ignore if it isnt a camera zoom item
    [SerializeField] public float howCloseFromFront = 1.5f;
    [SerializeField] public float upwardTilt = 0;

    //How close from above zoom
    [SerializeField] public float aboveZoomClose = 1.1f;



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
