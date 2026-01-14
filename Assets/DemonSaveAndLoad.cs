using UnityEngine;

public class DemonSaveAndLoad : MonoBehaviour
{
    public void Awake()
    {
        GameManager.Instance.Demon = this;
    }

    public void Save(ref DemonSaveData saveData)
    {
        saveData.position = transform.position;
        saveData.rotation = transform.rotation;
        Debug.Log("demon pos " + saveData.position);
        Debug.Log("demon rot " + saveData.rotation);
    }

    public void Load(DemonSaveData saveData)
    {
        transform.position = saveData.position;
        transform.rotation = saveData.rotation;
    }
}
[System.Serializable]
public struct DemonSaveData
{
    public Vector3 position;
    public Quaternion rotation;
}
