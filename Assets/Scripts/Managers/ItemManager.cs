using JetBrains.Annotations;
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
       // Debug.Log(currentItems.Count);
       // foreach (var item in currentItems)
       // {
       //     Debug.Log(item);
       // }
    }

    public void Save(ref ItemManagerSaveData data)
    {
        List<ItemSaveData> ItemSaveDataList = new List<ItemSaveData>();

        for(int i = currentItems.Count() - 1; i >= 0; i--)
        {
            if (currentItems[i] != null)
            {
                //Saves info properly, just needs to move it
                GameObject Item = currentItems[i].gameObject;
                ItemSaveData itemSaveData = new ItemSaveData
                {
                    item = Item,
                    itemPosition = Item.transform.position
                };

                ItemSaveDataList.Add(itemSaveData);
            }
            else
            {
                Debug.Log("Item at" + i + "is null");
                currentItems.RemoveAt(i);
            }
        }

        data.Items = ItemSaveDataList.ToArray();
    }

    public void Load(ItemManagerSaveData data)
    {
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
