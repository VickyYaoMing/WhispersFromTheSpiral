using UnityEngine;

public class SecondaryInteractionItem : InteractableBase
{
    protected InteractionManager interactionManager;
    private void Start()
    {
        HasSecondaryInteraction = true;
    }

    private void Awake()
    {
        interactionManager = GameManager.Instance.InteractionManager;
    }

    public virtual void SecondaryInteraction()
    {
        return;
    }

}
