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
    private bool disableInteraction = false;
    private Movement playerMovement;
    private bool isCurrentItemAmo = false;

    private void Start()
    {
        itemArray = new GameObject[3];
        inputManager = GetComponent<InputManager>();
        playerMovement = GetComponent<Movement>();
    }

    private void OnEnable()
    {
        GunController.disableBaseInteraction += DisableInteractionTemporarily;
    }

    private void OnDisable()
    {
        GunController.disableBaseInteraction -= DisableInteractionTemporarily;

    }

    private void Awake()
    {
        GameManager.Instance.InteractionManager = this;
    }

    private void Update()
    {
        AbortCameraLock();
        RayCastItem();
    }

    private void DisableInteractionTemporarily(bool aim)
    {
        disableInteraction = aim;
    }

    public void GetItemInInventory(int spot)
    {
        if (disableInteraction) return;
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
        if (detectedItem.GetComponent<InteractableBase>().HasSecondaryInteraction)
        {
            OnSecondaryInteraction(detectedItem);
            return;
        }
        if (detectedItem.GetComponent<InteractableBase>().isAmmo)
        {
            detectedItem.GetComponent<InteractableBase>().PickedUp();
            Destroy(detectedItem);
            return;
        }
        if (currentHandAvailable) 
        {
            OnPickUp(detectedItem);  
        }
      
        else if (!currentHandAvailable)
        {
            OnSwap(detectedItem);
        }
    }

    public void OnPickUp(GameObject rayHitObject)
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
        currentItem.transform.localRotation = currentItemInteractBase.itemShouldBeRotatedWhenHeld;
    }

    private void OnDrop(Vector3 placePos, bool hasPhysics)
    {
        currentHandAvailable = true;
        itemArray[currentItemSpot] = null;
        currentTotalItems--;
        currentItem.transform.SetParent(null);
        currentItem.transform.position = placePos;
        if (hasPhysics) ItemPhysics(false);
        currentItemInteractBase.enabled = false;
        currentItem = null;
    }

    private void OnSwap(GameObject detectedItem)
    {
        if (detectedItem.GetComponent<InteractableBase>().canBePlacedUpon)
        {
            CanBePlacedUpon(detectedItem);
            return;
        }
        if (detectedItem.GetComponent<InteractableBase>().itemShouldBeCameraLocked)
        {
            OnPickUp(detectedItem);
            return;
        }
        OnDrop(detectedItem.transform.position, true);
        OnPickUp(detectedItem);
    }

    private void OnSecondaryInteraction(GameObject detectedItem)
    {
        currentItem = detectedItem;
        detectedItem.GetComponent<SecondaryInteractionItem>().SecondaryInteraction();
        Debug.Log("Secondary interaction baybee");
        currentItem = itemArray[currentItemSpot];
    }

    private void OnItemCameraLock()
    {
        Reticle.Instance.SetActivity(false);
        if (itemArray[currentItemSpot]!=null) itemArray[currentItemSpot].SetActive(false);
        inputManager.enabled = false;
        playerMovement.LockCameraOnItem(currentItem.transform, currentItemInteractBase.howCloseFromFront, currentItemInteractBase.aboveZoomClose, currentItemInteractBase.upwardTilt, currentItemInteractBase.zoomFromFront);
        currentItem.GetComponent<InteractableBase>().enabled = true;
    }

    //ReleaseCameraLock and AbortCameraLock do the same thing, but Abort is a private method that requires pressing Escape to exit cam lock.
    //Release camera lock is public and is intended to be used by objects which may have a cutscene on them (like placing an item on a specific spot)
    private void AbortCameraLock()
    {
        if (lockItem && Input.GetKeyDown(KeyCode.Escape))
        {
            ReleaseCameraLock();
        }
    }

    public void ReleaseCameraLock()
    {
        inputManager.enabled = true;
        playerMovement.UnlockCamera();
        currentItem.GetComponent<InteractableBase>().enabled = false;
        lockItem = false;
        currentItem = itemArray[currentItemSpot];
        if (currentItem != null) currentItem.SetActive(true);
        Reticle.Instance.SetActivity(true);
    }

    private void RayCastItem()
    {
        if (disableInteraction) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, howFarYouCanPlaceItem)) return;

        bool isHitAnInteractable = (interactableLayer.value & (1 << hit.collider.gameObject.layer)) != 0;

        if (isHitAnInteractable) { Reticle.Instance.SetSprite(ReticleState.InteractableItem); }
        else { Reticle.Instance.SetSprite(ReticleState.Default); }

        //Maybe do lockItem || itemInUse?
        //that bool would have to be true only for the frame where it's being placed into the socket
        if (Input.GetMouseButtonDown(0) && !lockItem)
        {
            if (isHitAnInteractable)
            {
                GameObject currentInteractable = hit.collider.gameObject;
                if (currentInteractable == null) return;

                //make sure this doesnt create edge cases when interacting with an item that is in use
                //it shouldn't since this is per item
                if (currentInteractable.GetComponent<InteractableBase>().IsInUse) return;

                if (currentInteractable != null)
                {
                    OnInteractWithItem(currentInteractable);
                }
            }
            else if (!currentHandAvailable)
            {
                Vector3 placePos = hit.point + hit.normal * 0.01f;
                OnDrop(placePos, true);
            }
        }
    }

    private void CanBePlacedUpon(GameObject detectedItem)
    {
        OnDrop(detectedItem.GetComponent<InteractableBase>().placementArea, false);
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

    public GameObject GetItemInHand()
    {
        return itemArray[currentItemSpot];
    }

    public void PlaceItemInHand(Vector3 position, Quaternion rotation)
    {
        currentItem = itemArray[currentItemSpot];
        currentHandAvailable = true;
        itemArray[currentItemSpot] = null;
        currentTotalItems--;
        currentItem.transform.SetParent(null);
        currentItem.transform.position = position;
        currentItem.transform.rotation = rotation;
        ItemPhysics(true);
        currentItemInteractBase.enabled = false;
        currentItem = null;
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
