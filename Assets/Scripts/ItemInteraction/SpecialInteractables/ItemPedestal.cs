 using UnityEngine;

public class ItemPedestal : SecondaryInteractionItem
{
    bool isCorrectItem = false;

    public bool IsCorrectItem {  get { return isCorrectItem; } }

    [SerializeField] Default_Item itemOnPedestal;

    string thisTag;

    [SerializeField] Default_Item correctItem;
    Transform anchorPoint;

    Vector3 itemPosition = Vector3.zero;
    Quaternion itemRotation = Quaternion.identity;

    private void Start()
    {
        HasSecondaryInteraction = true;
        thisTag = gameObject.tag;
        interactionManager = GameManager.Instance.InteractionManager;
        anchorPoint = gameObject.transform;
    }

    public override void SecondaryInteraction()
    {
        if (!interactionManager.GetItemInHand())
        {
            if (!itemOnPedestal) return;

            TakeItemFromPedestal(itemOnPedestal);

            Debug.Log("Took item");
            return;

        };

        Default_Item itemInPlayerHand = interactionManager.GetItemInHand().GetComponent<Default_Item>();

        if (!itemInPlayerHand.CompareTag(thisTag)) return;

        if (itemOnPedestal)
        {
            SwapItems(itemOnPedestal, itemInPlayerHand);
            Debug.Log("Swapped");
            return;
        }

        itemInPlayerHand.IsInUse = true;

        interactionManager.PlaceItemInHand(itemPosition, itemRotation);

        PlaceItemUsingColliderBounds(itemInPlayerHand.gameObject);

        itemInPlayerHand.IsInUse = false;

        Debug.Log("Interacted");
    }


    void PlaceItemUsingColliderBounds(GameObject item) 
    { 
        item.transform.SetParent(anchorPoint);
        Renderer renderer = item.GetComponent<Renderer>();

        if (renderer == null)
        {
            Debug.Log("No renderer");
            return;
        }

        Bounds bounds = renderer.bounds;

        float verticalOffset = bounds.extents.y + anchorPoint.GetComponent<BoxCollider>().bounds.extents.y;

        item.transform.localPosition = Vector3.zero;
        item.transform.localPosition = new Vector3(0, verticalOffset, 0);

        item.transform.localRotation = Quaternion.identity;
        itemOnPedestal = item.GetComponent<Default_Item>();

        if(itemOnPedestal == correctItem)
        {
            isCorrectItem = true;
            EnableParticles();
        }
        else
        {
            isCorrectItem = false;
            DisableParticles();
        }
    }

    void TakeItemFromPedestal(Default_Item item)
    {
        item.IsInUse = true;
        interactionManager.OnPickUp(item.gameObject);
        itemOnPedestal = null;
        isCorrectItem = false;
        DisableParticles();
        item.IsInUse = false;
    }

    void SwapItems(Default_Item item, Default_Item playerItem)
    {
        TakeItemFromPedestal(item);

        PlaceItemUsingColliderBounds(playerItem.gameObject);

        itemOnPedestal = playerItem;
    }

    void EnableParticles()
    {
        gameObject.GetComponentInChildren<ParticleSystem>().Play();
    }

    void DisableParticles()
    {
        gameObject.GetComponentInChildren<ParticleSystem>().Clear();
        gameObject.GetComponentInChildren<ParticleSystem>().Pause();
    }

}
