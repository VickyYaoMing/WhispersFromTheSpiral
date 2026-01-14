using JetBrains.Annotations;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class LightSocket : SecondaryInteractionItem
{
    [SerializeField] private Lightbulb lightbulb;
    Vector3 lightPosition;
    Quaternion lightRotation;
    UVVisibleObject[] UVVisibleObjects;
    bool objectsActivated;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HasSecondaryInteraction = true;
        lightPosition = new Vector3(0f, 4.2f, 0.049f);
        lightRotation = Quaternion.identity;
        UVVisibleObjects = GetComponentsInChildren<UVVisibleObject>(true);
        GameManager.Instance.UVLightPuzzle = this;
    }

    private void Update()
    {
        if (lightbulb)
        {
            UVObjectActivation(lightbulb.IsUV);
        }
        else
        {
            //Band-aid fix for the time being, until I find a better way to
            //turn off all UV objects when no lightbulb is in the socket
            UVObjectActivation(false);
        }
    }

    public override void SecondaryInteraction()
    {
        //This if-tree is a little messier than i'd like but if it works dont fix it or something

        GameObject itemInPlayerHand = interactionManager.GetItemInHand();

        //player holding item
        if (itemInPlayerHand != null)
        {
            //if item is not a lightbulb, return
            if (itemInPlayerHand.GetComponent<Lightbulb>() == null) return;

            //if there is a lightbulb in the socket, swap them
            if (lightbulb)
            {
                SwapLightbulbs(lightbulb, itemInPlayerHand.GetComponent<Lightbulb>());
            }
            else
            {
                ScrewLightbulbInSocket(itemInPlayerHand.GetComponent<Lightbulb>());
            }
        }
        else
        {
            //Player *not* holding lightbulb
            if (lightbulb)
            {
                //pick up the lightbulb
                TakeLightbulbFromSocket(lightbulb);
            }
        }
        
        //Coroutine to wait for an animation to happen or finish.
        //StartCoroutine(AwaitBulbAnimation());
    }

    public void SwapLightbulbs(Lightbulb currentBulb, Lightbulb playerBulb)
    {
        ScrewLightbulbInSocket(playerBulb);
        TakeLightbulbFromSocket(currentBulb);
        lightbulb = playerBulb;
    }

    public void ScrewLightbulbInSocket(Lightbulb bulb)
    {
        //If the player is holding a lightbulb, place it into the socket.
        bulb.IsInUse = true;
        bulb.GetComponent<Light>().enabled = true;
        //bulb.enabled = false;
        interactionManager.PlaceItemInHand(transform.position, Quaternion.Euler(new Vector3(90,0,0)));
        lightbulb = bulb;
        lightbulb.transform.parent = transform;
        lightbulb.transform.localPosition = new Vector3(0, -0.00628999993f, 0);
    }

    public void TakeLightbulbFromSocket(Lightbulb bulb)
    {
        bulb.IsInUse = true;
        bulb.GetComponent<Light>().enabled = false;
        interactionManager.OnPickUp(bulb.gameObject);
        lightbulb = null;
        bulb.IsInUse = false;
    }

    public void UVObjectActivation(bool isUV)
    {
        //If lightbulb is the UV bulb, show the details visible under UV light.
        if (!isUV && objectsActivated)
        {
            foreach (var UVObject in UVVisibleObjects)
            {
                UVObject.gameObject.SetActive(false);
            }
            objectsActivated = false;
        }
        else if (isUV && !objectsActivated)
        {
            foreach (var UVObject in UVVisibleObjects)
            {
                UVObject.gameObject.SetActive(true);
            }
            objectsActivated = true;
        }
    }

    IEnumerator AwaitBulbAnimation()
    {
        yield return new WaitForSeconds(3);
        interactionManager.ReleaseCameraLock();
    }

    public void Save(ref LightSocketSaveData saveData)
    {
        if (lightbulb)
        {
            saveData.hasLightbulb = true;
        }
        else
        {
            saveData.hasLightbulb = false;
        }
    }

    public void Load(LightSocketSaveData saveData)
    {
        lightbulb = null;

        if (saveData.hasLightbulb)
        {
            //only 1 lightbulb in scene so we just find it
            //if i wanna scale this either i gotta turn it into an itempedestal or i gotta instantiate specifically for this
            //for the scope of the game this is enough
            Lightbulb savedBulb = FindFirstObjectByType<Lightbulb>();

            if (savedBulb != null)
            {
                GameObject bulbObject = savedBulb.gameObject;

                savedBulb.IsInUse = true;
                savedBulb.GetComponent<Light>().enabled = true;

                bulbObject.transform.SetParent(null);
                bulbObject.transform.position = transform.position;
                bulbObject.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
                bulbObject.GetComponent<Rigidbody>().isKinematic = true;
                bulbObject.GetComponent<Rigidbody>().detectCollisions = false;

                lightbulb = savedBulb;
                lightbulb.transform.SetParent(transform);
                lightbulb.transform.localPosition = new Vector3(0, -0.00628999993f, 0);
                Debug.Log(bulbObject.name);
            }

            Debug.Log(lightbulb);
        }
    }

}
[System.Serializable] 
public struct LightSocketSaveData
{
    public bool hasLightbulb;
}
