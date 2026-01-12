using UnityEngine;

public class CameraBob : MonoBehaviour
{
    private bool enabled = true;
    [SerializeField, Range(0, 0.1f)] private float amplitude = 0.015f;
    [SerializeField, Range(0, 30)] private float frequency = 10.0f;

    [SerializeField] private Transform _camera = null;
    [SerializeField] private Transform _cameraHolder = null;

    private float toggleSpeed = 3f;
    private Vector3 startPos;
    private CharacterController controller;

    [SerializeField] float focusDistance = 15.0f;

    #region Unity Methods
    void Start()
    {
        
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        startPos = _camera.localPosition;
    }

    void Update()
    {
        if (!enabled) return;
        CheckMotion();
        _camera.LookAt(FocusTarget());
    }
    #endregion

    private void CheckMotion()
    {
        float speed = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;

        if (speed < toggleSpeed || !controller.isGrounded)
        {
            ResetPosition();
            return;
        }

        PlayMotion(FootstepMotion());
    }

    private void PlayMotion(Vector3 motion)
    {
        _camera.localPosition += motion;
    }

    private Vector3 FootstepMotion()
    {
        Vector3 pos = Vector3.zero;

        //Don't forget to tweak these values. also maybe subtly increase freq/amp when panic goes up?
        pos.y += Mathf.Sin(Time.time * frequency) * amplitude;

        pos.x += Mathf.Cos(Time.time * frequency / 2) * amplitude * 2;

        return pos;
    }

    private void ResetPosition()
    {
        if (_camera.localPosition == startPos) return;
        _camera.localPosition = Vector3.Lerp(_camera.localPosition, startPos, 1*Time.deltaTime);
    }

    private Vector3 FocusTarget()
    {
        Vector3 pos = new Vector3(transform.position.x, transform.position.y + _cameraHolder.localPosition.y, transform.position.z);
        pos += _cameraHolder.forward * focusDistance;
        return pos;
    }
}
