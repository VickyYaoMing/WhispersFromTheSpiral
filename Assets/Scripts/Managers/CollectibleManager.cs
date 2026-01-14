using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;


public class CollectibleManager : MonoBehaviour
{
    //Metric ton of duplicate code between this and the item manager. It's almost 1 to 1 if not for the fact that they use different types. I could probably refactor this into using
    //Default_Item or something along the lines of that and use one singular manager to save and load everything.
    [SerializeField] List<CollectibleItem> currentCollectibles = new List<CollectibleItem>();
    private Dictionary<GameObject, GameObject> collectibleToPrefabMap = new Dictionary<GameObject, GameObject>();
    [SerializeField] List<GameObject> collectiblePrefabs = new List<GameObject>();

    #region Unity methods
    void Start()
    {
        PopulatePrefabList();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Awake()
    {
        Init();
    }

    void Update()
    {
        
    }
    #endregion

    public void PopulatePrefabList()
    {
        var prefabsToLoad = Resources.LoadAll("NotePrefabs");
        foreach (var prefab in prefabsToLoad)
        {
            collectiblePrefabs.Add(prefab.GameObject());
        }
    }

    public void RefreshCollectibleList()
    {
        currentCollectibles.Clear();
        currentCollectibles = FindObjectsByType<CollectibleItem>(default).ToList();
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        Init();
    }

    private void Init()
    {
        GameManager.Instance.CollectibleManager = this;
        foreach (var collectible in currentCollectibles)
        {
            GameObject prefab = GetPrefabForCollectible(collectible.gameObject);
            RefreshCollectibleList();
            if (prefab != null)
            {
                collectibleToPrefabMap[collectible.gameObject] = prefab;
            }
        }
    }

    private GameObject GetPrefabForCollectible(GameObject sceneItem)
    {
        //Find the prefab for the item
        foreach (var prefab in collectiblePrefabs)
        {
            if (prefab.name == sceneItem.name.Replace("(Clone)", ""))
            {
                return prefab;
            }
        }
        return null;
    }

    public void Save(ref CollectibleManagerSaveData data)
    {
        List<CollectibleSaveData> CollectibleSaveDataList = new List<CollectibleSaveData>();
        RefreshCollectibleList();

        collectibleToPrefabMap.Clear();

        foreach (var collectible in currentCollectibles)
        {
            GameObject prefab = GetPrefabForCollectible(collectible.gameObject);
            if (prefab != null)
            {
                collectibleToPrefabMap[collectible.gameObject] = prefab;
            }
        }
        for (int i = currentCollectibles.Count() - 1; i >= 0; i--)
        {            
            GameObject Collectible = currentCollectibles[i].gameObject;
            CollectibleSaveData collectibleSaveData = new CollectibleSaveData
            {
                collectiblePrefab = collectibleToPrefabMap[Collectible],
                hasBeenCollected = currentCollectibles[i].hasBeenCollected
            };

            CollectibleSaveDataList.Add(collectibleSaveData);            
        }

        data.collectibleSaveData = CollectibleSaveDataList.ToArray();
    }

    public void Load(CollectibleManagerSaveData data)
    {
        RefreshCollectibleList();

        foreach (var collectible in currentCollectibles)
        {
            //if collectible is not null, destroy it so it's overwritten by the new instances
            //expand this to make sure it doesnt destroy the notes in the notebook
            if (collectible != null)
            {
                Destroy(collectible.gameObject);
            }
        }

        RefreshCollectibleList();

        foreach (var savedCollectible in data.collectibleSaveData)
        {
            //if the collectible has a prefab & hasn't been picked up we just instantiate it
            if (savedCollectible.collectiblePrefab != null && !savedCollectible.hasBeenCollected)
            {
                GameObject spawnedCollectible = Instantiate(savedCollectible.collectiblePrefab, savedCollectible.collectiblePrefab.transform.position, savedCollectible.collectiblePrefab.transform.rotation);
                spawnedCollectible.name.Replace("(Clone)", "");
                currentCollectibles.Add(spawnedCollectible.GetComponent<CollectibleItem>());
                collectibleToPrefabMap[spawnedCollectible] = savedCollectible.collectiblePrefab;
            }
        }

        RefreshCollectibleList();
    }

}

[System.Serializable]
public struct CollectibleSaveData
{
    public GameObject collectiblePrefab;
    public bool hasBeenCollected;
}
[System.Serializable]
public struct CollectibleManagerSaveData
{
    public CollectibleSaveData[] collectibleSaveData;
}
