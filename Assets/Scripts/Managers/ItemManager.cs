using NUnit.Framework;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ItemManager : MonoBehaviour
{
    public List<Default_Item> currentItems;
    private Dictionary<GameObject, GameObject> itemToPrefabMap = new Dictionary<GameObject, GameObject>();
    [SerializeField] private List<GameObject> itemPrefabs;

    #region Unity Methods
    void Start()
    {
        PopulatePrefabList();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Awake()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {

    }
    #endregion

    private void Init()
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

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Init();
    }

    private void RefreshItemList()
    {
        currentItems = FindObjectsByType<Default_Item>(default).ToList();
    }

    private void PopulatePrefabList()
    {
        //This code only works in the editor. Now fixed using Resources.LoadAll!
        //string[] files = Directory.GetFiles("Assets/Prefabs/ItemPrefabs", "*.prefab", SearchOption.TopDirectoryOnly);
        //foreach (var file in files)
        //{
        //    var prefab = AssetDatabase.LoadAssetAtPath(file, typeof(GameObject));
        //    itemPrefabs.Add(prefab.GameObject());
        //}

        //Add all prefabs into the itemPrefabs list so we can load them later.
        var prefabsToLoad = Resources.LoadAll("ItemPrefabs");
        foreach (var prefab in prefabsToLoad) 
        {
            itemPrefabs.Add(prefab.GameObject());
        }
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
        //Make sure item list is up to date
        RefreshItemList();
    
        //Clean up all current items on the map
        foreach (var item in currentItems)
        {
            if(item != null && !GameManager.Instance.Player.GetComponent<InteractionManager>().isItemInInventory(item))
            {
                Destroy(item.gameObject);
            }
        }
    
        RefreshItemList();
    
        //Load up items from save. Replace the word "Clone" in the name to make sure the items can be referenced properly and to
        //keep the editor clean.
        foreach (var savedItem in data.Items)
        {
            if (savedItem.itemPrefab != null)
            {
                GameObject spawnedItem = Instantiate(savedItem.itemPrefab, savedItem.itemPosition, Quaternion.identity);
                spawnedItem.name.Replace("(Clone)", "");
                currentItems.Add(spawnedItem.GetComponent<Default_Item>());
                itemToPrefabMap[spawnedItem] = savedItem.itemPrefab;
            }
        }

        RefreshItemList();
    }
}

[System.Serializable]
public struct ItemSaveData
{
    //Consider saving transform values? or position and rotation, at least.
    public GameObject itemPrefab;
    public Vector3 itemPosition;
}

[System.Serializable] 
public struct ItemManagerSaveData
{
    public ItemSaveData[] Items;
}
