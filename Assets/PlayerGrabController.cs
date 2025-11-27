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
    [SerializeField] private float throwForce = 12f;
    [SerializeField] private float verticalForce = 2f;
    [SerializeField] private float grabFollowSpeed = 5f;

    private enum GrabState { None, Grabbed, Thrown }
    private GrabState currentState = GrabState.None;

    private Transform grabber;
    private Vector3 grabOffset;
    private Vector3 throwVelocity;
    private float throwTimer;

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

        // Setup physics
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
        }

        animator.SetTrigger("Grabbed");
        Debug.Log("Player grabbed by demon");
    }

    public void ApplyThrow(Vector3 grabberForward)
    {
        if (currentState != GrabState.Grabbed) return;

        currentState = GrabState.Thrown;

        // Calculate throw velocity
        throwVelocity = grabberForward * throwForce + Vector3.up * verticalForce;
        throwTimer = disableControlTime;

        // Enable physics for throw
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(throwVelocity, ForceMode.VelocityChange);
        }

        Debug.Log($"Player thrown with velocity: {throwVelocity}");

        // Start coroutine to handle throw duration
        StartCoroutine(ThrowCountdown());
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

        // Match grabber's rotation (optional - for more dynamic movement)
        // transform.rotation = Quaternion.Slerp(transform.rotation, grabber.rotation, grabFollowSpeed * Time.deltaTime);
    }

    private void UpdateThrown()
    {
        if (rb == null)
        {
            // Manual throw movement if no rigidbody
            transform.position += throwVelocity * Time.deltaTime;
            throwVelocity.y -= Physics.gravity.y * Time.deltaTime * 0.5f;
        }

        throwTimer -= Time.deltaTime;
        if (throwTimer <= 0)
        {
            EndGrab();
        }
    }

    // This is now handled by the follow system, no parenting needed
    public void ParentTo(Transform newParent, Vector3 localPos, Vector3 localEuler)
    {
        // Not used in this approach - we use position following instead
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