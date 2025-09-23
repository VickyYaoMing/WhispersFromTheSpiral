using UnityEngine;
using UnityEngine.InputSystem;
using System;
using static UnityEditor.Progress;
using System.Collections;

public class InteractionManager : MonoBehaviour
{
    private int availableHoldingItems = 3;
    private int currentItemSpot = 0;
    [SerializeField] private GameObject arms;
    private int currentTotalItems = 0;
    private bool itemTriggered = false;
    private bool currentHandAvailable = true;
    private Func<GameObject> currentItemCallback;
    [SerializeField] private Transform handSlot;
    private GameObject currentItem = null;
    private GameObject[] itemArray;
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
        arms.SetActive(false);
        lockItem = true;
        inputManager.enabled = false;
        playerLook.LockCameraOnItem(currentItem.transform);
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
            arms.SetActive(true);
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

}
