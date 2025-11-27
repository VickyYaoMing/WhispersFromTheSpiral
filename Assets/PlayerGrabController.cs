using System;
using UnityEngine;
using System.Collections;

public class PlayerGrabController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator;
    [SerializeField] private Movement movement;
    [SerializeField] private Rigidbody rb;

    [Header("Grab Settings")]
    [SerializeField] private float disableControlTime = 1.5f;
    [SerializeField] private float throwForce = 15f; // Increased force
    [SerializeField] private float verticalForce = 8f; // Increased vertical force
    [SerializeField] private float grabFollowSpeed = 8f;

    [Header("Throw Settings")]
    [SerializeField] private float throwDuration = 1f;
    [SerializeField] private AnimationCurve throwCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private enum GrabState { None, Grabbed, Thrown }
    private GrabState currentState = GrabState.None;

    private Transform grabber;
    private Vector3 grabOffset;
    private Vector3 throwDirection;
    private float throwTimer;
    private Vector3 throwStartPosition;

    public bool IsGrabbed => currentState == GrabState.Grabbed;
    public bool IsBeingThrown => currentState == GrabState.Thrown;

    public void StartGrab(Transform grabberTransform, Vector3 grabberPosition)
    {
        if (currentState != GrabState.None) return;

        currentState = GrabState.Grabbed;
        grabber = grabberTransform;

        // Calculate offset from grabber
        grabOffset = transform.position - grabber.position;

        // Disable movement
        if (movement != null)
            movement.enabled = false;

        // Face the grabber
        Vector3 lookDir = (grabberPosition - transform.position);
        lookDir.y = 0;
        if (lookDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(lookDir);

        // Setup physics - make sure we're kinematic during grab
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
        }

        // Disable character controller during grab
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        animator.SetTrigger("Grabbed");
        Debug.Log("Player grabbed by demon");
    }

    public void ApplyThrow(Vector3 grabberForward)
    {
        if (currentState != GrabState.Grabbed) return;

        currentState = GrabState.Thrown;

        // Simple direct throw
        Vector3 throwVector = (grabberForward * throwForce) + (Vector3.up * verticalForce);

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(throwVector, ForceMode.Impulse); // Try Impulse instead of VelocityChange
            Debug.Log($"Throw impulse: {throwVector}");
        }
        else
        {
            // Manual throw as fallback
            StartCoroutine(ManualThrow(throwVector));
        }

        StartCoroutine(ThrowCountdown());
    }

    private IEnumerator ManualThrow(Vector3 throwVector)
    {
        float timer = 1f;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            transform.position += throwVector * Time.deltaTime;
            // Apply gravity
            throwVector.y -= Physics.gravity.y * Time.deltaTime;
            yield return null;
        }
    }
    private IEnumerator ThrowCountdown()
    {
        yield return new WaitForSeconds(disableControlTime);
        EndGrab();
    }

    private void Update()
    {
        switch (currentState)
        {
            case GrabState.Grabbed:
                UpdateGrabbed();
                break;
            case GrabState.Thrown:
                UpdateThrown();
                break;
        }
    }

    private void UpdateGrabbed()
    {
        if (grabber == null) return;

        // Smoothly follow the grabber with offset
        Vector3 targetPosition = grabber.position + grabber.TransformDirection(grabOffset);
        transform.position = Vector3.Lerp(transform.position, targetPosition, grabFollowSpeed * Time.deltaTime);
    }

    private void UpdateThrown()
    {
        if (rb == null)
        {
            // Manual throw movement if no rigidbody (fallback)
            throwTimer -= Time.deltaTime;
            if (throwTimer > 0)
            {
                float curveValue = throwCurve.Evaluate(1f - (throwTimer / throwDuration));
                Vector3 newPosition = throwStartPosition +
                                    (throwDirection * throwForce * curveValue * Time.deltaTime) +
                                    (Vector3.up * verticalForce * curveValue * Time.deltaTime);
                transform.position = newPosition;
            }
        }
        else
        {
            // Let physics handle the throw, just track time
            throwTimer -= Time.deltaTime;
        }
    }

    public void ParentTo(Transform newParent, Vector3 localPos, Vector3 localEuler)
    {
        // Not used in this approach
    }

    public void Unparent()
    {
        // Not used in this approach
    }

    private void EndGrab()
    {
        if (currentState == GrabState.None) return;

        Debug.Log("Ending grab state");
        currentState = GrabState.None;
        grabber = null;

        // Re-enable movement
        if (movement != null)
        {
            movement.enabled = true;
            Debug.Log("Movement re-enabled");
        }

        // Re-enable character controller
        if (characterController != null)
        {
            characterController.enabled = true;
        }

        // Ensure physics is proper
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        // Stop all coroutines
        StopAllCoroutines();
    }

    public void ForceRelease()
    {
        EndGrab();
    }

    // Debug visualization
    private void OnDrawGizmos()
    {
        if (currentState == GrabState.Thrown && throwDirection != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, throwDirection * 3f);
        }
    }

    private void OnDisable()
    {
        if (currentState != GrabState.None)
        {
            ForceRelease();
        }
    }

    private void OnDestroy()
    {
        if (currentState != GrabState.None)
        {
            ForceRelease();
        }
    }
}