using System;
using System.Collections.Generic;
using SanitySystem;
using UnityEngine.AI;
using Unity.VisualScripting;
using UnityEditor.EditorTools;
using UnityEngine;

public class HallucinationSpawner : MonoBehaviour
{
    [Header("Refs")]
    public Transform playerTransform;
    public Sanity sanity;
    [Header("Prefabs")]
    public List<GameObject> halluciantionPrefabs = new();
    [Min(0)] public int initialPool = 3;

    [Header("LoS & Placement")]
    [Tooltip("Layers that block the los")]
    public LayerMask obstructionMask; //Wall
    [Tooltip("Hallucination layer")]
    public int hallucinationLayer = 0;
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
            sanity.OnSanityChanged += _ => ApplyPhaseCaps();
            sanity.OnSanityStateChanged += _ => ApplyPhaseCaps();
        }
    }
    void Onable()
    {
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
        if (sanity == null || sanity.phaseProfile == null || sanity.phaseProfile.phases == null || sanity.PhaseIndex < 0)
        {
            _nextTickTime = Time.time + 9999f;
            return;
        }

        var phases = sanity.phaseProfile.phases;
        int idx = Mathf.Clamp(sanity.PhaseIndex, 0, phases.Length - 1);
        var P = phases[idx];

        if (!P.allowHallucinationSpawn)
        {
            _nextTickTime = Time.time + 9999f;
            return;
        }

        // In-phase intensity t = 1 - (Sanity/Cap)
        float t = InPhaseIntensity(sanity.Sanity01, sanity.Cap01);
        float interval = Mathf.Lerp(Mathf.Max(0.1f, P.baseInterval), Mathf.Max(0.1f, P.minInterval), Mathf.Clamp01(t));
        _nextTickTime = Time.time + interval;
    }
    private static float InPhaseIntensity(float sanity01, float cap01)
    {
        if (cap01 <= 0f) return 1f;
        float frac = Mathf.Clamp01(sanity01 / cap01);
        return 1f - frac;
    }
    private void TrySpawn()
    {
        if (halluciantionPrefabs.Count == 0 || playerTransform == null) return;

        if (sanity == null || sanity.phaseProfile == null || sanity.phaseProfile.phases == null || sanity.PhaseIndex < 0)
            return;

        int idx = Mathf.Clamp(sanity.PhaseIndex, 0, sanity.phaseProfile.phases.Length - 1);
        var P = sanity.phaseProfile.phases[idx];
        if (!P.allowHallucinationSpawn) return;

        if (_active.Count >= _maxActive) return;

        Vector3 playerPos = playerTransform.position;
        Vector3 playerEye = playerPos + Vector3.up * playerEyeHeight;

        for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
        {
            Vector3? candidate = RandomRingCandidate(playerPos, _minRadius, _maxRadius);
            if (candidate == null) continue;

            // Horizontal separation
            Vector3 flat = new Vector3(candidate.Value.x, playerPos.y, candidate.Value.z);
            if (Vector3.Distance(flat, playerPos) < minDistanceFromPlayer)
                continue;

            // LoS: spawn eye -> player eye
            Vector3 spawnEye = candidate.Value + Vector3.up * spawnHeightOffset;
            Vector3 toPlayer = playerEye - spawnEye;
            float dist = toPlayer.magnitude;
            if (dist <= 0.0001f) continue;

            bool blocked = Physics.Raycast(spawnEye, toPlayer.normalized, dist, obstructionMask, QueryTriggerInteraction.Ignore);
            if (blocked) continue;

            // Valid spot → spawn
            Quaternion rot = Quaternion.LookRotation(new Vector3(playerEye.x, candidate.Value.y, playerEye.z) - candidate.Value);
            SpawnAt(candidate.Value, rot);
            return;
        }
    }
    private Vector3? RandomRingCandidate(Vector3 center, float minR, float maxR)
    {
        float r = UnityEngine.Random.Range(minR, maxR);
        float ang = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        Vector3 flat = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * r;
        Vector3 candidate = center + flat;

        if (!requireNavMesh) return candidate;

        if (NavMesh.SamplePosition(candidate, out var hit, navMeshMaxSampleDist, NavMesh.AllAreas))
            return hit.position;

        return null;
    }
    private void SpawnAt(Vector3 pos, Quaternion rot)
    {
        var inst = GetFromPool();
        var prefab = halluciantionPrefabs[UnityEngine.Random.Range(0, halluciantionPrefabs.Count)];
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
