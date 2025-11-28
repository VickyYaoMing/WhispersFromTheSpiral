using System.Collections;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] private Renderer[] m_playerMesh;

    [Header("Camera Effects & Sensitivity")]
    [SerializeField] private Camera m_camera;
    [SerializeField] private float m_defaultSensitivity = 0.1f;
    [SerializeField] private float m_crouchSensitivity = 0.05f;
    [SerializeField] private float m_trippingValue = 1f;
    [SerializeField] private bool m_isTripping;

    [Header("Head Bobbing")]
    [SerializeField] private bool m_isHeadBanging;
    [SerializeField] private float m_headBangAmount = 0.1f;
    [SerializeField] private float m_headBangFrequency = 10.0f;
    [SerializeField] private float m_headBangSmoothing = 10.0f;

    [Header("Zoom")]
    [SerializeField] private float m_duration = 1f;

    [Header("Crouch")]
    [SerializeField] private float m_crouchSpeed = 5f;

    [Header("Player Speed")]
    [SerializeField] private float m_standingSpeed = 5;
    [SerializeField] private float m_crouchingSpeed = 2;

    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerGrabController grabController;

    private readonly float m_standingColliderHeight = 2f;
    private readonly float m_crouchingColliderHeight = 0.5f;
    private readonly float m_standingColliderCenterY = 0;
    private readonly float m_crouchingColliderCenterY = -0.5f;
    private readonly float gravity = -9.8f;
    private Vector3 velocity;
    private CharacterController controller;

    private float m_xRotation;
    private float m_yRotation;
    private Transform m_camOriginalParent;
    private Vector3 m_camSavedLocalPos;
    private Quaternion m_camSavedLocalRot;
    private Vector3 m_standingCamPos;
    private Vector3 m_crouchingCamPos;
    private bool m_isCrouching;

    public bool LockCamera { get; set; } = false;
    public bool IsGrounded { get; set; }
   
    #region Unity Methods
    private void Start()
    {
        controller = GetComponent<CharacterController>();
        SetMeshVisible(false);
        m_standingCamPos = m_camera.transform.localPosition;
        m_crouchingCamPos = m_standingCamPos + new Vector3(0, -1f, 0);
    }
    #endregion

    public void UpdatePlayer(Vector2 lookInput, Vector2 moveInput)
    {
        // Thiti was here 
        if (grabController != null && (grabController.IsGrabbed || grabController.IsBeingThrown))
        {
            return;
        }

        IsGrounded = controller.isGrounded;
        ProcessLook(lookInput);
        ProcessMove(moveInput);
    }
    private void ProcessLook(Vector2 input)
    {
        if (LockCamera) return;

        float mouseX = input.x;
        float mouseY = input.y;

        float sensitivity = m_isCrouching ? m_crouchSensitivity : m_defaultSensitivity;
        Vector3 camPos = m_isCrouching ? m_crouchingCamPos : m_standingCamPos;

        if (m_isTripping)
        {
            Quaternion targetXRotation;
            Quaternion targetYRotation;

            m_yRotation += mouseX * sensitivity;
            m_xRotation -= mouseY * sensitivity;
            m_xRotation = Mathf.Clamp(m_xRotation, -60f, 60f);

            targetYRotation = Quaternion.Euler(0f, m_yRotation, 0f);
            targetXRotation = Quaternion.Euler(m_xRotation, 0f, 0f);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetYRotation, Time.deltaTime * m_trippingValue);
            m_camera.transform.localRotation = Quaternion.Slerp(m_camera.transform.localRotation, targetXRotation, Time.deltaTime * m_trippingValue);
        }
        else
        {
            m_xRotation -= mouseY * sensitivity;
            m_xRotation = Mathf.Clamp(m_xRotation, -60f, 60f);

            m_camera.transform.localRotation = Quaternion.Euler(m_xRotation, 0f, 0f);
            transform.Rotate(mouseX * sensitivity * Vector3.up);
        }

        m_camera.transform.localPosition = Vector3.Lerp(m_camera.transform.localPosition, camPos, Time.deltaTime * m_crouchSpeed);
    }
    private void ProcessMove(Vector2 input)
    {
        if (characterController == null || !characterController.enabled)
            return;

        if (GameManager.Instance.IsSaving || GameManager.Instance.IsLoading) return;
        Vector3 moveDir = Vector3.zero;
        moveDir.x = input.x;
        moveDir.z = input.y;

        float speed = m_isCrouching ? m_crouchingSpeed : m_standingSpeed;
        controller.Move(speed * Time.deltaTime * transform.TransformDirection(moveDir));
        velocity.y += gravity * Time.deltaTime;
        if (IsGrounded && velocity.y < 0)
        {
            velocity.y = -2;
        }
        if (m_isHeadBanging)
        {
            CheckMovement(input);
            StopHeadBob();
        }
        controller.Move(velocity * Time.deltaTime);
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
    public void LockCameraOnItem(Transform item, float frontClose, float aboveClose, float upwardTilt, bool zoomFromFront = false)
    {
        if (LockCamera) return;
        SetMeshVisible(false);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        Vector3 targetPos;
        Quaternion targetRot;

        if (zoomFromFront)
        {
            targetPos = item.position - item.forward * frontClose + item.up * upwardTilt;
            targetRot = Quaternion.LookRotation(item.position - targetPos, item.up);
        }
        else
        {
            targetPos = item.position + Vector3.up * aboveClose;
            targetRot = Quaternion.LookRotation(Vector3.down, Vector3.forward);
        }

        StartCoroutine(ZoomCamera(item, targetPos, targetRot));
    }
    public void UnlockCamera()
    {
        m_camera.transform.SetParent(m_camOriginalParent, worldPositionStays: false);
        m_camera.transform.localPosition = m_camSavedLocalPos;
        m_camera.transform.localRotation = m_camSavedLocalRot;

        LockCamera = false;
    }
    private void CheckMovement(Vector2 input)
    {
        float movement = new Vector3(input.x, 0f, input.y).magnitude;
        if (movement > 0f)
        {
            HeadBob();
        }
    }
    private void HeadBob()
    {
        Vector3 bobOffset = Vector3.zero;
        bobOffset.y = Mathf.Sin(Time.time * m_headBangFrequency) * m_headBangAmount;
        bobOffset.x = Mathf.Sin(Time.time * m_headBangFrequency / 2f) * m_headBangAmount;

        Vector3 targetPos = (m_isCrouching ? m_crouchingCamPos : m_standingCamPos) + bobOffset;
        m_camera.transform.localPosition = Vector3.Lerp(m_camera.transform.localPosition, targetPos, m_headBangSmoothing * Time.deltaTime);
    }
    private void StopHeadBob()
    {
        Vector3 targetCamPos = m_isCrouching ? m_crouchingCamPos : m_standingCamPos;
        if(m_camera.transform.localPosition == targetCamPos) { return ; }
        m_camera.transform.localPosition = Vector3.Lerp(m_camera.transform.localPosition, targetCamPos, 1 * Time.deltaTime);
    }
    private void SetMeshVisible(bool visible)
    {
        for (int i = 0; i < m_playerMesh.Length; i++)
        {
            m_playerMesh[i].enabled = visible;
        }
    }
    private IEnumerator ZoomCamera(Transform item, Vector3 targetPos, Quaternion targetRot)
    {
        LockCamera = true;

        m_camOriginalParent = m_camera.transform.parent;
        m_camSavedLocalPos = m_camera.transform.localPosition;
        m_camSavedLocalRot = m_camera.transform.localRotation;

        m_camera.transform.SetParent(null, worldPositionStays: true);

        Vector3 startPos = m_camera.transform.position;
        Quaternion startRot = m_camera.transform.rotation;


        float t = 0f;
        while (t < m_duration)
        {
            float u = t / m_duration;
            u = u * u * (3f - 2f * u);
            m_camera.transform.position = Vector3.Lerp(startPos, targetPos, u);
            m_camera.transform.rotation = Quaternion.Slerp(startRot, targetRot, u);
            t += Time.deltaTime;
            yield return null;
        }

        m_camera.transform.position = targetPos;
        m_camera.transform.rotation = targetRot;
    }
}
