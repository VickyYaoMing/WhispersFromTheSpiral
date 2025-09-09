using UnityEngine;

public class InputManager : MonoBehaviour
{
    private PlayerInput player_input;
    private PlayerInput.On_FootActions on_foot;
    private PlayerMovement player_movement;
    private PlayerLook player_look;

    void Awake()
    {
        player_input = new PlayerInput();
        player_movement = GetComponent<PlayerMovement>();
        player_look = GetComponent<PlayerLook>();
        on_foot = player_input.On_Foot;
        on_foot.Jump.performed += ctx => player_movement.Jump();
    }

    private void FixedUpdate()
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
    }

    void OnDisable()
    {
        on_foot.Disable();
    }
}
