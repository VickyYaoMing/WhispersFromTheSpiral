using NUnit.Framework;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public List<Default_Item> currentItems;
    private Dictionary<GameObject, GameObject> itemToPrefabMap = new Dictionary<GameObject, GameObject>();
    [SerializeField] private List<GameObject> itemPrefabs;

    void Start()
    {
        
    }

    void Awake()
    {
        GameManager.Instance.ItemManager = this;
        RefreshItemList();
        foreach (var item in currentItems)
        {
            GameObject prefab = GetPrefabForItem(item.gameObject);
            if (prefab != null)
            {
                itemToPrefabMap[item.gameObject] = prefab;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void RefreshItemList()
    {
        currentItems = FindObjectsByType<Default_Item>(default).ToList();
    }

    private GameObject GetPrefabForItem(GameObject sceneItem)
    {
        //Find the prefab for the item
        foreach (var prefab in itemPrefabs)
        {
            if (prefab.name == sceneItem.name.Replace("(Clone)", ""))
            {
                return prefab;
            }
        }
        return null;
    }


    public void Save(ref ItemManagerSaveData data)
    {
        Debug.Log("saving");
        List<ItemSaveData> ItemSaveDataList = new List<ItemSaveData>();
        RefreshItemList();

        itemToPrefabMap.Clear();

        foreach (var item in currentItems)
        {
            GameObject prefab = GetPrefabForItem(item.gameObject);
            if (prefab != null)
            {
                itemToPrefabMap[item.gameObject] = prefab;
            }
        }
        //Decrement so we can remove items that are null or exist in the inventory
        for (int i = currentItems.Count() - 1; i >= 0; i--)
        {
            //If item is not null and is not in the player inventory, make an ItemSaveData instance for it and add it to the list. Else, remove it from the currentItems list
            if (currentItems[i] != null && !GameManager.Instance.Player.GetComponent<InteractionManager>().isItemInInventory(currentItems[i]))
            {
                GameObject Item = currentItems[i].gameObject;
                ItemSaveData itemSaveData = new ItemSaveData
                {
                    itemPrefab = itemToPrefabMap[Item],
                    itemPosition = Item.transform.position
                };

                ItemSaveDataList.Add(itemSaveData);
            }

            //Following two if statements are separate for debug reasons. Turn them into one before production.
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
        RefreshItemList();
    
        foreach (var item in currentItems)
        {
            if(item != null && !GameManager.Instance.Player.GetComponent<InteractionManager>().isItemInInventory(item))
            {
                Debug.Log("Item destroyed");
                Destroy(item.gameObject);
            }
        }
    
        RefreshItemList();
    
        foreach (var savedItem in data.Items)
        {
            if (savedItem.itemPrefab != null)
            {
                GameObject spawnedItem = Instantiate(savedItem.itemPrefab, savedItem.itemPosition, Quaternion.identity);
                spawnedItem.name.Replace("(Clone)", "");
                currentItems.Add(spawnedItem.GetComponent<Default_Item>());
                Debug.Log("Item loaded");
                itemToPrefabMap[spawnedItem] = savedItem.itemPrefab;
            }
        }

        RefreshItemList();
    
    }
}

[System.Serializable]
public struct ItemSaveData
{
    public GameObject itemPrefab;
    public Vector3 itemPosition;
}

[System.Serializable] 
public struct ItemManagerSaveData
{
    public ItemSaveData[] Items;
}
