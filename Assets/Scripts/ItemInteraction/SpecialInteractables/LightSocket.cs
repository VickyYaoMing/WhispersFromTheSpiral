using JetBrains.Annotations;
using System.Collections;
using UnityEngine;

public class LightSocket : CameraLock_Item
{
    private Lightbulb lightbulb;
    [SerializeField] private LayerMask socketMask;
    bool lightOn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        itemShouldBeCameraLocked = true;
    }

    public override GameObject PickedUp()
    {
        GameObject itemInPlayerHand = GameManager.Instance.InteractionManager.GetItemInHand();
        lightbulb.GetComponent<Light>().enabled = true;

        if (itemInPlayerHand == null) return base.PickedUp();


        Debug.Log("This is how far we got last time");
        if (itemInPlayerHand.GetComponent<Lightbulb>() != null)
        {
            Debug.Log("Please tell me this code ran twice");
            //Check if lightbulb in hand; If lightbulb, disable current bulb, and enable the new one.
            if (lightbulb)
            {
                lightbulb.GetComponent<Light>().enabled = false;
            }
            lightbulb = itemInPlayerHand.GetComponent<Lightbulb>();
            lightbulb.enabled = false;

            UVVisibleObject[] UVVisibleObjects = GetComponentsInChildren<UVVisibleObject>(true);
            Debug.Log("We are looking at all the UV Visible objects");

            PlaceObjectInLightSocket(itemInPlayerHand);
            StartCoroutine(AwaitBulbAnimation());


            //If lightbulb is the UV bulb, show the details visible under UV light.
            if (!lightbulb.IsUV)
            {
                foreach (var UVObject in UVVisibleObjects)
                {
                    UVObject.gameObject.SetActive(false);
                }
            }
            else
            {
                foreach (var UVObject in UVVisibleObjects)
                {
                    UVObject.gameObject.SetActive(true);
                }
            }
        }
        Debug.Log("Last thing before dropping");
        //GameManager.Instance.InteractionManager.DropItemInHand();
        return base.PickedUp();
    }

    public void PlaceObjectInLightSocket(GameObject bulbObject)
    {
        //Next steps:
        //Set lightbulb gameobject to active;
        //Parent its position to the socket;
        //Remove it from the player inventory;

        bulbObject.SetActive(true);
        //this transform position is a bum and a fraud (rewrite with a proper preset vector3)
        bulbObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - 1, gameObject.transform.position.z);
    }

    IEnumerator AwaitBulbAnimation()
    {
        yield return new WaitForSeconds(3);
        GameManager.Instance.InteractionManager.AutoReleaseCameraLock();

    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //    RaycastHit hit;
        //    GameObject itemInPlayerHand = GameManager.Instance.InteractionManager.GetItemInHand();
        //
        //    if (Physics.Raycast(ray, out hit, 20f, socketMask) && hit.collider.gameObject == gameObject)
        //    {
        //        if (itemInPlayerHand == null) return;
        //
        //        Debug.Log("This is how far we got last time");
        //        if(itemInPlayerHand.GetComponent<Lightbulb>() != null)
        //        {
        //            Debug.Log("Please tell me this code ran twice");
        //            //Check if lightbulb in hand; If lightbulb, disable current bulb, and enable the new one.
        //            if(lightbulb)
        //            {
        //                lightbulb.GetComponent<Light>().enabled = false;
        //            }
        //            lightbulb = itemInPlayerHand.GetComponent<Lightbulb>();
        //
        //            //Next steps:
        //            //Set lightbulb gameobject to active;
        //            //Parent its position to the socket;
        //            //Remove it from the player inventory;
        //            lightbulb.gameObject.SetActive(true);
        //            lightbulb.gameObject.transform.position = gameObject.transform.position;
        //            lightbulb.GetComponent<Light>().enabled = true;
        //
        //            UVVisibleObject[] UVVisibleObjects = GetComponentsInChildren<UVVisibleObject>(true);
        //            Debug.Log("We are looking at all the UV Visible objects");
        //
        //            //If lightbulb is the UV bulb, show the details visible under UV light.
        //            if (!lightbulb.IsUV)
        //            {
        //                foreach(var  UVObject in UVVisibleObjects)
        //                {
        //                    UVObject.gameObject.SetActive(false);
        //                    GameManager.Instance.InteractionManager.AutoReleaseCameraLock();
        //                }
        //                return;
        //            }
        //
        //            foreach(var UVObject in UVVisibleObjects)
        //            {
        //                UVObject.gameObject.SetActive(true);
        //            }
        //
        //            GameManager.Instance.InteractionManager.AutoReleaseCameraLock();
        //        }
        //    }
        //}
    }
}
