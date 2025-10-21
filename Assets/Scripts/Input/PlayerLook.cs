using System.Collections;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] private Camera m_cam;
    [SerializeField] private float m_defaultSensitivity = 10f;
    [SerializeField] private float m_crouchSensitivity = 8f;
    [SerializeField] private float m_duration = 1f;
    [SerializeField] private Renderer[] m_playerMesh;

    private float m_xRotation;
    private Transform m_camOriginalParent;
    private Vector3 m_camSavedLocalPos;
    private Quaternion m_camSavedLocalRot;
    private Vector3 m_standingCamPos;
    private Vector3 m_crouchingCamPos;
    private readonly float m_crouchSpeed = 5f;
    private bool m_isCrouching;
    public bool LockCamera { get; set; } = false;

    #region Unity Methods
    private void Start()
    {
        SetMeshVisible(false);
        m_standingCamPos = m_cam.transform.localPosition;
        m_crouchingCamPos = m_standingCamPos + new Vector3(0, -1f, 0);
    }
    #endregion

    public void ProcessLook(Vector2 input)
    {
        if (LockCamera) return;

        float mouseX = input.x;
        float mouseY = input.y;

        float sensitivity = m_isCrouching ? m_crouchSensitivity : m_defaultSensitivity;
        Vector3 camPos = m_isCrouching ? m_crouchingCamPos : m_standingCamPos;

        m_xRotation -= mouseY * Time.deltaTime * sensitivity;
        m_xRotation = Mathf.Clamp(m_xRotation, -60f, 60f);

        m_cam.transform.localRotation = Quaternion.Euler(m_xRotation, 0f, 0f);
        transform.Rotate((mouseX * Time.deltaTime) * sensitivity * Vector3.up);

        m_cam.transform.localPosition = Vector3.Lerp(m_cam.transform.localPosition, camPos, Time.deltaTime * m_crouchSpeed);
    }
    public void Crouch()
    {
        m_isCrouching = !m_isCrouching;
    }

    private void SetMeshVisible(bool visible)
    {
        for(int i = 0; i < m_playerMesh.Length; i++)
        {
            m_playerMesh[i].enabled = visible;
        }
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

    //private IEnumerator ZoomCameraOnly(Transform item, float height, float duration)
    //{
    //    LockCamera = true;

    //    m_camOriginalParent = m_cam.transform.parent;
    //    m_camSavedLocalPos = m_cam.transform.localPosition;
    //    m_camSavedLocalRot = m_cam.transform.localRotation;

    //    m_cam.transform.SetParent(null, worldPositionStays: true);

    //    Vector3 startPos = m_cam.transform.position;
    //    Quaternion startRot = m_cam.transform.rotation;

    //    Vector3 targetPos = item.position + Vector3.up * height;
    //    Quaternion targetRot = Quaternion.LookRotation(Vector3.down, Vector3.forward);

    //    float t = 0f;
    //    while (t < duration)
    //    {
    //        float u = t / duration;
    //        u = u * u * (3f - 2f * u);
    //        m_cam.transform.position = Vector3.Lerp(startPos, targetPos, u);
    //        m_cam.transform.rotation = Quaternion.Slerp(startRot, targetRot, u);
    //        t += Time.deltaTime;
    //        yield return null;
    //    }

    //    m_cam.transform.position = targetPos;
    //    m_cam.transform.rotation = targetRot;
    //}

    public void UnlockCamera()
    {
        m_cam.transform.SetParent(m_camOriginalParent, worldPositionStays: false);
        m_cam.transform.localPosition = m_camSavedLocalPos;
        m_cam.transform.localRotation = m_camSavedLocalRot;

        LockCamera = false;
    }

    private IEnumerator ZoomCamera(Transform item, Vector3 targetPos, Quaternion targetRot)
    {
        LockCamera = true;

        m_camOriginalParent = m_cam.transform.parent;
        m_camSavedLocalPos = m_cam.transform.localPosition;
        m_camSavedLocalRot = m_cam.transform.localRotation;

        m_cam.transform.SetParent(null, worldPositionStays: true);

        Vector3 startPos = m_cam.transform.position;
        Quaternion startRot = m_cam.transform.rotation;


        float t = 0f;
        while (t < m_duration)
        {
            float u = t / m_duration;
            u = u * u * (3f - 2f * u); 
            m_cam.transform.position = Vector3.Lerp(startPos, targetPos, u);
            m_cam.transform.rotation = Quaternion.Slerp(startRot, targetRot, u);
            t += Time.deltaTime;
            yield return null;
        }

        m_cam.transform.position = targetPos;
        m_cam.transform.rotation = targetRot;
    }
}
