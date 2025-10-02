using UnityEngine;

public class Checkpoint : InteractableBase
{
    [SerializeField] Vector3 spawnPosition = new Vector3(-2.3f, 5, 1);

    private void Start()
    {
        
    }

    private void Awake()
    {
        GameManager.Instance.Checkpoint = this;
    }

    public override GameObject PickedUp()
    {
        GameManager.Instance.Save();
        return base.PickedUp();
    }

    public void Save(ref PlayerSaveData saveData)
    {
        saveData.position = spawnPosition;
    }
}
