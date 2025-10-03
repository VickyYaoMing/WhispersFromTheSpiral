using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{

    //private InteractionManager interactionManager; //dont need to serialize since i just use getcomponent. apply to the rest of the private fields?
    //[SerializeField] private PlayerLook playerLook;
    //[SerializeField] private PlayerInput input;
    //[SerializeField] private PlayerMovement playerMovement;
    //[SerializeField] private InputManager inputManager;

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
    }

    public void Save(ref PlayerSaveData data)
    {
        //data.position = transform.position;
    }

    public async Task Load(PlayerSaveData data)
    {
        //Works in Async.

        //Disable the player movement and controller so that the loaded position data isn't overwritten.
        var characterController = GetComponent<CharacterController>();
        var playerMovement = GetComponent<PlayerMovement>();

        if (characterController != null) characterController.enabled = false;
        if (playerMovement != null) playerMovement.enabled = false;

        await SetTransformLoad(data);

        //wait one frame (just in case)
        await Task.Delay(1);

        //Re-enable controller and movement
        characterController.Move(Vector3.zero);
        
        playerMovement.enabled = true;
    }

    public async Task SetTransformLoad(PlayerSaveData data)
    {
        //Loads player position asynchronously to prevent overwrite.
        transform.position = data.position;
        await Task.Yield();
    }

}

[System.Serializable]
public struct PlayerSaveData
{
    public Vector3 position;
}