using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public List<Default_Item> savedItems;

    void Start()
    {
        
    }

    void Awake()
    {
        GameManager.Instance.ItemManager = this;
        savedItems = FindObjectsByType<Default_Item>(default).ToList();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Save(ref ItemManagerSaveData data)
    {
        List<ItemSaveData> ItemSaveDataList = new List<ItemSaveData>();

        for(int i = savedItems.Count(); i > 0; i--)
        {
            if (savedItems[i] = null)
            {
                GameObject Item = savedItems[i].gameObject;
                ItemSaveData itemSaveData = new ItemSaveData
                {
                    item = Item,
                    itemPosition = Item.transform.position
                };

                ItemSaveDataList.Add(itemSaveData);
            }
            else
            {
                savedItems.RemoveAt(i);
            }
        }

        data.Items = ItemSaveDataList.ToArray();
    }

    public void Load(ItemManagerSaveData data)
    {
        foreach(var item in savedItems)
        {
            if(item == null)
            {
                Destroy(item);
            }
        }

        foreach (var savedItem in data.Items)
        {
            if (savedItem.item != null)
            {
                foreach(var existingItem in savedItems)
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
public struct ItemManagerSaveData
{
    public ItemSaveData[] Items;
}
