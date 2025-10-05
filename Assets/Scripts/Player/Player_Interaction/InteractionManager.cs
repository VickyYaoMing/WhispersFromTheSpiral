using UnityEngine;
using System;
using System.Collections;

public class InteractionManager : MonoBehaviour
{
    public delegate void CollectibleHandler(GameObject collectible);
    public event CollectibleHandler OnCollectibleFound;
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
    private bool lockItem = false;
    private bool isTriggerALockItem = false;
    private InputManager inputManager;
    private PlayerLook playerLook;

    private void Start()
    {
        itemArray = new GameObject[3];
        inputManager = GetComponent<InputManager>();
        playerLook = GetComponent<PlayerLook>();
    }

    private void Awake()
    {
        GameManager.Instance.InteractionManager = this;
    }

    public void OnItemTriggered(bool isItemTriggered, Func<GameObject> callback, bool isTriggerALockItem)

    {
        itemTriggered = isItemTriggered;
        this.isTriggerALockItem = isTriggerALockItem;
        currentItemCallback = callback;

    }
    public void OnPlayerLeaveTrigger(bool isItemTriggered)
    {
        itemTriggered = isItemTriggered;
    }
    public void GetItemInInventory(int spot)
    {
        if (lockItem) return;
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
        if (currentItem.GetComponent<InteractableBase>().IsCollectible)
        {
            OnCollectibleFound?.Invoke(currentItem);
            itemTriggered = false;
            return;
        }
        if (currentItem.GetComponent<InteractableBase>().itemShouldBeCameraLocked)
        {
            OnItemCameraLock();
            return;
        }
        currentItem.GetComponent<InteractableBase>().enabled = true;
        itemArray[currentItemSpot] = currentItem;
        currentTotalItems++;
        currentHandAvailable = false;
        itemTriggered = false;
        ItemPhysics(true);
        currentItem.transform.SetParent(handSlot.transform);
        currentItem.transform.localPosition = objectOffset;
    }

    private void OnItemCameraLock()
    {
        lockItem = true;
        inputManager.enabled = false;
        InteractableBase interactBase = currentItem.GetComponent<InteractableBase>();
        playerLook.LockCameraOnItem(currentItem.transform, interactBase.howCloseFromFront, interactBase.aboveZoomClose, interactBase.upwardTilt, interactBase.zoomFromFront);
        StartCoroutine(EnableAfterRelease());
    }

    private IEnumerator EnableAfterRelease()
    {
        yield return null;
        currentItem.GetComponent<InteractableBase>().enabled = true;

    }

    private void Update()
    {
        if(lockItem && Input.GetKeyDown(KeyCode.Escape))
        {
            inputManager.enabled = true;
            playerLook.UnlockCamera();
            currentItem.GetComponent<InteractableBase>().enabled = false;
            lockItem = false;
            currentItem = itemArray[currentItemSpot];
        }
    }

    private void OnDrop()
    {
        currentHandAvailable = true;
        itemArray[currentItemSpot] = null;
        currentTotalItems--;
        currentItem.transform.SetParent(null);
        currentItem.transform.position = transform.position + transform.forward * 1f;
        ItemPhysics(false);
        currentItem.GetComponent<InteractableBase>().enabled = false;
        currentItem = null;
    }
    private void OnSwap()
    {
        if(isTriggerALockItem)
        {
            OnPickUp();
            return;
        }
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
        //Item position is memorized and the current item is too.
        //Item is teleported into the player's hand and works as intended. 

        itemArray = data.inventory;

        currentItem = data.inventory[data.currentItemIndex];
        currentItemSpot = data.currentItemIndex;

        foreach (GameObject item in itemArray)
        {
            if (item == null) continue;

            //Refactor this into a method? Pretty much the same code that runs when picking an item up.
            currentTotalItems++;
            item.transform.SetParent(handSlot.transform);
            item.transform.localPosition = objectOffset;
            item.GetComponent<Rigidbody>().isKinematic = true;
            item.GetComponent<Rigidbody>().detectCollisions = false;

            if (item == currentItem)
            {
                currentHandAvailable = false;
            }
            else
            {
                item.SetActive(false);
            }
        }
    }

    public bool isItemInInventory(InteractableBase item)
    {
        foreach (GameObject inventoryItem in itemArray)
        {
            if (inventoryItem == item.gameObject)
            {
                return true;
            }
        }
        return false;
    }
    #endregion
}

[System.Serializable] 
public struct PlayerInventoryData
{
    public GameObject[] inventory;
    public int currentItemIndex;
}
