using System.Collections;
using UnityEngine;
using SanitySystem;
using System.Linq;
using System.Collections.Generic;


public class SafeManager : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.SafeManager = this;
    }

    public void Save(ref SafeManagerSaveData saveData) 
    {
        List<SafeSaveData> saveDataList = new List<SafeSaveData>();
        SafeInteraction[] safes = FindObjectsByType<SafeInteraction>(default);
        for (int i = 0; i < safes.Length; i++)
        {
            SafeSaveData data = new SafeSaveData()
            {
                unlocked = safes[i].Unlocked
            };
            saveDataList.Add(data);
        }
        saveData.safeSaves = saveDataList.ToArray();
    }

    public void Load(SafeManagerSaveData saveData)
    {
        SafeInteraction[] safes = FindObjectsByType<SafeInteraction>(default);
        for (int i = 0; i < safes.Length; i++)
        {
            if (saveData.safeSaves[i].unlocked)
            {
                safes[i].Unlock();
            }
        }

    }
}
[System.Serializable]
public struct SafeSaveData
{
    public bool unlocked;
}
[System.Serializable]
public struct SafeManagerSaveData
{
    public SafeSaveData[] safeSaves;
}