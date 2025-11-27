using UnityEngine;

public class InputManager : MonoBehaviour
{
    private PlayerInput player_input;
    private PlayerInput.On_FootActions on_foot;
    private PlayerInput.InventoryActions inventoryActions;
    private PlayerGrabController grabController;
    private CharacterController characterController;

    private Movement playerMovement;

    //private PlayerMovement player_movement;
    //private PlayerLook player_look;
    private InteractionManager interactionManager;
    private UIManager uiManager;
    private int currentScrollIndex = 0;

    void Awake()
    {
        player_input = new PlayerInput();
        grabController = GetComponent<PlayerGrabController>(); 
        characterController = GetComponent<CharacterController>(); 
        //player_movement = GetComponent<PlayerMovement>();
        //player_look = GetComponent<PlayerLook>();
        playerMovement = GetComponent<Movement>();
        interactionManager = GetComponent<InteractionManager>();
        uiManager = GetComponent<UIManager>();
        on_foot = player_input.On_Foot;
        inventoryActions = player_input.Inventory;
        on_foot.Crouch.performed += ctx => playerMovement.Crouch();
        on_foot.Exit.performed += ctx => uiManager.Exit();
        on_foot.OpenNotebook.performed += ctx => uiManager.ToggleNotebook();
        inventoryActions.Item1.performed += ctx => interactionManager.GetItemInInventory(0);
        inventoryActions.Item2.performed += ctx => interactionManager.GetItemInInventory(1);
        inventoryActions.Item3.performed += ctx => interactionManager.GetItemInInventory(2);
        inventoryActions.Scroll.performed += ctx =>
        {
            float scroll = ctx.ReadValue<float>();

            if (scroll > 0f) currentScrollIndex--; 
            else if (scroll < 0f) currentScrollIndex++; 

            if (currentScrollIndex < 0) currentScrollIndex = 2;
            if (currentScrollIndex > 2) currentScrollIndex = 0;

            interactionManager.GetItemInInventory(currentScrollIndex);
        };   
    }

    private void Update()
    {

        // Thiti was here (Got Hectors Permission) 
        if (grabController != null && (grabController.IsGrabbed || grabController.IsBeingThrown))
        {
            return;
        }

        if (uiManager.IsPaused || uiManager.IsNotebookActive || uiManager.IsViewingCollectible)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            Reticle.Instance.SetActivity(false);
            return;
        }
        Cursor.lockState = CursorLockMode.Locked;
        Reticle.Instance.SetActivity(true);
        playerMovement.UpdatePlayer(on_foot.Looking.ReadValue<Vector2>(), on_foot.Walking.ReadValue<Vector2>());
        //playerMovement.ProcessLook(on_foot.Looking.ReadValue<Vector2>());
        //playerMovement.ProcessMove(on_foot.Walking.ReadValue<Vector2>());

        //player_movement.ProcessMove(on_foot.Walking.ReadValue<Vector2>());
        //player_look.ProcessLook(on_foot.Looking.ReadValue<Vector2>());
    }

    private void LateUpdate()
    {
    }
    void OnEnable()
    {
        on_foot.Enable();
        inventoryActions.Enable();
    }

    void OnDisable()
    {
        on_foot.Disable();
        inventoryActions.Enable();
    }
}
