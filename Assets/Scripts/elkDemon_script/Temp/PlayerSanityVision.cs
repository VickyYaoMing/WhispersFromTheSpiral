using UnityEngine;
using SanitySystem;
[DisallowMultipleComponent]
public class PlayerSanityVision : MonoBehaviour
{
    [Header("Vision")]
    [SerializeField] float visionRange = 20f;
    [SerializeField, Range(1f, 180f)] float visionAngle = 30f;
    [SerializeField] Camera playerCam;
    [SerializeField] Transform enemy;

    [Header("Debug")]
    [SerializeField] bool showDebug = true;
    [SerializeField] Color coneColor = new Color(1f, 0f, 0f, 0.2f);

    ISanityProvider _sanity;
    StressController _stress;

    void Start()
    {
        if (!playerCam) playerCam = Camera.main;
        _sanity = GetComponentInParent<ISanityProvider>();
        _stress = GetComponentInParent<StressController>();
        //if (!_sanity) Debug.LogWarning("Sanity not found in parents.");
        if (!_stress) Debug.LogWarning("StressController not found in parents.");
        if (!enemy) Debug.LogWarning("Enemy reference not assigned.");
    }

    void Update()
    {
        if (!playerCam || !enemy || _sanity == null || _stress == null) return;

        Vector3 origin = playerCam.transform.position;
        Vector3 forward = playerCam.transform.forward;
        Vector3 toEnemy = enemy.position - origin;

        float dist = toEnemy.magnitude;
        if (dist > visionRange) return;

        float angle = Vector3.Angle(forward, toEnemy);
        if (angle > visionAngle * 0.5f) return;

        var affect = enemy.GetComponent<EnemyLookAffect>();
        if (affect != null)
        {
            affect.TickAffect(_sanity, _stress, origin, Time.deltaTime);
        }
        else
        {
            Debug.LogWarning("EnemyLookAffect not found on enemy.");
        }
    }

    void OnDrawGizmos()
    {
        if (!showDebug || !playerCam) return;
        Gizmos.color = coneColor;
        Vector3 origin = playerCam.transform.position;
        Quaternion left = Quaternion.AngleAxis(-visionAngle * 0.5f, Vector3.up);
        Quaternion right = Quaternion.AngleAxis(+visionAngle * 0.5f, Vector3.up);
        Gizmos.DrawRay(origin, left * playerCam.transform.forward * visionRange);
        Gizmos.DrawRay(origin, right * playerCam.transform.forward * visionRange);
        Gizmos.DrawWireSphere(origin, visionRange);
    }
}
