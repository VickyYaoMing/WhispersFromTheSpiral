using System;
using UnityEngine;
using System.Collections;

public class PlayerGrabController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator;
    [SerializeField] private Movement movement;

    [Header("Grab Settings")]
    [SerializeField] private float disableControlTime = 1.5f;
    [SerializeField] private float throwForce = 8f; // REDUCED - much lower force
    [SerializeField] private float verticalForce = 4f; // REDUCED
    [SerializeField] private float grabFollowSpeed = 8f;

    [Header("Collision Safety")]
    [SerializeField] private float maxMoveDistance = 1f; // Maximum move per frame
    [SerializeField] private LayerMask obstacleLayers = ~0; // All layers by default

    private enum GrabState { None, Grabbed, Thrown }
    private GrabState currentState = GrabState.None;

    private Transform grabber;
    private Vector3 localGrabOffset;
    private Vector3 throwDirection;
    private float throwTimer;


    public bool IsGrabbed => currentState == GrabState.Grabbed;
    public bool IsBeingThrown => currentState == GrabState.Thrown;

    public void StartGrab(Transform grabberTransform, Vector3 grabberPosition)
    {
        if (currentState != GrabState.None) return;

        currentState = GrabState.Grabbed;
        grabber = grabberTransform;

        // Calculate LOCAL offset instead of world offset
        localGrabOffset = grabber.InverseTransformPoint(transform.position);

        // Alternatively, use a fixed local offset:
        // localGrabOffset = new Vector3(0, 0.5f, 0); // Half a meter in front in local space

        if (movement != null)
            movement.enabled = false;

        // Face the grabber (look at demon)
        Vector3 lookDir = grabber.position - transform.position;
        lookDir.y = 0;
        if (lookDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(lookDir);

        animator.SetTrigger("Grabbed");
        Debug.Log("Player grabbed by demon - using LOCAL offset");
    }

    public void ApplyThrow(Vector3 grabberForward)
    {
        if (currentState != GrabState.Grabbed) return;

        currentState = GrabState.Thrown;
        throwDirection = grabberForward.normalized;
        throwTimer = disableControlTime;

        Debug.Log($"Throw direction: {throwDirection}");

        // Start the safe throw coroutine
        StartCoroutine(SafeThrowCoroutine());
    }

    private IEnumerator SafeThrowCoroutine()
    {
        Vector3 velocity = (throwDirection * throwForce) + (Vector3.up * verticalForce);
        float timer = throwTimer;

        while (timer > 0f && currentState == GrabState.Thrown)
        {
            timer -= Time.deltaTime;

            // Apply gravity
            velocity.y -= Physics.gravity.y * Time.deltaTime;

            // Calculate movement for this frame
            Vector3 frameMovement = velocity * Time.deltaTime;

            // Ensure we don't move too far in one frame
            if (frameMovement.magnitude > maxMoveDistance)
            {
                frameMovement = frameMovement.normalized * maxMoveDistance;
            }

            // Use safe movement that respects collisions
            SafeMove(frameMovement);

            yield return null;
        }

        EndGrab();
    }

    private void SafeMove(Vector3 movement)
    {
        if (characterController != null && characterController.enabled)
        {
            // CharacterController.Move already has collision detection
            characterController.Move(movement);
        }
        else
        {
            // Manual collision checking as fallback
            Vector3 newPosition = transform.position + movement;

            // Check if the new position is valid
            if (!WouldCollide(newPosition))
            {
                transform.position = newPosition;
            }
            else
            {
                // If we would collide, stop the throw
                Debug.Log("Throw stopped due to collision");
                EndGrab();
            }
        }
    }

    private bool WouldCollide(Vector3 newPosition)
    {
        // Check if moving to new position would cause collision
        float checkRadius = 0.4f;
        float checkHeight = 1.8f;
        Vector3 checkCenter = newPosition + Vector3.up * (checkHeight / 2f);

        return Physics.CheckCapsule(
            checkCenter - Vector3.up * (checkHeight / 2f - checkRadius),
            checkCenter + Vector3.up * (checkHeight / 2f - checkRadius),
            checkRadius,
            obstacleLayers
        );
    }

    private void Update()
    {
        if (currentState == GrabState.Grabbed)
        {
            UpdateGrabbed();
        }
    }

    private void UpdateGrabbed()
    {
        if (grabber == null) return;

        // Convert local offset to world position
        Vector3 targetPosition = grabber.TransformPoint(localGrabOffset);
        Vector3 moveDirection = (targetPosition - transform.position);

        // Limit movement per frame during grab too
        if (moveDirection.magnitude > maxMoveDistance)
        {
            moveDirection = moveDirection.normalized * maxMoveDistance;
        }

        SafeMove(moveDirection * grabFollowSpeed * Time.deltaTime);

        // OPTIONAL: Keep facing the demon while grabbed
        Vector3 lookDir = grabber.position - transform.position;
        lookDir.y = 0;
        if (lookDir.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(lookDir),
                Time.deltaTime * 10f);
        }
    }


    private void EndGrab()
    {
        if (currentState == GrabState.None) return;

        Debug.Log("Ending grab state");
        currentState = GrabState.None;
        grabber = null;

        if (movement != null)
        {
            movement.enabled = true;
        }

        StopAllCoroutines();
    }

    public void ForceRelease()
    {
        EndGrab();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // This gets called when CharacterController hits something
        if (currentState == GrabState.Thrown)
        {
            Debug.Log($"Player hit {hit.gameObject.name} during throw - stopping throw");
            EndGrab();
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