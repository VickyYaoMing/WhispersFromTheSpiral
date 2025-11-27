using System;
using UnityEngine;

public class PlayerGrabController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovement movement;    
    [SerializeField] private Rigidbody rb;

    [Header("Grab Settings")]
    [SerializeField] private float disableControlTime = 1.5f;
    [SerializeField] private float throwForce = 12f;
    [SerializeField] private float verticalForce = 2f;

    private bool isGrabbed = false;
    private Transform originalParent;

    // Called by the enemy when the grab starts
    public void StartGrab(Vector3 grabberPosition)
    {
        if (isGrabbed) return;

        isGrabbed = true;

        if (movement != null)
            movement.enabled = false;

        if (characterController != null)
            characterController.enabled = false;
 
        Vector3 lookDir = (grabberPosition - transform.position);
        lookDir.y = 0;
        transform.rotation = Quaternion.LookRotation(lookDir);

        animator.SetTrigger("Grabbed");
    }

    public void ApplyThrow(Vector3 grabberForward)
    {
        // If use Rigidbody movement
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(grabberForward * throwForce + Vector3.up * verticalForce, ForceMode.VelocityChange);
        }
        else
        {
            StartCoroutine(ThrowByMoving(grabberForward));
        }

        Invoke(nameof(EndGrab), disableControlTime);
    }

    public void ParentTo(Transform newParent, Vector3 localPos, Vector3 localEuler)
    {
        originalParent = transform.parent;

        transform.SetParent(newParent);
        transform.localPosition = localPos;
        transform.localEulerAngles = localEuler;

        // Freeze during grab
        if (rb != null) rb.isKinematic = true;
    }

    public void Unparent()
    {
        transform.SetParent(originalParent);

        // Re-enable physics
        if (rb != null) rb.isKinematic = true;
    }

    private System.Collections.IEnumerator ThrowByMoving(Vector3 dir)
    {
        float t = 0.25f;
        while (t > 0)
        {
            t -= Time.deltaTime;
            transform.position += (dir * throwForce + Vector3.up * verticalForce) * Time.deltaTime;
            yield return null;
        }
    }

    private void EndGrab()
    {
        isGrabbed = false;

        if (movement != null)
            movement.enabled = true;

        if (characterController != null)
            characterController.enabled = true;

        if (rb != null)
            rb.isKinematic = true;
    }

}
