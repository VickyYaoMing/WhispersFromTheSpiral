using System;
using UnityEngine;

public class AmmoInteraction : InteractableBase
{
    public static Action AmmoPickedUp;
    private void Start()
    {
        isAmmo = true;
    }

    public override GameObject PickedUp()
    {
        Debug.Log("Ammo picked up");
        AmmoPickedUp?.Invoke();
        return gameObject;
    }


}
