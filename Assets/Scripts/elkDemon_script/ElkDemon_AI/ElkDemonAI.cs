using UnityEngine;

[RequireComponent (typeof(Animator))]
public class ElkDemonAI : MonoBehaviour
{
    [Header("Atack Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackAngleThreshold = 0.7f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float huntSpeed = 5f;
    [SerializeField] private float maxAnimSpeed = 6f;
    [SerializeField] private float stalkSpeed = 1f;

    [Header("Sight")]
    [SerializeField] private float sightRange = 15f;
    [SerializeField] private float sightAngle = 45f;
    [SerializeField] private LayerMask obstructionMask;
    [SerializeField] private float eyeHeight = 1.5f;

    [Header("References")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private Transform[] observationPoints;
    [SerializeField] private Transform player;

    private Animator _stateMachine;
    private Vector3 _playerLastKnownPosition;
    private Vector3 _playerLastKnownDirection;
    private float _playerLastSeenTime;
    private bool _hasRecentPlayerInfo;
    private int _currentObservationIndex;

    public bool HasRecentPlayerInfo { get { return _hasRecentPlayerInfo; } }
    public Vector3 PlayerLastKnownPosition { get { return _playerLastKnownPosition; } }
    public Vector3 PlayerLastKnownDirection { get { return _playerLastKnownDirection; } }
    public float MoveSpeed { get { return moveSpeed; } }
    public float HuntSpeed { get { return huntSpeed; } }
    public float AttackRange { get { return attackRange; } }
    public float AttackAngleThreshold { get { return attackAngleThreshold; } }
    public Transform Player { get { return player; } }
    public Transform[] PatrolPoints { get { return patrolPoints;  } }


    void Start()
    {
        _stateMachine = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (player == null)
        {
            //Debug.LogError("No object with tag 'Player' found in scene!");
        }

    }

    public void MoveTowards(Vector3 targetPosition, float currentSpeed)
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, currentSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.LookAt(targetPosition);
        }

        // Normalize speed and update animator
        float normalizedSpeed = Mathf.Clamp01(currentSpeed / maxAnimSpeed);
        _stateMachine.SetFloat("Speed", normalizedSpeed);
    }

    public void StopMoving()
    {
        _stateMachine.SetFloat("Speed", 0f);
    }

    public bool CanSeePlayer()
    {
        if (player == null)
        {
            //Debug.Log("CanSeePlayer: Failed - Player reference is null.");
            return false;
        }

        Vector3 directionToPlayer = (player.position - transform.position);
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > sightRange)
        {
            //Debug.Log("CanSeePlayer: Failed - Player is too far. Distance: " + distanceToPlayer);
            return false;
        }
        else
        {
            //Debug.Log("CanSeePlayer: Passed Range Check. Distance: " + distanceToPlayer);
        }

        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer > sightAngle / 2)
        {
            //Debug.Log("CanSeePlayer: Failed - Player is outside FOV. Angle: " + angleToPlayer);
            return false;
        }
        else
        {
            //Debug.Log("CanSeePlayer: Passed Angle Check. Angle: " + angleToPlayer);
        }

        Vector3 rayStartPoint = transform.position + (Vector3.up * eyeHeight);
        RaycastHit hit;

        // Visualize the ray in the Scene View >:)
        Debug.DrawRay(rayStartPoint, directionToPlayer.normalized * sightRange, Color.red, 0.1f);

        if (Physics.Raycast(rayStartPoint, directionToPlayer.normalized, out hit, sightRange, obstructionMask))
        {
            //Debug.Log("Vision BLOCKED by: " + hit.collider.gameObject.name + " | Layer: " + LayerMask.LayerToName(hit.collider.gameObject.layer));
            return false;
        }
        else
        {
            UpdatePlayerTrackingInfo(player.position, directionToPlayer);
            //Debug.Log("Vision CLEAR. Can see player! Ray started from: " + rayStartPoint);
            return true;
        }
    }

    public bool CanAttackPlayer()
    {
        if (player == null) return false;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > attackRange) return false; 

        Vector3 direction = (player.position - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, direction);

        return dot > attackAngleThreshold && CanSeePlayer();
    }

    public void CheckForAttack(Animator animator)
    {
        if (CanAttackPlayer())
        {
            animator.SetTrigger("Attack");
        }
    }

    public void UpdatePlayerTrackingInfo(Vector3 playerPosition, Vector3 directionToPlayer)
    {
        _playerLastKnownPosition = playerPosition;
        _playerLastKnownDirection = directionToPlayer.normalized;
        _playerLastSeenTime = Time.time;
        _hasRecentPlayerInfo = true;
    }

    //public Transform GetObservationPoint()
    //{
    //    if (observationPoints == null || observationPoints.Length == 0)
    //        return player;

    //    // Simple round-robin selection
    //    _currentObservationIndex = (_currentObservationIndex + 1) % observationPoints.Length;
    //    return observationPoints[_currentObservationIndex];
    //}

    // Draw sight range and angle
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * eyeHeight, sightRange);

        Vector3 leftDir = Quaternion.Euler(0, -sightAngle / 2, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0, sightAngle / 2, 0) * transform.forward;
        Gizmos.DrawRay(transform.position + Vector3.up * eyeHeight, leftDir * sightRange);
        Gizmos.DrawRay(transform.position + Vector3.up * eyeHeight, rightDir * sightRange);
    }
}
