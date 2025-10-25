using JetBrains.Annotations;
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

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 20f, socketMask))
            {
                GameObject itemInPlayerHand = GameManager.Instance.InteractionManager.GetCurrentItem();
                if(itemInPlayerHand.GetComponent<Lightbulb>() != null)
                {
                    //Check if lightbulb in hand; If lightbulb, disable current bulb, and enable the new one.
                    if(lightbulb)
                    {
                        lightbulb.GetComponent<Light>().enabled = false;
                    }
                    lightbulb = itemInPlayerHand.GetComponent<Lightbulb>();
                    lightbulb.GetComponent<Light>().enabled = true;

                    UVVisibleObject[] UVVisibleObjects = GetComponentsInChildren<UVVisibleObject>(true);

                    //If lightbulb is the UV bulb, show the details visible under UV light.
                    if (!lightbulb.IsUV)
                    {
                        foreach(var  UVObject in UVVisibleObjects)
                        {
                            UVObject.gameObject.SetActive(false);
                        }
                        return;
                    }


                    foreach(var UVObject in UVVisibleObjects)
                    {
                        UVObject.gameObject.SetActive(true);
                    }
                }
            }
        }
    }
}
