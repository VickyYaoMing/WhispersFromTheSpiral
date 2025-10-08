//using UnityEngine;
//using SanitySystem;

//public class PlayerSanityVision : MonoBehaviour
//{
//    [Header("Vision Settings")]
//    [SerializeField] private float visionRange = 20f;
//    [SerializeField] private float visionAngle = 30f;
//    [SerializeField] private Camera playerCam;

//    [Header("References")]
//    [SerializeField] private Transform enemy;

//    [Header("Debug Settings")]
//    [SerializeField] private bool showDebug = true;
//    [SerializeField] private Color coneColor = new Color(1f, 0f, 0f, 0.2f);



//    void Start()
//    {
//        if (playerCam == null)
//        {
//            playerCam = Camera.main;
//        }

//    }


//    void Update()
//    {
//        if (playerCam == null)
//        {
//            return;
//        }

//        Vector3 origin = playerCam.transform.position;
//        Vector3 dirToEnemy = enemy.position - origin;

//        float dist = dirToEnemy.magnitude;
//        if (dist > visionRange) return;

//        float angle = Vector3.Angle(playerCam.transform.forward, dirToEnemy);
//        if (angle <= visionAngle * 0.5f)
//        {
//            var drain = enemy.GetComponent<SanityDrainOnLook>();
//            if (drain != null)
//                drain.DrainTick();
//        }

//    }

//    private void OnDrawGizmos()
//    {
//        if (!showDebug || playerCam == null) return;

//        Gizmos.color = coneColor;
//        Vector3 origin = playerCam.transform.position;

//        // Draw cone edges
//        Quaternion leftRot = Quaternion.AngleAxis(-visionAngle * 0.5f, Vector3.up);
//        Quaternion rightRot = Quaternion.AngleAxis(visionAngle * 0.5f, Vector3.up);

//        Vector3 leftDir = leftRot * playerCam.transform.forward;
//        Vector3 rightDir = rightRot * playerCam.transform.forward;

//        Gizmos.DrawRay(origin, leftDir * visionRange);
//        Gizmos.DrawRay(origin, rightDir * visionRange);
//        Gizmos.DrawWireSphere(origin, visionRange);
//    }
//}
