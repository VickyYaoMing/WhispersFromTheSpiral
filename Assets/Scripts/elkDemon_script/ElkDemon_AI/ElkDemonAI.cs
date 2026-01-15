using Assets.Scripts.AudioSystem;
using SanitySystem;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AudioSystem;

[RequireComponent(typeof(Animator))]
public class ElkDemonAI : MonoBehaviour
{
    [SerializeField] GameObject playerDeathScreen;
    [Header("Attack Settings")]
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
    [SerializeField] private Vector3 grabOffset = new Vector3(1, 1f, 1f);
    [SerializeField] private float throwAngle = 15f;

    [Header("References")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private Transform[] observationPoints;
    [SerializeField] private Transform player;
    [SerializeField] private BehaviorType currentBehavior;
    // [SerializeField] private AudioSource elkRoar;
    [SerializeField] private Animator _animator;

    [Header("Teleport Settings")]
    [SerializeField] private bool canTeleportAfterGrab = true;
    [SerializeField] private float teleportMinDistance = 10f;
    [SerializeField] private float teleportMaxDistance = 25f;
    [SerializeField] private GameObject teleportVFXPrefab;
    [SerializeField] private AudioClip teleportSound;
    [SerializeField] private float teleportCooldown = 5f;
    [SerializeField] private float minDistanceFromPlayer = 8f;

    [Header("Cutscene Attach")]
    [SerializeField] private Transform _handAttach;
    [SerializeField] private Vector3 grabLocalOffset = new Vector3(0, 0, 0.5f);
    [SerializeField] private Vector3 grabRotationOffset = new Vector3(0, 0, 0);

    [Header("Stun Settings")]
    [SerializeField] private bool canBeStunned = true;
    [SerializeField] private float stunCooldown = 5f;
    private float lastStunTime = -999f;
    [Header("Sounds")]
    [SerializeField] private SoundType screamSound;
    [SerializeField] private float screamCooldown = 10f;
    private float _lastScreamTime = -999f;
    private bool _playerVisibleLastFrame = false;
    [Header("Chase Music")]
    [SerializeField] private bool enableChaseMusic = true;
    [SerializeField] private SoundType chaseMusicType;
    [SerializeField] private float chaseFadeIn = 0.5f;         // Seconds to fade in
    [SerializeField] private float chaseFadeOut = 1.5f;        // Seconds to fade out
    [SerializeField] private string huntingBoolParam = "IsHunting"; // Animator bool
    [SerializeField] private float chaseMusicStopDelay = 3.5f; // time demon must NOT be hunting before music stops
    private bool _wasHuntingLastFrame = false;
    private bool _chaseMusicActive = false;
    private float _notHuntingSince = -1f;
    [Header("Movement Sounds")]
    [SerializeField] private SoundType footstepSound;
    [SerializeField] private float footstepIntervalWalk = 0.6f;
    [SerializeField] private float footstepIntervalRun = 0.35f;
    [SerializeField] private float movementThreshold = 0.1f;

    private float _footstepTimer = 0f;
    //

    private NavMeshAgent _navAgent;
    private Animator _stateMachine;
    private PlayerGrabController playerGrab;
    private ISanityProvider _playerSanity;

    private float lastTeleportTime;
    private bool isTeleporting = false;

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
        _playerSanity = player.GetComponent<ISanityProvider>();

        lastTeleportTime = -teleportCooldown;
        _navAgent.updatePosition = true;
        if (_stateMachine != null)
            _wasHuntingLastFrame = _stateMachine.GetBool(huntingBoolParam);
        if(playerDeathScreen != null)
        {
            playerDeathScreen.SetActive(false);
        }
    }

    private void Update()
    {
        if (_isGrabbingPlayer && _navAgent != null && _navAgent.isActiveAndEnabled)
        {
            _navAgent.isStopped = true;
        }
        UpdateFootstepSounds();
        HandleChaseMusic();
    }

    public void MoveTowards(Vector3 targetPosition, float currentSpeed)
    {
        if (_navAgent == null || _isGrabbingPlayer) return;

        _navAgent.speed = currentSpeed;
        _navAgent.SetDestination(targetPosition);

        Destination = _navAgent.destination;

        if (_navAgent.velocity.sqrMagnitude > 0.01f)
        {
            Quaternion lookRot = Quaternion.LookRotation(_navAgent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 8f);
        }

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
        if (_isGrabbingPlayer)
            return false;

        Vector3 toPlayer = player.position - transform.position;
        float distanceToPlayer = toPlayer.magnitude;

        if (distanceToPlayer > sightRange)
        {
            _playerVisibleLastFrame = false;
            return false;
        }

        float angleToPlayer = Vector3.Angle(transform.forward, toPlayer.normalized);
        if (angleToPlayer > sightAngle * 0.5f)
        {
            _playerVisibleLastFrame = false;
            return false;
        }

        Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;
        Vector3 playerCenter = player.position + Vector3.up * 1.0f;

        RaycastHit hit;
        Vector3 direction = (playerCenter - eyePosition).normalized;
        float checkDistance = Mathf.Min(distanceToPlayer, sightRange);

        if (Physics.Raycast(eyePosition, direction, out hit, checkDistance, obstructionMask))
        {
            if (hit.transform != player && !hit.transform.IsChildOf(player))
            {
                _playerVisibleLastFrame = false;
                return false;
            }
        }

        float heightDifference = Mathf.Abs(player.position.y - transform.position.y);
        if (heightDifference > 3f)
        {
            _playerVisibleLastFrame = false;
            return false;
        }

        UpdatePlayerTrackingInfo(player.position, toPlayer);

        // Scream logic 
        if (!_playerVisibleLastFrame)
        {
            TryPlaySightScream();
        }

        _playerVisibleLastFrame = true;
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
        if (_isGrabbingPlayer)
            return;

        if (CanAttackPlayer() && playerGrab != null && !playerGrab.IsGrabbed)
        {
            animator.SetTrigger("Attack");
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
        if (!canBeStunned || Time.time < lastStunTime + stunCooldown)
            return;

        // Do not stun if grabbing 
        if (_isGrabbingPlayer)
            return;

        _stateMachine.SetTrigger("Stunned");

        if (_isGrabbingPlayer)
        {
            ForceReleasePlayer();
        }

        lastStunTime = Time.time;
        Debug.Log("Elk Demon got Stunned!");
    }

    public void BeginGrabSequence()
    {
        if (_navAgent != null)
        {
            _navAgent.isStopped = true;
        }

        _isGrabbingPlayer = true;

        // Face to face
        ForceDemonToFacePlayer();

        ForcePlayerToFaceDemon();

        if (_animator != null)
            _animator.SetTrigger("Grabbed");

        playerGrab.StartGrab(transform, transform.position);

        KillPlayerSanity();

        OnGrabPlayer?.Invoke();
    }
    private void KillPlayerSanity()
    {
        if (_playerSanity == null) return;

        _playerSanity.SetSanity(0f);
        StartCoroutine(PlayerDeathSequence());
    }

    private IEnumerator PlayerDeathSequence()
    {
        var movement = player.GetComponent<Movement>();
        if (movement != null)
            movement.enabled = false;

        var cameraController = Camera.main?.GetComponent<MonoBehaviour>();
        if (cameraController != null)
            cameraController.enabled = false;

        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if(playerDeathScreen != null)
        {
            playerDeathScreen.SetActive(true);
            yield return new WaitForSeconds(2f);
            playerDeathScreen.SetActive(false);
        }
        GameManager.Instance.LoadAsync();
    }


    private void ForceDemonToFacePlayer()
    {
        if (player == null) return;

        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0;

        if (directionToPlayer.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(directionToPlayer);
        }
    }

    private void ForcePlayerToFaceDemon()
    {
        if (player == null) return;

        Vector3 playerPosition = transform.position + (transform.forward * 1.5f);
        playerPosition.y = player.position.y;

        if (!IsPositionBlocked(playerPosition))
        {
            player.position = playerPosition;
        }

        Vector3 directionToDemon = transform.position - player.position;
        directionToDemon.y = 0;

        if (directionToDemon.sqrMagnitude > 0.001f)
        {
            player.rotation = Quaternion.LookRotation(directionToDemon);
        }
    }

    private bool IsPositionBlocked(Vector3 position)
    {
        float checkRadius = 0.5f;
        float checkHeight = 2f;
        Vector3 bottom = position + Vector3.up * 0.1f;
        Vector3 top = position + Vector3.up * checkHeight;

        return Physics.CheckCapsule(bottom, top, checkRadius, obstructionMask);
    }

    // Called throw should happen in animation (Event)
    public void OnPlayerThrow()
    {
        if (playerGrab != null && _isGrabbingPlayer)
        {
            Debug.Log("Demon executing throw!");

            Vector3 throwDirection = CalculateThrowDirection();

            Debug.Log($"Throw direction calculated: {throwDirection}");
            Debug.Log($"Demon forward: {transform.forward}");
            Debug.Log($"Demon position: {transform.position}");
            Debug.Log($"Player position: {player.position}");

            playerGrab.ApplyThrow(throwDirection);
            OnPlayerReleased();
        }
        else
        {
            Debug.Log("Cannot throw - playerGrab is null or not grabbing!");
            if (playerGrab == null) Debug.Log("playerGrab is null!");
            if (!_isGrabbingPlayer) Debug.Log("Not grabbing player!");
        }
    }

    //private Vector3 CalculateThrowDirection()
    //{
    //    // Method 1: Forward with upward angle (your current approach)
    //    Vector3 baseDirection = transform.forward;
    //    Quaternion upwardRotation = Quaternion.Euler(throwAngle, 0, 0);
    //    Vector3 finalDirection = upwardRotation * baseDirection;

    //    // Method 2: Away from demon (often more reliable)
    //    if (player != null)
    //    {
    //        Vector3 awayFromDemon = (player.position - transform.position).normalized;
    //        awayFromDemon.y = 0.3f; // Keep some upward component
    //        awayFromDemon = awayFromDemon.normalized;

    //        Debug.Log($"Away direction: {awayFromDemon}");
    //        return awayFromDemon;
    //    }

    //    return finalDirection.normalized;
    //}

    private Vector3 CalculateThrowDirection()
    {
        if (player != null)
        {
            Vector3 toPlayer = (player.position - transform.position);

            Vector3 horizontalDirection = new Vector3(toPlayer.x, 0, toPlayer.z).normalized;

            if (horizontalDirection == Vector3.zero)
            {
                horizontalDirection = transform.forward;
            }

            float angleInRadians = Mathf.Deg2Rad * throwAngle;
            float horizontalMagnitude = Mathf.Cos(angleInRadians);
            float verticalMagnitude = Mathf.Sin(angleInRadians);

            Vector3 finalDirection = (horizontalDirection * horizontalMagnitude) + (Vector3.up * verticalMagnitude);
            finalDirection = finalDirection.normalized;

            Debug.Log($"Throw direction: {finalDirection}");
            return finalDirection;
        }

        Vector3 baseDirection = transform.forward;
        Quaternion upwardRotation = Quaternion.Euler(throwAngle, 0, 0);
        return (upwardRotation * baseDirection).normalized;
    }

    //private Vector3 CalculateThrowDirectionAway()
    //{
    //    if (player == null) return transform.forward;

    //    Vector3 directionFromDemon = (player.position - transform.position).normalized;
    //    directionFromDemon.y = 0.3f;
    //    return directionFromDemon.normalized;
    //}

    public void OnPlayerReleased()
    {
        _isGrabbingPlayer = false;

        if (_navAgent != null)
            _navAgent.isStopped = false;

        if (_animator != null)
            _animator.ResetTrigger("Grabbed");

        if (canTeleportAfterGrab && CanTeleport())
        {
            StartCoroutine(TeleportSequence());
        }

        Debug.Log("Player released by demon");
    }

    private bool CanTeleport()
    {
        if (Time.time < lastTeleportTime + teleportCooldown)
            return false;

        if (isTeleporting)
            return false;

        return true;
    }

    private IEnumerator TeleportSequence()
    {
        isTeleporting = true;

        // Step 1: Play teleport start effects
        PlayTeleportEffects(false); // Teleport out effects

        // Step 2: Brief delay before disappearing
        yield return new WaitForSeconds(0.2f);

        // Step 3: Disable renderer and collider temporarily
        SetVisibility(false);

        // Step 4: Find a teleport location
        Vector3 teleportPosition = FindTeleportPosition();

        // Step 5: Move to new position
        transform.position = teleportPosition;

        // Step 6: Brief delay before reappearing
        yield return new WaitForSeconds(0.3f);

        // Step 7: Enable renderer and collider
        SetVisibility(true);

        // Step 8: Play teleport arrival effects
        PlayTeleportEffects(true); // Teleport in effects

        // Step 9: Update teleport cooldown
        lastTeleportTime = Time.time;
        isTeleporting = false;

        Debug.Log($"Demon teleported to: {teleportPosition}");
    }

    private Vector3 FindTeleportPosition()
    {
        int maxAttempts = 30;

        for (int i = 0; i < maxAttempts; i++)
        {
            // Method 1: Random point around player within range
            Vector3 randomDirection = Random.insideUnitSphere.normalized;
            float randomDistance = Random.Range(teleportMinDistance, teleportMaxDistance);
            Vector3 candidatePosition = player.position + (randomDirection * randomDistance);
            candidatePosition.y = transform.position.y;

            // Method 2: Try to find a point behind the player
            if (i % 3 == 0)
            {
                Vector3 behindPlayer = player.position - (player.forward * Random.Range(teleportMinDistance, teleportMaxDistance * 0.7f));
                candidatePosition = behindPlayer;
            }

            if (IsValidTeleportPosition(candidatePosition))
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(candidatePosition, out hit, 5f, NavMesh.AllAreas))
                {
                    if (Vector3.Distance(hit.position, player.position) >= minDistanceFromPlayer)
                    {
                        return hit.position;
                    }
                }
            }
        }

        Debug.LogWarning("Could not find valid teleport position, using fallback");
        return GetFallbackTeleportPosition();
    }

    private bool IsValidTeleportPosition(Vector3 position)
    {
        if (Vector3.Distance(position, player.position) < minDistanceFromPlayer)
            return false;

        NavMeshHit hit;
        if (!NavMesh.SamplePosition(position, out hit, 1f, NavMesh.AllAreas))
            return false;

        RaycastHit sightHit;
        Vector3 eyePosition = position + Vector3.up * eyeHeight;
        Vector3 playerEyePosition = player.position + Vector3.up * 1.5f;
        Vector3 directionToPlayer = (playerEyePosition - eyePosition).normalized;

        if (Physics.Raycast(eyePosition, directionToPlayer, out sightHit, sightRange))
        {
            if (sightHit.transform == player)
            {

                return Random.value > 0.5f;
            }
        }

        return true;
    }

    private Vector3 GetFallbackTeleportPosition()
    {
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Transform farthestPoint = patrolPoints[0];
            float maxDistance = 0;

            foreach (Transform point in patrolPoints)
            {
                float distance = Vector3.Distance(point.position, player.position);
                if (distance > maxDistance && distance >= minDistanceFromPlayer)
                {
                    maxDistance = distance;
                    farthestPoint = point;
                }
            }

            NavMeshHit hit;
            if (NavMesh.SamplePosition(farthestPoint.position, out hit, 5f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        Vector3 randomPoint = player.position + Random.insideUnitSphere * teleportMaxDistance;
        randomPoint.y = transform.position.y;

        NavMeshHit lastResortHit;
        if (NavMesh.SamplePosition(randomPoint, out lastResortHit, 10f, NavMesh.AllAreas))
        {
            return lastResortHit.position;
        }

        Debug.LogError("Could not find any valid teleport position!");
        return transform.position;
    }

    private void PlayTeleportEffects(bool isArrival)
    {
        if (teleportSound != null)
        {
            AudioSource.PlayClipAtPoint(teleportSound, transform.position);
        }

        if (teleportVFXPrefab != null)
        {
            GameObject vfx = Instantiate(teleportVFXPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 2f);
        }


    }

    private void SetVisibility(bool isVisible)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = isVisible;
        }

        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = isVisible;
        }

        if (_navAgent != null)
        {
            _navAgent.isStopped = !isVisible;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(player.position, teleportMinDistance);
            Gizmos.DrawWireSphere(player.position, teleportMaxDistance);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position, minDistanceFromPlayer);
        }
    }

    public void ForceReleasePlayer()
    {
        if (_isGrabbingPlayer && playerGrab != null)
        {
            var forceReleaseMethod = playerGrab.GetType().GetMethod("ForceRelease");
            if (forceReleaseMethod != null)
            {
                forceReleaseMethod.Invoke(playerGrab, null);
            }

            OnPlayerReleased();
        }
    }

    public enum BehaviorType { Roar, Idle }

    public void ChangeBehavior(BehaviorType newBehavior)
    {
        if (_isGrabbingPlayer) return;

        currentBehavior = newBehavior;
        Debug.Log($"Elk Demon behavior changed to: {currentBehavior}");

        switch (newBehavior)
        {
            case BehaviorType.Roar:
                SoundManager.PlayAt(screamSound, transform.position, 1f);
                break;
            case BehaviorType.Idle:
                _stateMachine.SetTrigger("Idle");
                break;
        }
    }

    private void OnDisable()
    {
        if (_isGrabbingPlayer)
        {
            ForceReleasePlayer();
        }
        if (enableChaseMusic && _chaseMusicActive)
        {
            SoundManager.StopMusic(chaseFadeOut);
            _chaseMusicActive = false;
        }
    }

    private void OnDestroy()
    {
        if (_isGrabbingPlayer)
        {
            ForceReleasePlayer();
        }
        if (enableChaseMusic && _chaseMusicActive)
        {
            SoundManager.StopMusic(chaseFadeOut);
            _chaseMusicActive = false;
        }
    }
    private void TryPlaySightScream()
    {
        if (Time.time < _lastScreamTime + screamCooldown)
        {
            return;
        }
        _lastScreamTime = Time.time;
        SoundManager.PlayAt(screamSound, transform.position, 1f);
        Debug.Log("Elk is screaming RAHHHH");
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.yellow;
        Vector3 eyePos = transform.position + Vector3.up * eyeHeight;
        Gizmos.DrawWireSphere(eyePos, sightRange);

        Quaternion leftRot = Quaternion.Euler(0, -sightAngle / 2, 0);
        Quaternion rightRot = Quaternion.Euler(0, sightAngle / 2, 0);

        Vector3 leftDir = leftRot * transform.forward;
        Vector3 rightDir = rightRot * transform.forward;

        Gizmos.DrawRay(eyePos, leftDir * sightRange);
        Gizmos.DrawRay(eyePos, rightDir * sightRange);

        if (CanSeePlayer())
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(eyePos, player.position + Vector3.up * 1.0f);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    //New method for footstep sounds
    private void UpdateFootstepSounds()
    {
        // No agent, no footsteps
        if (_navAgent == null) return;

        // Don't play steps while grabbing / invisible / stunned etc.
        if (_isGrabbingPlayer) return;

        // If you haven't assigned a sound, do nothing
        if (footstepSound == SoundType.None) return;

        // If agent is stopped or not really moving, reset timer and bail
        if (_navAgent.isStopped)
        {
            _footstepTimer = 0f;
            return;
        }

        // Only care about horizontal speed
        Vector3 vel = _navAgent.velocity;
        vel.y = 0f;
        float speed = vel.magnitude;

        if (speed < movementThreshold)
        {
            _footstepTimer = 0f;
            return;
        }

        // Blend interval between walk & run, based on current speed
        // (0 = walkSpeed, 1 = huntSpeed)
        float t = 0f;
        if (huntSpeed > moveSpeed)
        {
            t = Mathf.InverseLerp(moveSpeed, huntSpeed, speed);
        }
        float interval = Mathf.Lerp(footstepIntervalWalk, footstepIntervalRun, t);

        _footstepTimer -= Time.deltaTime;
        if (_footstepTimer <= 0f)
        {
            SoundManager.PlayAt(footstepSound, transform.position, 1f);
            _footstepTimer = interval;
        }
    }
    private void HandleChaseMusic()
    {
        if (!enableChaseMusic || _stateMachine == null)
            return;

        bool isHuntingNow = _stateMachine.GetBool(huntingBoolParam);

        // Transition: NOT hunting -> HUNTING  → start music
        if (isHuntingNow && !_wasHuntingLastFrame)
        {
            _notHuntingSince = -1f; // clear timer

            if (!_chaseMusicActive && chaseMusicType != SoundType.None)
            {
                SoundManager.PlayMusic(chaseMusicType, chaseFadeIn);
                _chaseMusicActive = true;
                Debug.Log("Chase music started.");
            }
        }

        // Transition: HUNTING -> NOT hunting  → start countdown to stop music
        if (!isHuntingNow && _wasHuntingLastFrame)
        {
            _notHuntingSince = Time.time;
            Debug.Log("Chase music: left hunting, starting stop-delay timer.");
        }

        // If we are NOT hunting and music is active, only stop after delay
        if (!isHuntingNow && _chaseMusicActive && _notHuntingSince > 0f)
        {
            if (Time.time - _notHuntingSince >= chaseMusicStopDelay)
            {
                SoundManager.StopMusic(chaseFadeOut);
                _chaseMusicActive = false;
                Debug.Log("Chase music stopped after delay.");
            }
        }

        // If we are hunting again before delay is over, timer gets reset next frame
        if (isHuntingNow)
        {
            _notHuntingSince = -1f;
        }

        _wasHuntingLastFrame = isHuntingNow;
    }
}