using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class ElkDemonAI : MonoBehaviour
{
    [Header("Atack Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackAngleThreshold = 0.7f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float huntSpeed = 5f;
    [SerializeField] private float maxAnimSpeed = 6f;
    //[SerializeField] private float stalkSpeed = 1f;

    [Header("Sight")]
    [SerializeField] private float sightRange = 15f;
    [SerializeField] private float sightAngle = 45f;
    [SerializeField] private LayerMask obstructionMask;
    [SerializeField] private float eyeHeight = 1.5f;

    [Header("Grab Settings")]
    [SerializeField] private Vector3 grabOffset = new Vector3(0, 1.5f, 1f);

    [Header("References")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private Transform[] observationPoints;
    [SerializeField] private Transform player;
    [SerializeField] private BehaviorType currentBehavior;
    [SerializeField] private AudioSource elkRoar;
    [SerializeField] private Animator _animator;

    [Header("Cutscene Attach")]
    [SerializeField] private Transform _handAttach;
    [SerializeField] private Vector3 grabLocalOffset = new Vector3(0, 0, 0.5f);
    [SerializeField] private Vector3 grabRotationOffset = new Vector3(0, 0, 0);

    private NavMeshAgent _navAgent;
    private Animator _stateMachine;
    private PlayerGrabController playerGrab;

    private Vector3 _playerLastKnownPosition;
    private Vector3 _playerLastKnownDirection;

    private float _playerLastSeenTime;
    private bool _hasRecentPlayerInfo;
    private int _currentObservationIndex;

    // Track grab state
    private bool _isGrabbingPlayer = false;

    public System.Action OnGrabPlayer;

    [SerializeField] Vector3 Destination;

    public bool HasRecentPlayerInfo { get { return _hasRecentPlayerInfo; } }
    public Vector3 PlayerLastKnownPosition { get { return _playerLastKnownPosition; } }
    public Vector3 PlayerLastKnownDirection { get { return _playerLastKnownDirection; } }
    public float MoveSpeed { get { return moveSpeed; } }
    public float HuntSpeed { get { return huntSpeed; } }
    public float AttackRange { get { return attackRange; } }
    public float AttackAngleThreshold { get { return attackAngleThreshold; } }
    public Transform Player { get { return player; } }
    public Transform[] PatrolPoints { get { return patrolPoints; } }
    public bool IsGrabbingPlayer => _isGrabbingPlayer;

    private void Start()
    {
        _navAgent = GetComponent<NavMeshAgent>();
        _stateMachine = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerGrab = player.GetComponent<PlayerGrabController>();

        _navAgent.updatePosition = true;
    }

    private void Update()
    {
        // If we're grabbing player, make sure we stay stopped
        if (_isGrabbingPlayer && _navAgent != null && _navAgent.isActiveAndEnabled)
        {
            _navAgent.isStopped = true;
        }

    }

    public void MoveTowards(Vector3 targetPosition, float currentSpeed)
    {
        if (_navAgent == null || _isGrabbingPlayer) return; // Don't move while grabbing

        _navAgent.speed = currentSpeed;
        _navAgent.SetDestination(targetPosition);

        Destination = _navAgent.destination;

        if (_navAgent.velocity.sqrMagnitude > 0.01f)
        {
            Quaternion lookRot = Quaternion.LookRotation(_navAgent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 8f);
        }

        // Normalize speed and update animator
        float normalizedSpeed = Mathf.Clamp01(currentSpeed / maxAnimSpeed);
        _stateMachine.SetFloat("Speed", normalizedSpeed, 0.2f, Time.deltaTime);
    }

    public void StopMoving()
    {
        if (_navAgent == null) return;
        _navAgent.ResetPath();
        _navAgent.velocity = Vector3.zero;
        _stateMachine.SetFloat("Speed", 0f);
    }

    public bool CanSeePlayer()
    {
        if (player == null || _isGrabbingPlayer)
            return false;

        Vector3 toPlayerRaw = player.position - transform.position;
        float distanceToPlayer = toPlayerRaw.magnitude;

        if (distanceToPlayer > sightRange)
            return false;

        float angleToPlayer = Vector3.Angle(transform.forward, toPlayerRaw);
        if (angleToPlayer > sightAngle * 0.5f)
            return false;

        Vector3 rayStart = transform.position + Vector3.up * eyeHeight;
        Vector3 playerTargetPoint = player.position + Vector3.up * 1.0f;

        Vector3 toTarget = (playerTargetPoint - rayStart);
        toTarget.y = Mathf.Clamp(toTarget.y, -0.5f, 0.5f);
        Vector3 direction = toTarget.normalized;

        float sphereRadius = 0.4f;
        RaycastHit hit;

        Debug.DrawRay(rayStart, direction * sightRange, Color.red, 0.1f);

        if (Physics.SphereCast(rayStart, sphereRadius, direction, out hit, sightRange, obstructionMask))
        {
            if (hit.transform != player)
                return false;
        }

        UpdatePlayerTrackingInfo(player.position, toPlayerRaw);
        return true;
    }

    public bool CanAttackPlayer()
    {
        if (player == null || _isGrabbingPlayer) return false;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > attackRange) return false;

        Vector3 direction = (player.position - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, direction);

        return dot > attackAngleThreshold && CanSeePlayer();
    }

    public void CheckForAttack(Animator animator)
    {
        if (CanAttackPlayer() && playerGrab != null && !playerGrab.IsGrabbed)
        {
            animator.SetTrigger("Attack");

            // Use the new StartGrab signature that requires the grabber transform
            playerGrab.StartGrab(transform, transform.position);

            BeginGrabSequence();
        }
    }

    public void UpdatePlayerTrackingInfo(Vector3 playerPosition, Vector3 directionToPlayer)
    {
        _playerLastKnownPosition = playerPosition;
        _playerLastKnownDirection = directionToPlayer.normalized;
        _playerLastSeenTime = Time.time;
        _hasRecentPlayerInfo = true;
    }

    public void GetStunned()
    {
        _stateMachine.SetTrigger("Stunned");

        // If stunned while grabbing, release player
        if (_isGrabbingPlayer)
        {
            ForceReleasePlayer();
        }

        Debug.Log("Elk Demon got Stunned!");
    }

    public void BeginGrabSequence()
    {
        if (_navAgent != null)
        {
            _navAgent.isStopped = true;
        }

        _isGrabbingPlayer = true;

        // Face player
        Vector3 lookDirection = (player.position - transform.position);
        lookDirection.y = 0f;
        if (lookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDirection);
            transform.rotation = targetRot;
        }

        if (_animator != null)
            _animator.SetTrigger("Grabbed");

        // Start grab with offset position
        Vector3 grabPosition = transform.position + transform.TransformDirection(grabOffset);
        playerGrab.StartGrab(transform, grabPosition);

        OnGrabPlayer?.Invoke();
    }

    // Called when demon's hand makes contact in animation
    public void OnDemonGrabAttach()
    {
        // No parenting needed in new system
        Debug.Log("Grab attachment point reached");
    }

    // Called when throw should happen in animation
    public void OnPlayerThrow()
    {
        if (playerGrab != null && _isGrabbingPlayer)
        {
            Debug.Log("Demon executing throw!");
            playerGrab.ApplyThrow(transform.forward);
            OnPlayerReleased();
        }
    }

    public void OnPlayerReleased()
    {
        if (_navAgent != null)
            _navAgent.isStopped = false;

        if (_animator != null)
        {
            _animator.ResetTrigger("Grabbed");
            _animator.SetTrigger("Idle"); // Or whatever state you want
        }

        _isGrabbingPlayer = false;
        Debug.Log("Player released by demon");
    }

    // Force release player (for stuns, damage, etc.)
    public void ForceReleasePlayer()
    {
        if (_isGrabbingPlayer && playerGrab != null)
        {
            // Use the force release method if available
            var forceReleaseMethod = playerGrab.GetType().GetMethod("ForceRelease");
            if (forceReleaseMethod != null)
            {
                forceReleaseMethod.Invoke(playerGrab, null);
            }
            else
            {
                // Fallback to regular release
                playerGrab.Unparent();
            }

            OnPlayerReleased();
        }
    }

    public enum BehaviorType { Roar, Idle }

    public void ChangeBehavior(BehaviorType newBehavior)
    {
        // Don't change behavior while grabbing player
        if (_isGrabbingPlayer) return;

        currentBehavior = newBehavior;
        Debug.Log($"Elk Demon behavior changed to: {currentBehavior}");

        switch (newBehavior)
        {
            case BehaviorType.Roar:
                elkRoar.Play();
                break;
            case BehaviorType.Idle:
                _stateMachine.SetTrigger("Idle");
                break;
        }
    }

    // Clean up when disabled or destroyed
    private void OnDisable()
    {
        if (_isGrabbingPlayer)
        {
            ForceReleasePlayer();
        }
    }

    private void OnDestroy()
    {
        if (_isGrabbingPlayer)
        {
            ForceReleasePlayer();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * eyeHeight, sightRange);

        Vector3 leftDir = Quaternion.Euler(0, -sightAngle / 2, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0, sightAngle / 2, 0) * transform.forward;
        Gizmos.DrawRay(transform.position + Vector3.up * eyeHeight, leftDir * sightRange);
        Gizmos.DrawRay(transform.position + Vector3.up * eyeHeight, rightDir * sightRange);

        // Draw grab range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}