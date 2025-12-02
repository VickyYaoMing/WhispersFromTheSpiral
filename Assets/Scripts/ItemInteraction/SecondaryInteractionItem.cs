using UnityEngine;

public class SecondaryInteractionItem : InteractableBase
{
    protected InteractionManager interactionManager;
   
    private void Awake()
    {
        if(GameManager.Instance != null)
        {
            interactionManager = GameManager.Instance.InteractionManager;
        }
        if(interactionManager == null)
        {
            interactionManager = FindFirstObjectByType<InteractionManager>();
        }
    }
    private void Start()
    {
        HasSecondaryInteraction = true;
    }

    public virtual void SecondaryInteraction()
    {
        return;
    }

}
