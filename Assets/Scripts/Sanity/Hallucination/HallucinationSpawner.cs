using System;
using System.Collections.Generic;
using SanitySystem;
using UnityEngine.AI;
using UnityEngine;

public class HallucinationSpawner : MonoBehaviour
{
    [Header("Refs")]
    public Transform playerTransform;
    public Sanity sanity;
    [Header("Prefabs")]
    public List<GameObject> hallucinationPrefabs = new();
    [Min(0)] public int initialPool = 3;

    [Header("LoS & Placement")]
    [Tooltip("Layers that block the los")]
    public LayerMask obstructionMask; //Wall
    [Tooltip("Hallucination layer")]
    public int hallucinationLayer = 0;
    [Header("NavMesh Sampling")]
    [Tooltip("Use the nearest NavMesh point to the player as the ring center when player is off-mesh.")]
    public bool centerOnNearestNavMesh = true;

    [Tooltip("How far from the player we search for a NavMesh anchor.")]
    public float centerSearchRadius = 50f;

    [Tooltip("Area mask for NavMesh sampling (NavMesh.AllAreas by default).")]
    public int navMeshAreaMask = NavMesh.AllAreas;

    [Tooltip("Max allowed snap from target ring point to the sampled NavMesh point.")]
    public float navMeshMaxSnapFromTarget = 2.0f;
    [Tooltip("Snap spawn to the NavMesh")]
    public bool requireNavMesh = true;
    [Tooltip("Max distnace")]
    public float navMeshMaxSampleDist = 1.0f;
    [Tooltip("How many random tries per spawn tick to find a valid spot")]
    [Range(1, 32)] public int maxPlacementAttempts = 12; //Can be changed depending on... well, testing
    [Tooltip("Keep distance so it doesn't spawn inside the player")]
    public float minDistanceFromPlayer = 3.0f;
    [Tooltip("Enemy eye hight")]
    public float spawnHeightOffset = 1.6f;
    [Tooltip("Player eye hight use for los")]
    public float playerEyeHeight = 1.6f;
    [Header("Lifetime")]
    [Tooltip("Random lifetime range for a hallucination instance.")]
    public Vector2 lifeSecondsRange = new Vector2(3.5f, 8.0f);
    [Header("Debug")]
    public bool drawGizmos = true;
    public KeyCode debugSpawnKey = KeyCode.H;

    //Runtime state
    private readonly List<HallucinationInstance> _pool = new();
    private struct Active
    {
        public HallucinationInstance inst;
        public float despawnAt;
    }
    private readonly List<Active> _active = new();
    private float _nextTickTime;

    //cache the per phase limits, which applies at runtime
    private float _minRadius = 6f;
    private float _maxRadius = 16f;
    private int _maxActive = 0;

    void Awake()
    {
        if (!playerTransform)
            playerTransform = Camera.main ? Camera.main.transform : transform;

        // Prewarm pool
        for (int i = 0; i < Mathf.Max(0, initialPool); i++)
            _pool.Add(CreateOne());

        // Find sanity if not set
        if (!sanity) sanity = FindFirstObjectByType<Sanity>();
        if (sanity != null)
        {
            sanity.OnSanityChanged += _ => { ApplyPhaseCaps(); ScheduleNextTick(); };
            sanity.OnSanityStateChanged += _ => { ApplyPhaseCaps(); ScheduleNextTick(); };
        }
    }
    void OnEnable()
    {
        Debug.Log("[HallucinationSpawner] Enabled");
        ApplyPhaseCaps();
        ScheduleNextTick();
        StartCoroutine(KickstartTick());
    }
    System.Collections.IEnumerator KickstartTick()
    {
        // wait 1 frame so Sanity.Start() can run
        yield return null;
        ApplyPhaseCaps();
        ScheduleNextTick();
    }
    void Update()
    {
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            if (_active[i].inst == null)
            {
                _active.RemoveAt(i);
                continue;
            }
            if (Time.time >= _active[i].despawnAt)
            {
                ReturnToPool(_active[i].inst);
                _active.RemoveAt(i);
            }
        }

        if (Time.time >= _nextTickTime)
        {
            TrySpawn();
            ScheduleNextTick();
        }
        if (Input.GetKeyDown(debugSpawnKey))
        {
            var pos = playerTransform.position + playerTransform.forward * (minDistanceFromPlayer + 1f);
            SpawnAt(pos, Quaternion.LookRotation(playerTransform.forward));
            Debug.Log("[HallucinationSpawner] Forced spawn via key.");
        }
    }
    private void ApplyPhaseCaps()
    {
        if (sanity == null || sanity.phaseProfile == null || sanity.phaseProfile.phases == null) return;

        int idx = Mathf.Clamp(sanity.PhaseIndex, 0, sanity.phaseProfile.phases.Length - 1);
        var P = sanity.phaseProfile.phases[idx];

        _minRadius = Mathf.Max(0.5f, P.minRadius);
        _maxRadius = Mathf.Max(_minRadius + 0.1f, P.maxRadius);
        _maxActive = Mathf.Max(0, P.maxActive);
    }
    private void ScheduleNextTick()
    {
        if (sanity == null) { _nextTickTime = Time.time + 9999f; Debug.LogWarning("[HallucinationSpawner] Pause: sanity == null"); return; }
        if (sanity.phaseProfile == null || sanity.phaseProfile.phases == null || sanity.phaseProfile.phases.Length == 0)
        { _nextTickTime = Time.time + 9999f; Debug.LogWarning("[HallucinationSpawner] Pause: no phase profile / empty phases"); return; }
        if (sanity.PhaseIndex < 0 || sanity.PhaseIndex >= sanity.phaseProfile.phases.Length)
        { _nextTickTime = Time.time + 9999f; Debug.LogWarning($"[HallucinationSpawner] Pause: invalid PhaseIndex={sanity.PhaseIndex}"); return; }

        var P = sanity.phaseProfile.phases[Mathf.Clamp(sanity.PhaseIndex, 0, sanity.phaseProfile.phases.Length - 1)];
        if (!P.allowHallucinationSpawn)
        { _nextTickTime = Time.time + 9999f; Debug.Log($"[HallucinationSpawner] Pause: allowHallucinationSpawn=false in phase '{P.id}'"); return; }

        float t = InPhaseIntensity(sanity.Sanity01, sanity.Cap01);
        float baseI = Mathf.Max(0.1f, P.baseInterval);
        float minI = Mathf.Max(0.1f, P.minInterval);
        float interval = Mathf.Lerp(baseI, minI, Mathf.Clamp01(t));
        _nextTickTime = Time.time + interval;

        Debug.Log($"[HallucinationSpawner] Tick in {interval:0.00}s (phase='{P.id}', t={t:0.00}, base={baseI}, min={minI})");
    }
    private static float InPhaseIntensity(float sanity01, float cap01)
    {
        if (cap01 <= 0f) return 1f;
        float frac = Mathf.Clamp01(sanity01 / cap01);
        return 1f - frac;
    }
    private void TrySpawn()
    {
        Debug.Log("[HallucinationSpawner] TrySpawn()");
        if (hallucinationPrefabs.Count == 0 || playerTransform == null)
        { Debug.LogWarning("[HallucinationSpawner] Abort: no prefabs or no playerTransform"); return; }

        if (sanity == null || sanity.phaseProfile == null || sanity.phaseProfile.phases == null || sanity.PhaseIndex < 0)
        { Debug.LogWarning("[HallucinationSpawner] Abort: sanity/phase not ready"); return; }

        int idx = Mathf.Clamp(sanity.PhaseIndex, 0, sanity.phaseProfile.phases.Length - 1);
        var P = sanity.phaseProfile.phases[idx];
        if (!P.allowHallucinationSpawn)
        { Debug.Log("[HallucinationSpawner] Abort: phase forbids spawn"); return; }

        if (_active.Count >= _maxActive)
        { Debug.Log($"[HallucinationSpawner] Abort: maxActive reached ({_active.Count}/{_maxActive})"); return; }

        Vector3 playerPos = playerTransform.position;
        Vector3 playerEye = playerPos + Vector3.up * playerEyeHeight;

        for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
        {
            Vector3? candidate = RandomRingCandidate(playerPos, _minRadius, _maxRadius);
            if (candidate == null)
            { if (attempt == maxPlacementAttempts - 1) Debug.Log("[HallucinationSpawner] No NavMesh candidate found"); continue; }

            Vector3 flat = new Vector3(candidate.Value.x, playerPos.y, candidate.Value.z);
            if (Vector3.Distance(flat, playerPos) < minDistanceFromPlayer)
            { if (attempt == maxPlacementAttempts - 1) Debug.Log("[HallucinationSpawner] Too close to player"); continue; }

            Vector3 spawnEye = candidate.Value + Vector3.up * spawnHeightOffset;
            Vector3 toPlayer = playerEye - spawnEye;
            float dist = toPlayer.magnitude;
            if (dist <= 0.0001f) { if (attempt == maxPlacementAttempts - 1) Debug.Log("[HallucinationSpawner] Zero-length ray"); continue; }

            bool blocked = Physics.Raycast(spawnEye, toPlayer.normalized, dist, obstructionMask, QueryTriggerInteraction.Ignore);
            if (blocked)
            { if (attempt == maxPlacementAttempts - 1) Debug.Log("[HallucinationSpawner] LOS blocked by obstructionMask"); continue; }

            Quaternion rot = Quaternion.LookRotation(new Vector3(playerEye.x, candidate.Value.y, playerEye.z) - candidate.Value);
            SpawnAt(candidate.Value, rot);
            Debug.Log("[HallucinationSpawner] Spawned at " + candidate.Value);
            return;
        }

        Debug.Log("[HallucinationSpawner] Gave up after attempts with no valid spot.");
    }
    private Vector3? RandomRingCandidate(Vector3 center, float minR, float maxR)
    {
        // If we don’t require NavMesh, keep the old world-space annulus
        if (!requireNavMesh)
        {
            float r = UnityEngine.Random.Range(minR, maxR);
            float ang = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            Vector3 flat = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * r;
            return center + flat;
        }

        // 1) Find a NavMesh anchor for the ring center
        Vector3 ringCenter = center;
        if (centerOnNearestNavMesh)
        {
            if (NavMesh.SamplePosition(center, out var anchor, centerSearchRadius, navMeshAreaMask))
            {
                ringCenter = anchor.position;
            }
            else
            {
                // No NavMesh near the player → skip spawning this tick
                // (Prevents “far away” spawns beyond the map)
                return null;
            }
        }

        // 2) Sample ring points ON the NavMesh
        //    Keep the snap short so we don't jump across gaps
        for (int i = 0; i < 8; i++)
        {
            float r = UnityEngine.Random.Range(minR, maxR);
            float ang = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            Vector3 flat = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * r;
            Vector3 target = ringCenter + flat;

            if (NavMesh.SamplePosition(target, out var hit, navMeshMaxSampleDist, navMeshAreaMask))
            {
                // Reject if the sample snapped too far from the intended ring
                if ((hit.position - target).sqrMagnitude <= navMeshMaxSnapFromTarget * navMeshMaxSnapFromTarget)
                    return hit.position;
            }
        }

        // Couldn’t find a good spot this tick
        return null;
    }
    private void SpawnAt(Vector3 pos, Quaternion rot)
    {
        var inst = GetFromPool();
        var prefab = hallucinationPrefabs[UnityEngine.Random.Range(0, hallucinationPrefabs.Count)];
        inst.BuildVisualIfNeeded(prefab, hallucinationLayer);

        inst.transform.SetPositionAndRotation(pos, rot);
        inst.gameObject.SetActive(true);

        float life = Mathf.Clamp(UnityEngine.Random.Range(lifeSecondsRange.x, lifeSecondsRange.y), 0.1f, 999f);
        _active.Add(new Active { inst = inst, despawnAt = Time.time + life });
    }
    // ---------- Pool ----------

    private HallucinationInstance CreateOne()
    {
        var root = new GameObject("[Hallucination]");
        root.layer = hallucinationLayer;
        root.transform.SetParent(transform);

        var inst = root.AddComponent<HallucinationInstance>();
        root.SetActive(false);
        return inst;
    }
    private HallucinationInstance GetFromPool()
    {
        if (_pool.Count > 0)
        {
            int last = _pool.Count - 1;
            var instPooled = _pool[last];
            _pool.RemoveAt(last);
            return instPooled;
        }
        return CreateOne();
    }
    private void ReturnToPool(HallucinationInstance inst)
    {
        if (!inst) return;
        inst.transform.SetParent(transform);
        inst.gameObject.SetActive(false);
        inst.ResetInstance();
        _pool.Add(inst);
    }
    // ---------- Gizmos ----------

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos || playerTransform == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(playerTransform.position, _minRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(playerTransform.position, _maxRadius);
    }

    void OnValidate()
    {
        if (navMeshMaxSampleDist < 0f) navMeshMaxSampleDist = 0f;
        if (minDistanceFromPlayer < 0f) minDistanceFromPlayer = 0f;
        if (maxPlacementAttempts < 1) maxPlacementAttempts = 1;
        if (lifeSecondsRange.x < 0.05f) lifeSecondsRange.x = 0.05f;
        if (lifeSecondsRange.y < lifeSecondsRange.x) lifeSecondsRange.y = lifeSecondsRange.x + 0.01f;
    }
}
