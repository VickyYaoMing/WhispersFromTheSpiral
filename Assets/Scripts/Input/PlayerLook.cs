using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [SerializeField]
    private Camera camera;
    private float xRotation = 0f;
    private float xSensitivity = 30f;
    private float ySensitivity = 30f;
    
    public void ProcessLook(Vector2 input)
    {
        float mouseX = input.x;
        float mouseY = input.y;

        xRotation -= (mouseY * Time.deltaTime) * ySensitivity;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
        camera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * (mouseX * Time.deltaTime) * xSensitivity);
    }
}
