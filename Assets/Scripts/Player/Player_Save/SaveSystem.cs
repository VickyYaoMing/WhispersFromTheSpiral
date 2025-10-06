using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using UnityEditorInternal;

public class SaveSystem
{
    private static SaveData _saveData = new SaveData();

    [System.Serializable]
    public struct SaveData
    {
        public PlayerSaveData PlayerData;
        public PlayerInventoryData InventoryData;
        public ItemManagerSaveData ItemManagerSaveData;
    }
    
    public static string SaveFileName()
    {
        string saveFile = Application.persistentDataPath + "/save" + ".save";
        return saveFile;
    }

    #region Async Save

    public static async Task SaveAsynchronously()
    {
        await SaveAsync();
    }

    private static async Task SaveAsync()
    {
        HandleSaveData();
        
        await File.WriteAllTextAsync(SaveFileName(), JsonUtility.ToJson(_saveData, true));
    }

    #endregion

    #region Async Load
    public static async Task LoadAsynchronously()
    {
        await LoadAsync();
    }

    public static async Task LoadAsync()
    {
        string saveContent = File.ReadAllText(SaveFileName());

        _saveData = JsonUtility.FromJson<SaveData>(saveContent);

        await HandleLoadDataAsync();
    }

    private static async Task HandleLoadDataAsync()
    {
        await GameManager.Instance.Player.Load(_saveData.PlayerData);
        GameManager.Instance.InteractionManager.Load(_saveData.InventoryData);
        GameManager.Instance.ItemManager.Load(_saveData.ItemManagerSaveData);
    }

    #endregion

    public static void Save()
    {
        HandleSaveData();

        File.WriteAllText(SaveFileName(), JsonUtility.ToJson(_saveData, true));
    }

    public static void HandleSaveData()
    {
        GameManager.Instance.CheckpointManager.Save(ref _saveData.PlayerData);
        GameManager.Instance.InteractionManager.Save(ref _saveData.InventoryData);
        GameManager.Instance.ItemManager.Save(ref _saveData.ItemManagerSaveData);
    }

    public static void Load()
    {
        string saveContent = File.ReadAllText(SaveFileName());

        _saveData = JsonUtility.FromJson<SaveData>(saveContent);

        HandleLoadData();

    }

    public static void HandleLoadData()
    {
        GameManager.Instance.InteractionManager.Load(_saveData.InventoryData);
        GameManager.Instance.ItemManager.Load(_saveData.ItemManagerSaveData);
    }

}
