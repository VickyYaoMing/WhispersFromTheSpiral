using UnityEngine;

public class PlayerGrabController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private CharacterController characterController; // or your player movement script type
    [SerializeField] private Transform grabAttachPoint; // optional: demon hand attach target on player (local)
    [SerializeField] private MonoBehaviour[] scriptsToDisableDuringCutscene;

    private Transform originalParent;
    private bool inCutscene = false;
    private ElkDemonAI currentDemon;

    // Called by the demon to start the cutscene
    public void StartGrabCutscene(ElkDemonAI demon)
    {
        if (inCutscene) return;
        inCutscene = true;
        currentDemon = demon;

        // Disable player movement / input
        if (characterController != null)
            characterController.enabled = false;

        foreach (var s in scriptsToDisableDuringCutscene)
            if (s != null) s.enabled = false;

        // Trigger player animator "Grabbed" state
        if (playerAnimator != null)
            playerAnimator.SetTrigger("Grabbed");

        // Call demon hook so it can play its grab animation and align
        demon?.BeginGrabSequence(this);
    }

    // Called by an animation event in the player's "grabbed" animation once cutscene ends
    // Name must match the event string in the animation timeline
    public void OnPlayerGrabAnimationComplete()
    {
        EndGrabCutscene();
    }

    // Undo the cutscene lock and notify demon
    public void EndGrabCutscene()
    {
        if (!inCutscene) return;
        inCutscene = false;

        // Re-enable player movement / input
        if (characterController != null)
            characterController.enabled = true;

        foreach (var s in scriptsToDisableDuringCutscene)
            if (s != null) s.enabled = true;

        // Reset animator trigger if needed
        if (playerAnimator != null)
            playerAnimator.ResetTrigger("Grabbed");

        // notify demon if it needs to do cleanup
        if (currentDemon != null)
        {
            currentDemon.OnPlayerReleased();
            currentDemon = null;
        }
    }

    // Utility if you need the demon to parent the player to a bone:
    public void ParentTo(Transform parent, Vector3 localPosition, Vector3 localRotation)
    {
        originalParent = transform.parent;
        transform.SetParent(parent, true);
        transform.localPosition = localPosition;
        transform.localEulerAngles = localRotation;
    }

    public void Unparent()
    {
        transform.SetParent(originalParent, true);
    }
}
