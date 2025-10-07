using UnityEngine;
using SanitySystem;

public class PlayerSanityVision : MonoBehaviour
{
    [Header("Vision Settings")]
    [SerializeField] private float visionRange = 20f;
    [SerializeField, Range(1f, 180f)] private float visionAngle = 30f;
    [SerializeField] private Camera playerCam;

    [Header("Target")]
    [SerializeField] private Transform enemy;

    [Header("Line of Sight (optional)")]
    [SerializeField] private bool useLineOfSight = true;
    [SerializeField] private LayerMask losBlockers = ~0;   // which layers block sight (e.g., Default, Environment)

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;
    [SerializeField] private Color coneColor = new Color(1f, 0f, 0f, 0.2f);
    [SerializeField] private Color rayColorHit = Color.red;
    [SerializeField] private Color rayColorClear = Color.green;

    // cached refs
    Sanity _sanity;
    StressController _stress;

    void Reset()
    {
        if (!playerCam) playerCam = Camera.main;
    }

    void Start()
    {
        if (!playerCam) playerCam = Camera.main;

        // find player meters on self or parents
        _sanity = GetComponentInParent<Sanity>();
        _stress = GetComponentInParent<StressController>();

        if (!_sanity) Debug.LogWarning($"{nameof(PlayerSanityVision)}: Sanity not found in parents.");
        if (!_stress) Debug.LogWarning($"{nameof(PlayerSanityVision)}: StressController not found in parents.");
        if (!enemy) Debug.LogWarning($"{nameof(PlayerSanityVision)}: Enemy reference not assigned.");
    }

    void Update()
    {
        if (!playerCam || !enemy || _sanity == null || _stress == null) return;

        Vector3 origin = playerCam.transform.position;
        Vector3 toEnemy = enemy.position - origin;

        // range check
        float dist = toEnemy.magnitude;
        if (dist > visionRange) return;

        // angle (cone) check
        float angle = Vector3.Angle(playerCam.transform.forward, toEnemy);
        if (angle > visionAngle * 0.5f) return;

        // optional LOS check (ray from camera to enemy)
        if (useLineOfSight)
        {
            if (Physics.Raycast(origin, toEnemy.normalized, out RaycastHit hit, dist, losBlockers, QueryTriggerInteraction.Ignore))
            {
                // blocked by something that isn't the enemy or its children
                if (!IsHitEnemy(hit.transform)) return;

                if (showDebug) Debug.DrawLine(origin, hit.point, rayColorHit, 0f, false);
            }
            else
            {
                // no hit at all — consider it blocked if LOS is required
                return;
            }
        }
        else if (showDebug)
        {
            Debug.DrawLine(origin, enemy.position, rayColorClear, 0f, false);
        }

        // Apply the effect this frame (stress gain + gated sanity drain handled inside EnemyLookAffect)
        var affect = enemy.GetComponent<EnemyLookAffect>();
        if (affect != null)
        {
            affect.TickAffect(_sanity, _stress, origin, Time.deltaTime);
        }
    }

    bool IsHitEnemy(Transform hit)
    {
        // true if the hit transform is the enemy or a child of it
        if (!enemy || !hit) return false;
        return hit == enemy || hit.IsChildOf(enemy);
    }

    void OnDrawGizmos()
    {
        if (!showDebug || !playerCam) return;

        Gizmos.color = coneColor;
        Vector3 origin = playerCam.transform.position;

        // draw left/right edges of the vision cone on the horizontal plane
        Quaternion leftRot = Quaternion.AngleAxis(-visionAngle * 0.5f, Vector3.up);
        Quaternion rightRot = Quaternion.AngleAxis(+visionAngle * 0.5f, Vector3.up);

        Vector3 leftDir = leftRot * playerCam.transform.forward;
        Vector3 rightDir = rightRot * playerCam.transform.forward;

        Gizmos.DrawRay(origin, leftDir * visionRange);
        Gizmos.DrawRay(origin, rightDir * visionRange);
        Gizmos.DrawWireSphere(origin, visionRange);
    }
}
