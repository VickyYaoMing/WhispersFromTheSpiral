using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    int ID;
    Vector3 spawnPosition;
    Vector3 spawnOffset;
    [SerializeField] Vector3 presetSpawn;

    private void Start()
    {
        spawnOffset = new Vector3(0, 2, 0);
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
        GameManager.Instance.SaveAsync();
        gameObject.SetActive(false);
    }

    public void SetID(int number)
    {
        ID = number;
    }

    public Vector3 GetSpawnPosition()
    {
        return spawnPosition;
    }

    public void Save(ref PlayerSaveData saveData)
    {
        saveData.position = spawnPosition;
    }
}
