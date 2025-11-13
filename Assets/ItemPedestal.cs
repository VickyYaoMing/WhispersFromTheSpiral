 using UnityEngine;

public class ItemPedestal : SecondaryInteractionItem
{
    bool isCorrectItem = false;
    Default_Item item = null;

    string thisTag;

    [SerializeField] Default_Item correctItem;

    Vector3 itemPosition = Vector3.zero;
    Quaternion itemRotation = Quaternion.identity;

    private void Start()
    {
        HasSecondaryInteraction = true;
        thisTag = gameObject.tag;
    }

    public override void SecondaryInteraction()
    {
        if(!interactionManager.GetItemInHand().GetComponent<Default_Item>()) return;

        Default_Item itemInPlayerHand = interactionManager.GetItemInHand().GetComponent<Default_Item>();

        if (!itemInPlayerHand) return;

        if (!itemInPlayerHand.CompareTag(thisTag)) return;

        itemInPlayerHand.IsInUse = true;
        
        
        interactionManager.PlaceItemInHand(itemPosition, itemRotation);
        item = itemInPlayerHand;


    }

}
