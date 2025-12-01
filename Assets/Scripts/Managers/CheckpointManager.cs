using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using JetBrains.Annotations;

public class CheckpointManager : MonoBehaviour
{
    [SerializeField] private List<Checkpoint> Checkpoints;
    private CheckpointSaveData[] checkpointSaveData;
    private int currentCheckpointID = 0;
    [SerializeField] private GameObject checkpointPrefab;
    [SerializeField] private GameObject checkpointParent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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

    private void Init()
    {
        RefreshCheckpointList();
        for (int i = 0; i < Checkpoints.Count; i++)
        {
            Checkpoints[i].SetID(i);
        }
        GameManager.Instance.CheckpointManager = this;
        checkpointParent = GameObject.Find("Checkpoints");
    }

    public void RefreshCheckpointList()
    {
        Checkpoints = FindObjectsByType<Checkpoint>(default).ToList();
    }

    public void CreateNewCheckpoint(Vector3 position)
    {
        Instantiate(checkpointPrefab, position, Quaternion.identity, checkpointParent.transform);
        RefreshCheckpointList();
    }

    public void CreateNewCheckpoint(Vector3 position, Vector3 scale)
    {
        GameObject temp = Instantiate(checkpointPrefab, position, Quaternion.identity, checkpointParent.transform);
        temp.transform.localScale = scale;
        RefreshCheckpointList();
    }

    public void SetCurrentCheckpointID(int checkpointID)
    {
        currentCheckpointID = checkpointID;
    }

    public void Save(ref PlayerSaveData playerSaveData, ref CheckpointManagerSaveData checkpointSaveData)
    {
        if (Checkpoints[currentCheckpointID] == null) return;
        playerSaveData.position = Checkpoints[currentCheckpointID].GetSpawnPosition();
        RefreshCheckpointList();
        List<CheckpointSaveData> list = new List<CheckpointSaveData>();
        foreach (var checkpoint in Checkpoints)
        {
            if( checkpoint != null)
            {
                CheckpointSaveData checkpointSave = new CheckpointSaveData {
                    checkpointPos = checkpoint.checkpointPos
                };
                list.Add(checkpointSave);
            }
        }

        checkpointSaveData.savedCheckpoints = list.ToArray();
    }

    public void Load(CheckpointManagerSaveData saveData)
    {
        foreach (var checkpoint in Checkpoints)
        {  
            Destroy(checkpoint.gameObject);
        }

        RefreshCheckpointList();

        //Load up items from save. Replace the word "Clone" in the name to make sure the items can be referenced properly and to
        //keep the editor clean.
        foreach (var savedCheckpoint in saveData.savedCheckpoints)
        {
            GameObject spawnedItem = Instantiate(checkpointPrefab, savedCheckpoint.checkpointPos, Quaternion.identity);
            spawnedItem.name.Replace("(Clone)", "");
            Checkpoints.Add(spawnedItem.GetComponent<Checkpoint>());            
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Init();
    }

}

[System.Serializable]
public struct CheckpointManagerSaveData
{
    public CheckpointSaveData[] savedCheckpoints;
}

[System.Serializable]
public struct CheckpointSaveData
{
    public Vector3 checkpointPos;
}
