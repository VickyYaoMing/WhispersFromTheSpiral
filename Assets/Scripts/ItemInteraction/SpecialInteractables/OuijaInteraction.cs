using System.Collections.Generic;
using UnityEngine;

public class OuijaInteraction : InteractableBase
{
    [SerializeField] private List<GameObject> anchors;
    private PlanchetteInteraction planchette;
    private KeyCode[] allKeys;
    void Start()
    {
        itemShouldBeCameraLocked = true;
        planchette = GetComponentInChildren<PlanchetteInteraction>();
        allKeys = (KeyCode[])System.Enum.GetValues(typeof(KeyCode));
    }

    public override GameObject PickedUp()
    {
        return base.PickedUp();
    }

    public void Update()
    {
        if (!isActive) return;
        for (int i = 0; i < anchors.Count; i++)
        {
            KeyCode key = (KeyCode)System.Enum.Parse(typeof(KeyCode), anchors[i].name);
            if (Input.GetKeyDown(key))
            {
                planchette.MoveToTarget(anchors[i].transform);
            }
        }
    }


}
