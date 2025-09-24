using UnityEngine;

public class Default_Item : InteractableBase
{
    public void Save(ref ItemSaveData data)
    {
        data.itemPosition = transform.position;
    }

    public void Load(ItemSaveData data)
    {
        transform.position = data.itemPosition;
    }
}

[System.Serializable]
public struct ItemSaveData
{
    public GameObject item;
    public Vector3 itemPosition;
}
