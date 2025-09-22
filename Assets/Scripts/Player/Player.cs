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

        //theoretically works, can't test because i can't grab the items in my scene :////
        data.inventory = interactionManager.GetItemArray();
        data.savedItem = interactionManager.GetCurrentItem();
    }

    public void Load(PlayerSaveData data)
    {
        transform.position = data.position;

        //same goes for these 2
        interactionManager.SetItemArray(data.inventory);
        interactionManager.SetCurrentItem(data.savedItem);
    }

}

[System.Serializable]
public struct PlayerSaveData
{
    public Vector3 position;
    public GameObject[] inventory;
    public GameObject savedItem;
}