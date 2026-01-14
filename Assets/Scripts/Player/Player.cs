using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using SanitySystem;

public class Player : MonoBehaviour
{

    //private InteractionManager interactionManager; //dont need to serialize since i just use getcomponent. apply to the rest of the private fields?
    //[SerializeField] private PlayerLook playerLook;
    //[SerializeField] private PlayerInput input;
    //[SerializeField] private PlayerMovement playerMovement;
    //[SerializeField] private InputManager inputManager;

    public bool holdingDoorHandle;

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

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        GameManager.Instance.Player = this;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Init();
    }

    public void Save(ref PlayerSaveData data)
    {
        data.sanityPhaseCap = GetComponent<Sanity>().Cap01;
        data.stress = GetComponent<StressController>().GetStressValue();

        Debug.Log("Phase Cap" + data.sanityPhaseCap);
        Debug.Log("Stress" + data.stress);
    }

    public async Task Load(PlayerSaveData data)
    {
        //Works in Async.

        //Disable the player movement and controller so that the loaded position data isn't overwritten.
        var characterController = GetComponent<CharacterController>();
        var playerMovement = GetComponent<Movement>();

        if (characterController != null) characterController.enabled = false;
        if (playerMovement != null) playerMovement.enabled = false;

        await SetTransformLoad(data);

        //wait one frame (just in case)
        await Task.Delay(1);

        GetComponent<Sanity>().SetPhaseCap(data.sanityPhaseCap);
        GetComponent<StressController>().SetStressValue(data.stress);

        //Re-enable controller and movement
        characterController.enabled = true;
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
    public float sanityPhaseCap;
    public float stress;
    public Vector3 position;
}