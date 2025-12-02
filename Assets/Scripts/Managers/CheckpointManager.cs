using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    private Checkpoint[] Checkpoints;
    private int currentCheckpointID;
    [SerializeField] private GameObject checkpointPrefab;
    [SerializeField] private GameObject checkpointParent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void Awake()
    {
        RefreshCheckpointList();
        for (int i = 0; i < Checkpoints.Length; i++)
        {
            Checkpoints[i].SetID(i);
        }
        GameManager.Instance.CheckpointManager = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RefreshCheckpointList()
    {
        Checkpoints = FindObjectsByType<Checkpoint>(default);
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

    public void Save(ref PlayerSaveData saveData)
    {
        saveData.position = Checkpoints[currentCheckpointID].GetSpawnPosition();
    }

}
