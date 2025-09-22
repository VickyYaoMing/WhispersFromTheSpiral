using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [SerializeField]
    private Camera cam;
    private float xRotation = 0f;
    private float xSensitivity = 30f;
    private float ySensitivity = 30f;
    private Vector3 storedPlayerPos;
    private Quaternion storedPlayerRot;
    private float storedXRotation;
    public bool lockCamera { get; set; } = false;

    public void ProcessLook(Vector2 input)
    {
        if (lockCamera) return;
        float mouseX = input.x;
        float mouseY = input.y;

        xRotation -= (mouseY * Time.deltaTime) * ySensitivity;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * (mouseX * Time.deltaTime) * xSensitivity);
    }

    public void LockCameraOnItem(Transform item)
    {
        storedPlayerPos = transform.position;
        storedPlayerRot = transform.rotation;
        storedXRotation = xRotation;
        StartCoroutine(ZoomTowardsTarget(item, 1.4f, 1f));
    }

    public void UnlockCamera()
    {
        transform.position = storedPlayerPos;
        transform.rotation = storedPlayerRot;
        xRotation = storedXRotation;
        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        lockCamera = false;
    }
    private IEnumerator ZoomTowardsTarget(Transform item, float height, float duration)
    {
        lockCamera = true;

        Vector3 camLocalPos = cam.transform.localPosition;
        Quaternion camLocalRot = cam.transform.localRotation;

        Quaternion camWorldTargetRot = Quaternion.LookRotation(Vector3.down, Vector3.forward);

        Vector3 camWorldTargetPos = item.position + Vector3.up * height;

        Quaternion playerTargetRot = camWorldTargetRot * Quaternion.Inverse(camLocalRot);
        Vector3 playerTargetPos = camWorldTargetPos - (playerTargetRot * camLocalPos);

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        float t = 0f;
        while (t < duration)
        {
            float u = t / duration;
            u = u * u * (3f - 2f * u); 

            transform.position = Vector3.Lerp(startPos, playerTargetPos, u);
            transform.rotation = Quaternion.Slerp(startRot, playerTargetRot, u);

            t += Time.deltaTime;
            yield return null;
        }

        transform.position = playerTargetPos;
        transform.rotation = playerTargetRot;

        cam.transform.localRotation = camLocalRot;
    }
}
