using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveSystem
{
    private static SaveData _saveData = new();

    [System.Serializable]
    public struct SaveData
    {
        public PlayerSaveData PlayerSave;
    }
    
    public static string SaveFileName()
    {
        string saveFile = Application.persistentDataPath + "/save" + ".txt";
        return saveFile;
    }

    public static void Save()
    {
        HandleSaveData();

        File.WriteAllText(SaveFileName(), JsonUtility.ToJson(_saveData, true));
    }

    public static void HandleSaveData()
    {
        GameManager.Instance.Player.Save(ref _saveData.PlayerData);
    }

    public static void Load()
    {
        string saveContent = File.ReadAllText(SaveFileName());

        _saveData = JsonUtility.FromJson<SaveData>(saveContent);

        HandleLoadData();

    }

    public static void HandleLoadData()
    {
        GameManager.Instance.Player.Load(_saveData.PlayerData);

    }

}
