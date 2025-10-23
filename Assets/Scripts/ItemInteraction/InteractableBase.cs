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

    public bool IsCollectible { get; protected set; } = false;
    protected bool isActive = false;
   

    public virtual GameObject PickedUp()
    {
        return gameObject;
    }

    
}
