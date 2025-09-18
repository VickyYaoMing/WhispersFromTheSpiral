using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{

    [SerializeField] private InteractionManager interactionManager;
    [SerializeField] private PlayerLook playerLook;
    [SerializeField] private PlayerInput input;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private InputManager inputManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        #region doesn't work, but it would be nice to have all of this collected here
        //input = new();
        //playerLook = this.AddComponent<PlayerLook>();
        //playerMovement = this.AddComponent<PlayerMovement>();
        //interactionManager = this.AddComponent<InteractionManager>();
        //inputManager = this.AddComponent<InputManager>();
        //
        //interactionManager.SetHandSlot(GameObject.Find("palm.01.R").transform);
        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Awake()
    {
        GameManager.Instance.Player = this;
        Debug.Log(transform.position);
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