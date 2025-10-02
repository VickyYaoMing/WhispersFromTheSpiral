using UnityEngine;

public class Checkpoint : InteractableBase
{
    int ID;
    [SerializeField] Vector3 spawnPosition;

    private void Start()
    {
        
    }

    private void Awake()
    {

    }

    public override GameObject PickedUp()
    {
        GameManager.Instance.CheckpointManager.SetCurrentCheckpointID(ID);
        GameManager.Instance.SaveAsync();
        return base.PickedUp();
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
