using UnityEngine;

public class ElkDemonAI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform[] patrolPoints;
    public Transform[] observationPoints;
    public float moveSpeed = 3f;
    public float huntSpeed = 5f;
    public float sightRange = 15f;
    public float sightAngle = 45f; 
    public LayerMask obstructionMask;
    public float eyeHeight = 1.5f;
    public Transform player;
    public float maxAnimSpeed = 6f;
    public float stalkSpeed = 1f;

    private Animator stateMachine;
    private Vector3 playerLastKnownPosition;
    private Vector3 playerLastKnownDirection;
    private float playerLastSeenTime;
    private bool hasRecentPlayerInfo = false;
    private int currentObservationIndex = 0;

    public bool HasRecentPlayerInfo { get { return hasRecentPlayerInfo; } }
    public Vector3 PlayerLastKnownPosition { get { return playerLastKnownPosition; } }
    public Vector3 PlayerLastKnownDirection { get { return playerLastKnownDirection; } }

    void Start()
    {
        stateMachine = GetComponent<Animator>();
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
        stateMachine.SetFloat("Speed", normalizedSpeed);
    }

    public void StopMoving()
    {
        stateMachine.SetFloat("Speed", 0f);
    }

    public bool canSeePlayer()
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
        if (distance > 2f) return false; 

        Vector3 direction = (player.position - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, direction);

        return dot > 0.7f && canSeePlayer();
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
        playerLastKnownPosition = playerPosition;
        playerLastKnownDirection = directionToPlayer.normalized;

        playerLastSeenTime = Time.time;
        hasRecentPlayerInfo = true;
    }

    public Transform GetObservationPoint()
    {
        if (observationPoints == null || observationPoints.Length == 0)
            return player;

        // Simple round-robin selection
        currentObservationIndex = (currentObservationIndex + 1) % observationPoints.Length;
        return observationPoints[currentObservationIndex];
    }

    private void OnDrawGizmos()
    {
        // Draw sight range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * eyeHeight, sightRange);

        // Draw sight angle
        Vector3 leftDir = Quaternion.Euler(0, -sightAngle / 2, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0, sightAngle / 2, 0) * transform.forward;
        Gizmos.DrawRay(transform.position + Vector3.up * eyeHeight, leftDir * sightRange);
        Gizmos.DrawRay(transform.position + Vector3.up * eyeHeight, rightDir * sightRange);
    }
}
