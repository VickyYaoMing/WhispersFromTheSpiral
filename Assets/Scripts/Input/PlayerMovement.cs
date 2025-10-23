using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Speed")]
    [SerializeField] private float m_standingSpeed = 5;
    [SerializeField] private float m_crouchingSpeed = 2;

    private readonly float m_standingColliderHeight = 2f;
    private readonly float m_crouchingColliderHeight = 0.5f;
    private readonly float m_standingColliderCenterY = 0;
    private readonly float m_crouchingColliderCenterY = -0.5f;
    private CharacterController controller;
    private bool m_isCrouching;

    #region Unity Methods
    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }
    #endregion

    public void ProcessMove(Vector2 input)
    {
        if (GameManager.Instance.IsSaving || GameManager.Instance.IsLoading) return;
        Vector3 moveDir = Vector3.zero;
        moveDir.x = input.x;
        moveDir.z = input.y;

        float speed = m_isCrouching ? m_crouchingSpeed : m_standingSpeed;
        controller.Move(speed * Time.deltaTime * transform.TransformDirection(moveDir));
    }
    public void Crouch()
    {
        m_isCrouching = !m_isCrouching;
        float height = m_isCrouching ? m_crouchingColliderHeight : m_standingColliderHeight;
        float centerY = m_isCrouching ? m_crouchingColliderCenterY : m_standingColliderCenterY;

        Vector3 controllerCenter = controller.center;
        controllerCenter.y = centerY;

        controller.height = height;
        controller.center = controllerCenter;
    }
}
