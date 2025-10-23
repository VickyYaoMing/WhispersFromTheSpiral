using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    int ID;
    [SerializeField] Vector3 spawnPosition;

    private void Start()
    {
        
    }

    private void Awake()
    {

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
