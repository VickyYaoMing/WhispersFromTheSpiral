using JetBrains.Annotations;
using UnityEngine;

public class Default_Item : InteractableBase
{
    //Saving and loading like this only works for one (1) object. Consider moving everything from here into the ItemManager, and the ItemSaveData struct holding a list of both objects and vectors. Maybe tuples.
    public void Save(ref ItemSaveData data)
    {
        data.itemPosition = transform.position;
    }

    public void Load(ItemSaveData data)
    {
        transform.position = data.itemPosition;
    }
}


