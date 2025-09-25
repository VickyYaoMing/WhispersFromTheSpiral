using System.Collections;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private float xSensitivity = 30f;
    [SerializeField] private float ySensitivity = 30f;
    [SerializeField] private Renderer[] playerMesh;

    private float xRotation;
    public bool lockCamera { get; set; } = false;
    private Transform camOriginalParent;
    private Vector3 camSavedLocalPos;
    private Quaternion camSavedLocalRot;

    public void ProcessLook(Vector2 input)
    {
        if (lockCamera) return;

        float mouseX = input.x;
        float mouseY = input.y;

        xRotation -= mouseY * Time.deltaTime * ySensitivity;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * (mouseX * Time.deltaTime) * xSensitivity);
    }

    private void SetMeshVisible(bool visible)
    {
        for(int i = 0; i < playerMesh.Length; i++)
        {
            playerMesh[i].enabled = visible;
        }
    }

    public void LockCameraOnItem(Transform item)
    {
        if (lockCamera) return;
        SetMeshVisible(false);
        StartCoroutine(ZoomCameraOnly(item, 1.1f, 1f));
    }

    public void UnlockCamera()
    {
        SetMeshVisible(true);
        cam.transform.SetParent(camOriginalParent, worldPositionStays: false);
        cam.transform.localPosition = camSavedLocalPos;
        cam.transform.localRotation = camSavedLocalRot;

        lockCamera = false;
    }

    private IEnumerator ZoomCameraOnly(Transform item, float height, float duration)
    {
        lockCamera = true;

        camOriginalParent = cam.transform.parent;
        camSavedLocalPos = cam.transform.localPosition;
        camSavedLocalRot = cam.transform.localRotation;

        cam.transform.SetParent(null, worldPositionStays: true);

        Vector3 startPos = cam.transform.position;
        Quaternion startRot = cam.transform.rotation;

        Vector3 targetPos = item.position + Vector3.up * height;
        Quaternion targetRot = Quaternion.LookRotation(Vector3.down, Vector3.forward);

        float t = 0f;
        while (t < duration)
        {
            float u = t / duration;
            u = u * u * (3f - 2f * u); 
            cam.transform.position = Vector3.Lerp(startPos, targetPos, u);
            cam.transform.rotation = Quaternion.Slerp(startRot, targetRot, u);
            t += Time.deltaTime;
            yield return null;
        }

        cam.transform.position = targetPos;
        cam.transform.rotation = targetRot;
    }
}
