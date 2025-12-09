using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class GrabPlayerCutsceneHandler : MonoBehaviour
{
    [Header("Cutscene")]
    public Camera cutsceneCamera;            // optional, assign a dedicated cutscene cam
    public Transform elkGrabPoint;           // assign a Transform on the elk where player should be moved to
    public float pullDuration = 0.5f;        // how fast the player is pulled into grab
    public bool disableRigidbody = true;     // if player uses Rigidbody-based movement

    private Animator playerAnimator;
    private Camera mainCamera;
    private bool inCutscene = false;
    private CharacterController playerController; // your input controller interface (optional)
    private Rigidbody rb;

    void Awake()
    {
        playerAnimator = GetComponent<Animator>();
        rb = GetComponentInChildren<Rigidbody>();
        mainCamera = Camera.main;
    }

    void Start()
    {
        // subscribe to elk event (assumes single elk; adapt if multiple)
        var elk = GetComponent<ElkDemonAI>();
        if (elk != null)
        {
            elk.OnGrabPlayer += StartGrabCutscene;
        }

        // try to find a player input controller interface to disable controls cleanly
        playerController = GetComponent<CharacterController>(); // replace with your controller class if not using an interface
    }

    void OnDestroy()
    {
        var elk = GetComponent<ElkDemonAI>();
        if (elk != null)
            elk.OnGrabPlayer -= StartGrabCutscene;
    }

    // Called when elk invokes the grab
    public void StartGrabCutscene()
    {
        if (inCutscene) return;
        StartCoroutine(GrabCutsceneRoutine());
    }

    private IEnumerator GrabCutsceneRoutine()
    {
        inCutscene = true;

        // 1) lock player input
        if (playerController != null)
            playerController.enabled = false; // if your input is a MonoBehaviour
        else
        {
            // fallback: disable scripts that move the player (replace names as needed)
            var scripts = GetComponents<MonoBehaviour>();
            foreach (var s in scripts)
            {
                if (s == this || s == playerAnimator) continue;
                s.enabled = false;
            }
        }

        // 2) optionally disable physics for controlled movement
        if (disableRigidbody && rb != null)
        {
            rb.isKinematic = true;
        }

        // 3) switch camera
        if (cutsceneCamera != null)
        {
            mainCamera.enabled = false;
            cutsceneCamera.enabled = true;
        }

        // 4) play player's grabbed animation
        playerAnimator.SetTrigger("Grabbed"); // make sure this trigger exists in player animator

        // 5) pull player toward elk's grab point (if assigned)
        var elk = GetComponent<ElkDemonAI>();
        if (elkGrabPoint == null && elk != null)
        {
            // try to use a child named "GrabPoint" on elk
            var gp = elk.transform.Find("GrabPoint");
            if (gp != null) elkGrabPoint = gp;
        }

        if (elkGrabPoint != null)
        {
            Vector3 startPos = transform.position;
            Vector3 targetPos = elkGrabPoint.position;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.0001f, pullDuration);
                transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }
            transform.position = targetPos;
        }

        // 6) wait for cutscene duration (tune to your animation length)
        float cutsceneLength = 0.35f; // adjust to the grabbed animation length
        yield return new WaitForSeconds(cutsceneLength);

        // 7) apply consequences: sanity damage, death state, etc.
        var sanity = GetComponentInParent<SanitySystem.Sanity>();
        if (sanity != null)
        {
            sanity.ApplyImpulse(0.5f); // or adapt the damage you want
        }

        // 8) restore camera and controls (or you can keep player locked if you want)
        if (cutsceneCamera != null)
        {
            cutsceneCamera.enabled = false;
            mainCamera.enabled = true;
        }

        if (disableRigidbody && rb != null)
        {
            rb.isKinematic = false;
        }

        // re-enable player scripts (simple approach: reload scene-specific enabling if necessary)
        if (playerController != null)
            playerController.enabled = true;
        else
        {
            var scripts = GetComponents<MonoBehaviour>();
            foreach (var s in scripts)
            {
                if (s == this || s == playerAnimator) continue;
                s.enabled = true;
            }
        }

        inCutscene = false;
    }
}
