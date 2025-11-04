using UnityEngine;

public class SecondaryInteractionItem : InteractableBase
{
    private void Start()
    {
        HasSecondaryInteraction = true;
    }

    public virtual void SecondaryInteraction()
    {
        return;
    }

}
