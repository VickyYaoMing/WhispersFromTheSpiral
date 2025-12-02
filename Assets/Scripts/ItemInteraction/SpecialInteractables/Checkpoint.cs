using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    int ID;
    Vector3 spawnPosition;
    Vector3 spawnOffset;
    public Vector3 checkpointPos;
    [SerializeField] Vector3 presetSpawn;

    private void Start()
    {
        spawnOffset = new Vector3(0, 2, 0);
        checkpointPos = transform.position;
    }

    private void Awake()
    {
        if (presetSpawn != Vector3.zero)
        {
            spawnPosition = presetSpawn;
            return;
        }
        spawnPosition = transform.position + spawnOffset;
    }

    private void OnTriggerEnter()
    {
        GameManager.Instance.CheckpointManager.SetCurrentCheckpointID(ID);
        Destroy(gameObject);
        GameManager.Instance.SaveAsync();
    }

    public void SetID(int number)
    {
        ID = number;
    }

    public Vector3 GetSpawnPosition()
    {
        return spawnPosition;
    }

    public void Save(ref PlayerSaveData saveData, ref CheckpointSaveData checkpointSaveData)
    {
        saveData.position = spawnPosition;
        checkpointSaveData.checkpointPos = checkpointPos;
    }
}
