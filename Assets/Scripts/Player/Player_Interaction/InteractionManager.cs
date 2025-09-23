using UnityEngine;
using UnityEngine.InputSystem;
using System;
using static UnityEditor.Progress;

public class InteractionManager : MonoBehaviour
{
    private int availableHoldingItems = 3;
    private int currentItemSpot = 0;

    private int currentTotalItems = 0;
    private bool itemTriggered = false;
    private bool currentHandAvailable = true;
    private Func<GameObject> currentItemCallback;
    [SerializeField] private Transform handSlot;
    [SerializeField] private GameObject currentItem = null;
    [SerializeField] private GameObject[] itemArray;
    private Vector3 objectOffset = new Vector3(-0.001f, 0.0004f, 0);

    private void Start()
    {
        itemArray = new GameObject[3];
    }

    private void Awake()
    {
        GameManager.Instance.InteractionManager = this;
    }

    public void OnItemTriggered(bool isItemTriggered, Func<GameObject> callback)
    {
        itemTriggered = isItemTriggered;
        currentItemCallback = callback;

    }
    public void OnPlayerLeaveTrigger(bool isItemTriggered)
    {
        itemTriggered = isItemTriggered;
    }
    public void GetItemInInventory(int spot)
    {
        Debug.Log("Inventory spot: " + spot);
        if(currentItemSpot != spot)
        {
            if(currentItem != null)
            {
                currentItem.SetActive(false);
                
            }
            if (itemArray[spot] != null)
            {
                currentItem = itemArray[spot];
                currentItem.SetActive(true);
                currentHandAvailable = false;

            }
            if (itemArray[spot] == null)
            {
                currentHandAvailable = true;
                currentItem = null;
            }

            currentItemSpot = spot;
        }
    }
    public void OnInteractWithItem()
    {

        if(itemTriggered && currentHandAvailable && currentTotalItems < availableHoldingItems) 
        {
            OnPickUp();  
        }
        else if (!currentHandAvailable && !itemTriggered)
        {
            OnDrop();
        }
        else if(itemTriggered && !currentHandAvailable)
        {
            OnSwap();
        }
    }

    private void OnPickUp()
    {
        currentItem = currentItemCallback?.Invoke();
        if (currentItem == null) return;
        itemArray[currentItemSpot] = currentItem;
        currentTotalItems++;
        currentHandAvailable = false;
        itemTriggered = false;
        ItemPhysics(true);
        currentItem.transform.SetParent(handSlot.transform);
        currentItem.transform.localPosition = objectOffset;
    }

    private void OnDrop()
    {
        currentHandAvailable = true;
        itemArray[currentItemSpot] = null;
        currentTotalItems--;
        currentItem.transform.SetParent(null);
        currentItem.transform.position = transform.position + transform.forward * 1f;
        ItemPhysics(false);

        currentItem = null;
    }
    private void OnSwap()
    {
        OnDrop();
        OnPickUp();
    }

    private void ItemPhysics(bool isGoingToBePickedUp)
    {
        if (isGoingToBePickedUp)
        {
            currentItem.GetComponent<Rigidbody>().isKinematic = true;
            currentItem.GetComponent<Rigidbody>().detectCollisions = false;
        }
        else
        {
            currentItem.GetComponent<Rigidbody>().isKinematic = false;
            currentItem.GetComponent<Rigidbody>().detectCollisions = true;
        }

    }

    public void SetHandSlot(Transform handSlot)
    {
        this.handSlot = handSlot;
    }

    #region Methods for save and load

    public void Save(ref PlayerInventoryData data)
    {
        data.inventory = itemArray;
        data.currentItemIndex = currentItemSpot;
    }

    public void Load(PlayerInventoryData data) 
    {
        //Works, but doesn't move the item into the character's hand.
        //Item position is memorized and the current item is too.
        //If item is in slot 0, it is teleported into the player's hand and works as intended. 
        //If it is not in slot 0, it will not be teleported into the player's hand and will instead remain on the floor.

        itemArray = data.inventory;

        currentItem = data.inventory[data.currentItemIndex];
        currentItemSpot = data.currentItemIndex;

        foreach (GameObject item in itemArray)
        {
            if (item == null) continue;

            item.transform.SetParent(handSlot.transform);
            item.transform.localPosition = objectOffset;
            item.GetComponent<Rigidbody>().isKinematic = true;
            item.GetComponent<Rigidbody>().detectCollisions = false;
        }

    }

    #endregion
}

[System.Serializable] 
public struct PlayerInventoryData
{
    public GameObject[] inventory;
    public int currentItemIndex;
}
