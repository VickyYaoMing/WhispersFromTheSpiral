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
                    if(lightbulb)
                    {
                        lightbulb.GetComponent<Light>().enabled = false;
                    }
                    lightbulb = itemInPlayerHand.GetComponent<Lightbulb>();
                    lightbulb.GetComponent<Light>().enabled = true;
                }

                //if (lightbulb == null)
                //{
                //    lightbulb = GameObject.Find("Lightbulb 1").GetComponent<Lightbulb>();
                //    Debug.Log(lightbulb);
                //}
                //if (lightbulb != null && lightOn == true)
                //{
                //    lightbulb.GetComponent<Light>().enabled = false;
                //    lightbulb = GameObject.Find("UV Lightbulb").GetComponent<Lightbulb>();
                //    Debug.Log(lightbulb);
                //}
                //
                //lightbulb.GetComponent<Light>().enabled = true;
                //lightOn = true;
            }
        }
    }
}
