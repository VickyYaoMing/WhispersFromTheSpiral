using UnityEngine;

public class InputManager : MonoBehaviour
{
    private PlayerInput player_input;
    private PlayerInput.On_FootActions on_foot;
    private PlayerInput.InventoryActions inventoryActions;

    private PlayerMovement player_movement;
    private PlayerLook player_look;
    private InteractionManager interactionManager;
    private int currentScrollIndex = 0;


    void Awake()
    {
        player_input = new PlayerInput();
        player_movement = GetComponent<PlayerMovement>();
        player_look = GetComponent<PlayerLook>();
        interactionManager = GetComponent<InteractionManager>();
        on_foot = player_input.On_Foot;
        inventoryActions = player_input.Inventory;
        on_foot.Jump.performed += ctx => player_movement.Jump();
        on_foot.Interact.performed += ctx => interactionManager.OnInteractWithItem();
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
        player_movement.ProcessMove(on_foot.Walking.ReadValue<Vector2>());

    }

    private void LateUpdate()
    {
        player_look.ProcessLook(on_foot.Looking.ReadValue<Vector2>());
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
