using JetBrains.Annotations;
using UnityEngine;

public class Default_Item : InteractableBase
{

    //Add code to make the visual effect show up when in range

    public void Save(ref ItemSaveData data)
    {
        data.itemPosition = transform.position;
    }

    public void Load(ItemSaveData data)
    {
        transform.position = data.itemPosition;
    }
}


