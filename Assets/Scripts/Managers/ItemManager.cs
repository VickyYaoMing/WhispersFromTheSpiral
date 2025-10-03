using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public List<Default_Item> currentItems;

    void Start()
    {
        
    }

    void Awake()
    {
        GameManager.Instance.ItemManager = this;
        currentItems = FindObjectsByType<Default_Item>(default).ToList();
        foreach (var item in currentItems)
        {
            Debug.Log(item);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Save(ref ItemManagerSaveData data)
    {
        List<ItemSaveData> ItemSaveDataList = new List<ItemSaveData>();

        //Decrement so we can remove items that are null or exist in the inventory
        for(int i = currentItems.Count() - 1; i >= 0; i--)
        {
            //If item is not null and is not in the player inventory, make an ItemSaveData instance for it and add it to the list. Else, remove it from the currentItems list
            if (currentItems[i] != null && !GameManager.Instance.Player.GetComponent<InteractionManager>().isItemInInventory(currentItems[i]))
            {
                GameObject Item = currentItems[i].gameObject;
                ItemSaveData itemSaveData = new ItemSaveData
                {
                    item = Item,
                    itemPosition = Item.transform.position
                };

                ItemSaveDataList.Add(itemSaveData);
            }
            else if (currentItems[i] == null) 
            {
                Debug.Log("Item at" + i + "is null");
                currentItems.RemoveAt(i);
            }
            else
            {
                Debug.Log("Item at" + i + "is in inventory");
                currentItems.RemoveAt(i);
            }
        }

        data.Items = ItemSaveDataList.ToArray();
    }

    public void Load(ItemManagerSaveData data)
    {
        //Change the load method such that it instantiates prefabs, then the whole save/load system should be done.
        foreach(var item in currentItems)
        {
            if(item == null)
            {
                Debug.Log("Item destroyed");
                Destroy(item);
            }
        }

        foreach (var savedItem in data.Items)
        {
            if (savedItem.item != null)
            {
                foreach(var existingItem in currentItems)
                {
                    if(existingItem == savedItem.item)
                    {
                        existingItem.transform.position = savedItem.itemPosition;
                    }
                }
            }
        }

    }

}

[System.Serializable]
public struct ItemSaveData
{
    public GameObject item;
    public Vector3 itemPosition;
}

[System.Serializable] 
public struct ItemManagerSaveData
{
    public ItemSaveData[] Items;
}
