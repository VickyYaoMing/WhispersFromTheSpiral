using UnityEngine;
using SanitySystem;
[DisallowMultipleComponent]
public class PlayerSanityVision : MonoBehaviour
{
    [Header("Vision (world cone)")]
    [SerializeField] float visionRange = 20f;
    [SerializeField, Range(1f, 180f)] float visionAngle = 55f;

    [Header("Vision (screen/reticle assist)")]
    [SerializeField, Range(0.01f, 0.5f)] float viewportRadius = 0.18f;

    [Header("Line of Sight")]
    [SerializeField] bool useLineOfSight = true;
    [Tooltip("Environment-only layers that BLOCK sight (exclude Enemy & Player!).")]
    [SerializeField] LayerMask losBlockers;               // set in Inspector
    [SerializeField, Range(0.05f, 0.6f)] float losSphereRadius = 0.25f;

    [Header("Refs")]
    [SerializeField] Camera playerCam;
    [SerializeField] Transform enemy;

    ISanityProvider _sanity;
    StressController _stress;
    Renderer _enemyRenderer;
    Collider _enemyCollider;

    void Start()
    {
        if (!playerCam) playerCam = Camera.main;

        _sanity = GetComponentInParent<ISanityProvider>();
        _stress = GetComponentInParent<StressController>();

        if (enemy)
        {
            _enemyRenderer = enemy.GetComponentInChildren<Renderer>();
            _enemyCollider = enemy.GetComponentInChildren<Collider>();
        }
    }

    void Update()
    {
        if (!playerCam || !enemy || _sanity == null || _stress == null) return;

        Vector3 origin = playerCam.transform.position;
        Vector3 forward = playerCam.transform.forward;

        // pick a forgiving target point
        Vector3 target = enemy.position;
        if (_enemyRenderer) target = _enemyRenderer.bounds.center;
        else if (_enemyCollider) target = _enemyCollider.bounds.center;

        Vector3 toEnemy = target - origin;
        float dist = toEnemy.magnitude;
        if (dist > visionRange) return;

        // screen-space assist (near reticle)
        bool visibleByScreen = false;
        Vector3 vp = playerCam.WorldToViewportPoint(target);
        if (vp.z > 0f) // in front of camera
        {
            Vector2 d = new Vector2(vp.x - 0.5f, vp.y - 0.5f);
            visibleByScreen = (d.magnitude <= viewportRadius);
        }

        // world cone
        bool visibleByCone = Vector3.Angle(forward, toEnemy) <= visionAngle * 0.5f;

        if (!(visibleByScreen || visibleByCone)) return;

        // LOS (only return if a blocker is in the way)
        if (useLineOfSight)
        {
            Ray ray = new Ray(origin, toEnemy.normalized);
            if (Physics.SphereCast(ray, losSphereRadius, out RaycastHit hit, dist, losBlockers, QueryTriggerInteraction.Ignore))
            {
                if (!(hit.transform == enemy || hit.transform.IsChildOf(enemy)))
                    return; // blocked by environment
            }
        }

        // apply affect this frame
        var affect = enemy.GetComponent<EnemyLookAffect>();
        if (affect) affect.TickAffect(_sanity, _stress, origin, Time.deltaTime);
    }

    // void OnDrawGizmos()
    // {
    //     if (!showDebug || !playerCam) return;
    //     Gizmos.color = coneColor;
    //     Vector3 origin = playerCam.transform.position;
    //     Quaternion left = Quaternion.AngleAxis(-visionAngle * 0.5f, Vector3.up);
    //     Quaternion right = Quaternion.AngleAxis(+visionAngle * 0.5f, Vector3.up);
    //     Gizmos.DrawRay(origin, left * playerCam.transform.forward * visionRange);
    //     Gizmos.DrawRay(origin, right * playerCam.transform.forward * visionRange);
    //     Gizmos.DrawWireSphere(origin, visionRange);
    // }
}
