using UnityEngine;
using SanitySystem;

public class SanityEffectOnPlayer : MonoBehaviour
{
    [Header("Drain Vision Settings")]
    [SerializeField] private float visionRange = 20f;
    [SerializeField] private float visionAngle = 30f;
    [SerializeField] private Camera playerCam;

    [Header("References")]
    [SerializeField] private Transform enemy;
    [SerializeField] private MonoBehaviour playerMovementScript;

    [Header("Debug Settings")]
    [SerializeField] private bool showDebug = true;
    [SerializeField] private Color coneColor = new Color(1f, 0f, 0f, 0.2f);

    private float _sanityDmg;
    private bool _isDead = false;

    public float SanityDmg { get { return _sanityDmg; } }
    public bool IsDead { get { return _isDead; } }


    private void Start()
    {
        if (playerCam == null) 
            playerCam = Camera.main;
        
    }

    private void Update()
    {
        if (playerCam == null)
            return;

        SeeEnemy();
    }

    public void SeeEnemy()
    {
        Vector3 origin = playerCam.transform.position;
        Vector3 dirToEnemy = enemy.position - origin;

        float dist = dirToEnemy.magnitude;
        if (dist > visionRange) return;

        float angle = Vector3.Angle(playerCam.transform.forward, dirToEnemy);
        if (angle <= visionAngle * 0.5f)
        {
            var drain = enemy.GetComponent<SanityDrainOnLook>();
            if (drain != null)
                drain.DrainTick();
        }
    }
    public void HandleFatalHit()
    {
        Debug.Log(" Player received a fatal hit!");
        ZeroSanityDeath();
    }

    public void ZeroSanityDeath()
    {
        Debug.Log("\" Maaaaan I'm deaaaaaaaaaaad!");
        _isDead = true;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

    private void OnDrawGizmos()
    {
        if (!showDebug || playerCam == null) return;

        Gizmos.color = coneColor;
        Vector3 origin = playerCam.transform.position;

        // Draw cone edges
        Quaternion leftRot = Quaternion.AngleAxis(-visionAngle * 0.5f, Vector3.up);
        Quaternion rightRot = Quaternion.AngleAxis(visionAngle * 0.5f, Vector3.up);

        Vector3 leftDir = leftRot * playerCam.transform.forward;
        Vector3 rightDir = rightRot * playerCam.transform.forward;

        Gizmos.DrawRay(origin, leftDir * visionRange);
        Gizmos.DrawRay(origin, rightDir * visionRange);
        Gizmos.DrawWireSphere(origin, visionRange);
    }
}
