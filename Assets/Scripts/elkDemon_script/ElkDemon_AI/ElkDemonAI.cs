using UnityEngine;

public class ElkDemonAI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform[] patrolPoints;
    public float moveSpeed = 3f;
    public float huntSpeed = 5f;
    public float sightRange = 15f;
    public float sightAngle = 45f; 
    public LayerMask obstructionMask;
    public float eyeHeight = 1.5f;
    public Transform player;
    public float maxAnimSpeed = 6f;

    private Animator stateMachine;


    void Start()
    {
        stateMachine = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (player == null)
        {
            Debug.LogError("No object with tag 'Player' found in scene!");
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
            Debug.Log("CanSeePlayer: Failed - Player reference is null.");
            return false;
        }

        Vector3 directionToPlayer = (player.position - transform.position);
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > sightRange)
        {
            Debug.Log("CanSeePlayer: Failed - Player is too far. Distance: " + distanceToPlayer);
            return false;
        }
        else
        {
            Debug.Log("CanSeePlayer: Passed Range Check. Distance: " + distanceToPlayer);
        }

        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer > sightAngle / 2)
        {
            Debug.Log("CanSeePlayer: Failed - Player is outside FOV. Angle: " + angleToPlayer);
            return false;
        }
        else
        {
            Debug.Log("CanSeePlayer: Passed Angle Check. Angle: " + angleToPlayer);
        }

        Vector3 rayStartPoint = transform.position + (Vector3.up * eyeHeight);

        RaycastHit hit;
        // Visualize the ray in the Scene View. THIS IS CRUCIAL FOR DEBUGGING.
        Debug.DrawRay(rayStartPoint, directionToPlayer.normalized * sightRange, Color.red, 0.1f);

        if (Physics.Raycast(rayStartPoint, directionToPlayer.normalized, out hit, sightRange, obstructionMask))
        {
            Debug.Log("Vision BLOCKED by: " + hit.collider.gameObject.name + " | Layer: " + LayerMask.LayerToName(hit.collider.gameObject.layer));
            return false;
        }
        else
        {
            Debug.Log("Vision CLEAR. Can see player! Ray started from: " + rayStartPoint);
            return true;
        }
    }
}
