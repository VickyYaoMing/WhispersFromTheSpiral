using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 velocity;
    private float speed = 5;
    private bool isGrounded;
    private float gravity = -9.8f;
    private float jumpHeight = 1.5f;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }
    private void Update()
    {
        isGrounded = controller.isGrounded;
    }
    public void ProcessMove(Vector2 input)
    {
        if (GameManager.Instance.IsSaving || GameManager.Instance.IsLoading) return;
        Vector3 moveDir = Vector3.zero;
        moveDir.x = input.x;
        moveDir.z = input.y;
        controller.Move(transform.TransformDirection(moveDir) * speed * Time.deltaTime);
        velocity.y += gravity * Time.deltaTime;
        if(isGrounded && velocity.y < 0)
        {
            velocity.y = -2;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    public void Jump()
    {
        if(isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -(jumpHeight) * gravity);
        }
    }
}
