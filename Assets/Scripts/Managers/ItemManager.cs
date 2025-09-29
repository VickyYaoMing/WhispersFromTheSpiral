using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Default_Item[] Items;
    void Start()
    {
        
    }

    void Awake()
    {
        GameManager.Instance.ItemManager = this;
        Items = FindObjectsByType<Default_Item>(default);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Save(ref ItemSaveData data)
    {
        foreach (var item in Items)
        {
            item.Save(ref data);
        }
    }

    public void Load(ItemSaveData data)
    {
        foreach (var item in Items)
        {
            if (!item == data.item) continue;
            item.Load(data);
        }
    }
}
