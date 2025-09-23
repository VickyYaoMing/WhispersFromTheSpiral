using UnityEngine;
using SanitySystem;

public class PlayerSanityVision : MonoBehaviour
{
    [Header("Vision Settings")]
    [SerializeField] private float visionRange = 20f;
    [SerializeField] private float visionAngle = 30f;
    [SerializeField] private Camera playerCam;

    [Header("Debug Settings")]
    [SerializeField] private bool showDebugCone = true;
    [SerializeField] private Color coneColor = new Color(1f, 0f, 0f, 0.2f);

    private SanityDrainOnLook currentTarget;

    void Start()
    {
        if (playerCam == null)
        {
            playerCam = Camera.main;
        }
            
    }


    void Update()
    {
        if (playerCam == null)
        {
            return;
        }

        currentTarget = null;

        Collider[] hits = Physics.OverlapSphere(playerCam.transform.position, visionRange);
        foreach (var hit in hits)
        {
            var target = hit.GetComponent<SanityDrainOnLook>();
            if (target == null)
            {
                continue;
            }

            Vector3 directionToTarget = (hit.transform.position - playerCam.transform.position).normalized;
            float angleToTarget = Vector3.Angle(playerCam.transform.forward, directionToTarget);
            if (angleToTarget <= visionAngle)
            {
                if (Physics.Raycast(playerCam.transform.position,directionToTarget, out RaycastHit rayHit, visionRange))
                {
                    if (rayHit.collider.GetComponent<SanityDrainOnLook>() == target)
                    {
                        target.DrainTick();
                        currentTarget = target;
                        break;
                    }
                }
            }

        }

        
    }

    private void OnDrawGizmos()
    {
        if (!showDebugCone || playerCam == null) return;

        Gizmos.color = coneColor;
        Vector3 origin = playerCam.transform.position;
        Vector3 forward = playerCam.transform.forward;

        // Draw vision cone outline
        for (int i = 0; i < 36; i++)
        {
            float angle = (i / 36f) * 360f;
            Quaternion rotation = Quaternion.AngleAxis(angle, forward);
            Vector3 dir = rotation * (Quaternion.AngleAxis(visionAngle, playerCam.transform.right) * forward);
            Gizmos.DrawRay(origin, dir * visionRange);
        }

        // Draw range sphere for clarity
        Gizmos.DrawWireSphere(origin, visionRange);
    }
}
