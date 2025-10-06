using UnityEngine;

public class LockedObject : InteractableBase
{
    void Start()
    {
    }

    public override GameObject PickedUp()
    {
        isActive = true;
        return gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
