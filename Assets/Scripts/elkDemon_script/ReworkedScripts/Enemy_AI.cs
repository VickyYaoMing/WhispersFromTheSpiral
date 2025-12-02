using UnityEngine;
using UnityEngine.AI;
using static ElkDemonAI;

//[RequireComponent (typeof(Animator))]
public class Enemy_AI : MonoBehaviour
{
    //Ported from old AI system; To be used unless a better set of data can be made
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

    private NavMeshAgent _navAgent;
    private Animator _stateMachine;
    private Vector3 _playerLastKnownPosition;
    private Vector3 _playerLastKnownDirection;
    private float _playerLastSeenTime;
    private bool _hasRecentPlayerInfo;
    private int _currentObservationIndex;

    [SerializeField] Vector3 targetPosition;

    public bool HasRecentPlayerInfo { get { return _hasRecentPlayerInfo; } }
    public Vector3 PlayerLastKnownPosition { get { return _playerLastKnownPosition; } }
    public Vector3 PlayerLastKnownDirection { get { return _playerLastKnownDirection; } }
    public float MoveSpeed { get { return moveSpeed; } }
    public float HuntSpeed { get { return huntSpeed; } }
    public float AttackRange { get { return attackRange; } }
    public float AttackAngleThreshold { get { return attackAngleThreshold; } }
    public Transform Player { get { return player; } }
    public Transform[] PatrolPoints { get { return patrolPoints; } }

    void Start()
    {
        
    }

    private void Awake()
    {
        _navAgent = GetComponent<NavMeshAgent>();
        _stateMachine = GetComponent<Animator>();
        player = GameManager.Instance.Player.transform;

        _navAgent.updatePosition = true;
    }

    public void MoveTowards(Vector3 targetPosition, float speed)
    {
        //Start here for now, and hook up a new animator.
        _navAgent.speed = speed;
        _navAgent.SetDestination(targetPosition);

        float normalizedSpeed = Mathf.Clamp01(speed / maxAnimSpeed);
        _stateMachine.SetFloat("Speed", normalizedSpeed, 0.2f, Time.deltaTime);
    }

    // Update is called once per frame
    void Update()
    {
        MoveTowards(targetPosition, moveSpeed);
    }
}
