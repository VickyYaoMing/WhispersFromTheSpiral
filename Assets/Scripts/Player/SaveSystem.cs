using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;

public class SaveSystem
{
    [System.Serializable]
    public struct SaveData
    {
        public LanternSaveData LanternSaveData;
        public DemonSaveData DemonSaveData;
        public PlayerSaveData PlayerData;
        public PlayerInventoryData InventoryData;
        public ItemManagerSaveData ItemManagerSaveData;
        public CollectibleManagerSaveData CollectibleManagerSaveData;
        public CheckpointManagerSaveData CheckpointManagerSaveData;
        public bool hasSaved;
    }

    private static SaveData _saveData = new SaveData();

    public static bool CheckForSave()
    {
        if(SaveFileName() == null)
        {
            Debug.Log("No save file found");
            return false;
        }
        string saveContent = File.ReadAllText(SaveFileName());

        _saveData = JsonUtility.FromJson<SaveData>(saveContent);

        Debug.Log("Checked for save. Save exists? " + DoesSaveExist());
        return DoesSaveExist();
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
        GameManager.Instance.CheckpointManager.Load(_saveData.CheckpointManagerSaveData);
        await GameManager.Instance.Player.Load(_saveData.PlayerData);
        GameManager.Instance.InteractionManager.Load(_saveData.InventoryData);
        GameManager.Instance.ItemManager.Load(_saveData.ItemManagerSaveData);   
        GameManager.Instance.CollectibleManager.Load(_saveData.CollectibleManagerSaveData);
        GameManager.Instance.Lantern.Load(_saveData.LanternSaveData);
        GameManager.Instance.Demon.Load(_saveData.DemonSaveData);
    }

    #endregion

    public static void Save()
    {
        HandleSaveData();

        File.WriteAllText(SaveFileName(), JsonUtility.ToJson(_saveData, true));
    }

    public static void HandleSaveData()
    {
        GameManager.Instance.CheckpointManager.Save(ref _saveData.PlayerData, ref _saveData.CheckpointManagerSaveData);
        GameManager.Instance.InteractionManager.Save(ref _saveData.InventoryData);
        GameManager.Instance.ItemManager.Save(ref _saveData.ItemManagerSaveData);
        GameManager.Instance.CollectibleManager.Save(ref _saveData.CollectibleManagerSaveData);
        GameManager.Instance.Lantern.Save(ref _saveData.LanternSaveData);
        GameManager.Instance.Player.Save(ref _saveData.PlayerData);
        GameManager.Instance.Demon.Save(ref _saveData.DemonSaveData);

        if (!_saveData.hasSaved)
        {
            _saveData.hasSaved = true;
        }
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

    public static bool DoesSaveExist()
    {
        return _saveData.hasSaved;
    }

}
