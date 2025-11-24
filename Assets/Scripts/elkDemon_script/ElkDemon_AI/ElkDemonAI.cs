using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

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
    //[SerializeField] private float stalkSpeed = 1f;

    [Header("Sight")]
    [SerializeField] private float sightRange = 15f;
    [SerializeField] private float sightAngle = 45f;
    [SerializeField] private LayerMask obstructionMask;
    [SerializeField] private float eyeHeight = 1.5f;

    [Header("References")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private Transform[] observationPoints;
    [SerializeField] private Transform player;
    [SerializeField] private BehaviorType currentBehavior;
    [SerializeField] private AudioSource elkRoar;
    [SerializeField] private Animator _animator;

    [Header("Cutscene Attach")]
    [SerializeField] private Transform _handAttach;

    private NavMeshAgent _navAgent;
    private Animator _stateMachine;
    private Vector3 _playerLastKnownPosition;
    private Vector3 _playerLastKnownDirection;
    private float _playerLastSeenTime;
    private bool _hasRecentPlayerInfo;
    private int _currentObservationIndex;


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
    public Transform[] PatrolPoints { get { return patrolPoints;  } }


    private void Start()
    {
        _navAgent = GetComponent<NavMeshAgent>();
        _stateMachine = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        _navAgent.updatePosition = true;
    }

    public void MoveTowards(Vector3 targetPosition, float currentSpeed)
    {
        if(_navAgent == null) return;

        _navAgent.speed = currentSpeed;
        _navAgent.SetDestination(targetPosition);

        Destination = _navAgent.destination;

        if(_navAgent.velocity.sqrMagnitude > 0.01f)
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
        if (player == null)
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

    public void GetStunned()
    {
        _stateMachine.SetTrigger("Stunned");
        Debug.Log("Elk Demon got Stunned!");
    }

    public void BeginGrabSequence(PlayerGrabController playerController)
    {
        if (_navAgent != null)
        {
            _navAgent.isStopped = true;
        }

        Vector3 lookDirection = (playerController.transform.position - transform.position);
        lookDirection.y = 0f;
        if (lookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDirection);
            transform.rotation = targetRot; 
        }

        if (_animator != null)
            _animator.SetTrigger("GrabPlayer");


    }

    public void OnDemonGrabAttach()
    {
        var playerGrab = player.GetComponentInParent<PlayerGrabController>();
        if (playerGrab != null)
        {
            if (_handAttach != null)
            {
                playerGrab.ParentTo(_handAttach, Vector3.zero, Vector3.zero);
            }
        }
    }
    public void OnPlayerReleased()
    {
        if (_navAgent != null)
        {
            _navAgent.isStopped = false;
        }

        if (_animator != null)
        {
            _animator.ResetTrigger("GrabPlayer");
        }

        var playerGrab = player.GetComponentInParent<PlayerGrabController>();
        if (playerGrab != null)
        {
            playerGrab.Unparent();
        }
    }

    public enum BehaviorType { Roar, Idle }

    public void ChangeBehavior(BehaviorType newBehavior)
    {
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
