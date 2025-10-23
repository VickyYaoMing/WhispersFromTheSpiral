using UnityEngine;
using System;
using System.Collections;
using System.Drawing;
using UnityEngine.InputSystem.XR.Haptics;

public class InteractionManager : MonoBehaviour
{
    [SerializeField] private Transform handSlot;
    [SerializeField] private GameObject currentItem = null;
    [SerializeField] private GameObject[] itemArray;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private int howFarYouCanPlaceItem = 3;

    private InteractableBase currentItemInteractBase = null;

    public delegate void CollectibleHandler(GameObject collectible);
    public event CollectibleHandler OnCollectibleFound;

    private int currentItemSpot = 0;
    private int currentTotalItems = 0;
    private bool currentHandAvailable = true;
    private Vector3 objectOffset = new Vector3(-0.001f, 0.0004f, 0);
    private bool lockItem = false;
    private InputManager inputManager;
    //private PlayerLook playerLook;
    private Movement playerMovement;

    private void Start()
    {
        itemArray = new GameObject[3];
        inputManager = GetComponent<InputManager>();
        playerMovement = GetComponent<Movement>();
        //playerLook = GetComponent<PlayerLook>();
    }

    private void Awake()
    {
        GameManager.Instance.InteractionManager = this;
    }

    public void GetItemInInventory(int spot)
    {
        if (lockItem) return;
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
    public void OnInteractWithItem(GameObject detectedItem)
    {
        if (currentHandAvailable) 
        {
            OnPickUp(detectedItem);  
        }
      
        else if (!currentHandAvailable)
        {
            OnSwap(detectedItem);
        }
    }

    private void OnPickUp(GameObject rayHitObject)
    {
        currentItem = rayHitObject;
        currentItemInteractBase = currentItem.GetComponent<InteractableBase>();
        lockItem = currentItemInteractBase.itemShouldBeCameraLocked;
        if (currentItem == null) return;
        currentItemInteractBase.PickedUp();
        if (currentItemInteractBase.IsCollectible)
        {
            OnCollectibleFound?.Invoke(currentItem);
            return;
        }
        if (lockItem)
        {
            OnItemCameraLock();
            return;
        }
        currentItemInteractBase.enabled = true;
        itemArray[currentItemSpot] = currentItem;
        currentTotalItems++;
        currentHandAvailable = false;
        ItemPhysics(true);
        currentItem.transform.SetParent(handSlot.transform);
        currentItem.transform.localPosition = objectOffset;
    }

    private void OnItemCameraLock()
    {
        Reticle.Instance.SetActivity(false);
        if (itemArray[currentItemSpot]!=null) itemArray[currentItemSpot].SetActive(false);
        inputManager.enabled = false;
        playerMovement.LockCameraOnItem(currentItem.transform, currentItemInteractBase.howCloseFromFront, currentItemInteractBase.aboveZoomClose, currentItemInteractBase.upwardTilt, currentItemInteractBase.zoomFromFront);
        currentItem.GetComponent<InteractableBase>().enabled = true;
    }

    private void Update()
    {
        AbortCameraLock();
        RayCastItem();
    }

    private void AbortCameraLock()
    {
        if (lockItem && Input.GetKeyDown(KeyCode.Escape))
        {
            inputManager.enabled = true;
            playerMovement.UnlockCamera();
            currentItem.GetComponent<InteractableBase>().enabled = false;
            lockItem = false;
            currentItem = itemArray[currentItemSpot];
            if (currentItem != null) currentItem.SetActive(true);
            Reticle.Instance.SetActivity(true);
        }
    }

    private void RayCastItem()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, howFarYouCanPlaceItem)) return;

        bool isHitAnInteractable = (interactableLayer.value & (1 << hit.collider.gameObject.layer)) != 0;

        if (isHitAnInteractable) { Reticle.Instance.SetSprite(ReticleState.InteractableItem); }
        else { Reticle.Instance.SetSprite(ReticleState.Default); }

        if (Input.GetMouseButtonDown(0) && !lockItem)
        {
            if (isHitAnInteractable)
            {
                GameObject currentInteractable = hit.collider.gameObject;
                if (currentInteractable != null)
                {
                    OnInteractWithItem(currentInteractable);
                }
            }
            else if (!currentHandAvailable)
            {
                Vector3 placePos = hit.point + hit.normal * 0.01f;
                OnDrop(placePos);
            }
        }
    }

    private void OnDrop(Vector3 placePos)
    {
        currentHandAvailable = true;
        itemArray[currentItemSpot] = null;
        currentTotalItems--;
        currentItem.transform.SetParent(null);
        currentItem.transform.position = placePos;
        ItemPhysics(false);
        currentItemInteractBase.enabled = false;
        currentItem = null;
    }
    private void OnSwap(GameObject detectedItem)
    {
        if (detectedItem.GetComponent<InteractableBase>().itemShouldBeCameraLocked)
        {
            OnPickUp(detectedItem);
            return;
        }
        OnDrop(detectedItem.transform.position);
        OnPickUp(detectedItem);
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

    public GameObject GetCurrentItem()
    {
        return currentItem;
    }

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
