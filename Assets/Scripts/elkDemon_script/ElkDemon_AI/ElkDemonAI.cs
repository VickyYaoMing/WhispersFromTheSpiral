using System.Collections;
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
    [SerializeField] private Vector3 grabOffset = new Vector3(1, 1f, 1f);
    [SerializeField] private float throwAngle = 15f;

    [Header("References")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private Transform[] observationPoints;
    [SerializeField] private Transform player;
    [SerializeField] private BehaviorType currentBehavior;
    [SerializeField] private AudioSource elkRoar;
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

    private NavMeshAgent _navAgent;
    private Animator _stateMachine;
    private PlayerGrabController playerGrab;

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

        lastTeleportTime = -teleportCooldown;
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
        // Check cooldown and conditions
        if (!canBeStunned || Time.time < lastStunTime + stunCooldown)
            return;

        // Don't stun if grabbing player
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

        // Force demon to face player
        ForceDemonToFacePlayer();

        // Force player to face demon
        ForcePlayerToFaceDemon();

        if (_animator != null)
            _animator.SetTrigger("Grabbed");

        // Just pass demon's transform and position
        // The PlayerGrabController will handle the positioning
        playerGrab.StartGrab(transform, transform.position);

        OnGrabPlayer?.Invoke();
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

        // Position player directly in front of demon
        Vector3 playerPosition = transform.position + (transform.forward * 1.5f);
        playerPosition.y = player.position.y; // Keep player's current height

        // Check if position is clear
        if (!IsPositionBlocked(playerPosition))
        {
            player.position = playerPosition;
        }

        // Make player look at demon
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

    // Called when throw should happen in animation
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

    private Vector3 CalculateThrowDirection()
    {
        // Method 1: Forward with upward angle (your current approach)
        Vector3 baseDirection = transform.forward;
        Quaternion upwardRotation = Quaternion.Euler(throwAngle, 0, 0);
        Vector3 finalDirection = upwardRotation * baseDirection;

        // Method 2: Away from demon (often more reliable)
        if (player != null)
        {
            Vector3 awayFromDemon = (player.position - transform.position).normalized;
            awayFromDemon.y = 0.3f; // Keep some upward component
            awayFromDemon = awayFromDemon.normalized;

            Debug.Log($"Away direction: {awayFromDemon}");
            return awayFromDemon;
        }

        return finalDirection.normalized;
    }

    private Vector3 CalculateThrowDirectionAway()
    {
        if (player == null) return transform.forward;

        Vector3 directionFromDemon = (player.position - transform.position).normalized;
        directionFromDemon.y = 0.3f;
        return directionFromDemon.normalized;
    }

    public void OnPlayerReleased()
    {
        if (_navAgent != null)
            _navAgent.isStopped = false;

        if (_animator != null)
        {
            _animator.ResetTrigger("Grabbed");
        }

        if (canTeleportAfterGrab && CanTeleport())
        {
            StartCoroutine(TeleportSequence());
        }

        _isGrabbingPlayer = false;
        Debug.Log("Player released by demon");
    }

    private bool CanTeleport()
    {
        // Check cooldown
        if (Time.time < lastTeleportTime + teleportCooldown)
            return false;

        // Don't teleport if already teleporting
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
        yield return new WaitForSeconds(0.3f);

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
            candidatePosition.y = transform.position.y; // Keep same height initially

            // Method 2: Try to find a point behind the player
            if (i % 3 == 0) // Every 3rd attempt, try behind player
            {
                Vector3 behindPlayer = player.position - (player.forward * Random.Range(teleportMinDistance, teleportMaxDistance * 0.7f));
                candidatePosition = behindPlayer;
            }

            // Check if position is valid
            if (IsValidTeleportPosition(candidatePosition))
            {
                // Get exact NavMesh position
                NavMeshHit hit;
                if (NavMesh.SamplePosition(candidatePosition, out hit, 5f, NavMesh.AllAreas))
                {
                    // Ensure minimum distance from player
                    if (Vector3.Distance(hit.position, player.position) >= minDistanceFromPlayer)
                    {
                        return hit.position;
                    }
                }
            }
        }

        // Fallback: Use patrol points or observation points
        Debug.LogWarning("Could not find valid teleport position, using fallback");
        return GetFallbackTeleportPosition();
    }

    private bool IsValidTeleportPosition(Vector3 position)
    {
        // Check if too close to player
        if (Vector3.Distance(position, player.position) < minDistanceFromPlayer)
            return false;

        // Check if position is on NavMesh
        NavMeshHit hit;
        if (!NavMesh.SamplePosition(position, out hit, 1f, NavMesh.AllAreas))
            return false;

        // Check line of sight (optional - for surprise attacks)
        // You might want the demon to teleport out of sight
        RaycastHit sightHit;
        Vector3 eyePosition = position + Vector3.up * eyeHeight;
        Vector3 playerEyePosition = player.position + Vector3.up * 1.5f;
        Vector3 directionToPlayer = (playerEyePosition - eyePosition).normalized;

        // Don't teleport right in front of player
        if (Physics.Raycast(eyePosition, directionToPlayer, out sightHit, sightRange))
        {
            if (sightHit.transform == player)
            {
                // Player can see this spot - maybe avoid it for surprise
                return Random.value > 0.5f; // 50% chance to allow visible spots
            }
        }

        return true;
    }

    private Vector3 GetFallbackTeleportPosition()
    {
        // Try patrol points first
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

        // Last resort: random point on NavMesh
        Vector3 randomPoint = player.position + Random.insideUnitSphere * teleportMaxDistance;
        randomPoint.y = transform.position.y;

        NavMeshHit lastResortHit;
        if (NavMesh.SamplePosition(randomPoint, out lastResortHit, 10f, NavMesh.AllAreas))
        {
            return lastResortHit.position;
        }

        // Ultimate fallback: don't move
        Debug.LogError("Could not find any valid teleport position!");
        return transform.position;
    }

    private void PlayTeleportEffects(bool isArrival)
    {
        // Play sound
        if (teleportSound != null)
        {
            AudioSource.PlayClipAtPoint(teleportSound, transform.position);
        }

        // Spawn VFX
        if (teleportVFXPrefab != null)
        {
            GameObject vfx = Instantiate(teleportVFXPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 2f); // Clean up after 2 seconds
        }

        // You could also:
        // - Play particle effects
        // - Screen shake
        // - Flash effect
        // - Distortion shader
    }

    private void SetVisibility(bool isVisible)
    {
        // Disable/enable renderers
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = isVisible;
        }

        // Disable/enable collider during teleport (optional)
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = isVisible;
        }

        // Stop/start NavAgent during teleport
        if (_navAgent != null)
        {
            _navAgent.isStopped = !isVisible;
        }
    }

    // Debug visualization for teleport range
    private void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            // Draw teleport range
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(player.position, teleportMinDistance);
            Gizmos.DrawWireSphere(player.position, teleportMaxDistance);

            // Draw minimum distance from player
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position, minDistanceFromPlayer);
        }
    }

    // Force release player (for stuns, damage, etc.)
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
                elkRoar.Play();
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