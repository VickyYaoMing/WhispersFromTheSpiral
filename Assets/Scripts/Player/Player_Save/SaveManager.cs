using System.Data.Common;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Save(ref PlayerSaveData data)
    {
        data.position = transform.position;
    }

    public void Load(PlayerSaveData data)
    {
        transform.position = data.position;
    }

}

[System.Serializable]
public struct PlayerSaveData
{
    public Vector3 position;
    
}
