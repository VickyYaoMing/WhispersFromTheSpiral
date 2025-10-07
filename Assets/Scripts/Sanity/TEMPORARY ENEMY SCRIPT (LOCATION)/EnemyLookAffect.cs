using UnityEngine;
using SanitySystem;
using System;

[DisallowMultipleComponent]
public class EnemyLookAffect : MonoBehaviour
{
    [Header("While looked at")]
    [Range(0f, 2f)] public float stressGainPerSecond = 0.35f;
    [Range(0f, 2f)] public float sanityDrainPerSecond = 0.10f;

    [Header("High-stress drain gate (hysteresis)")]
    [Range(0f, 1f)] public float drainGateHigh = 0.80f;
    [Range(0f, 1f)] public float healGateLow = 0.60f;

    [Header("Distance falloff (optional)")]
    public bool useDistanceFalloff = false;
    public float maxAffectDistance = 20f;

    bool draining;
    float logTimer;

    float DistanceWeight(Vector3 playerPos)
    {
        if (!useDistanceFalloff || maxAffectDistance <= 0f) return 1f;
        float d = Vector3.Distance(playerPos, transform.position);
        if (d >= maxAffectDistance) return 0f;
        float t = 1f - d / maxAffectDistance;           // linear 0..1
        return t * t * (3f - 2f * t);                   // smoothstep
    }

    // Call once per frame while the player is seeing the enemy
    public void TickAffect(ISanityProvider sanity, StressController stress, Vector3 playerPos, float dt)
    {
        if (sanity == null || stress == null || dt <= 0f) return;

        float w = DistanceWeight(playerPos);
        if (w <= 0f) return;

        // Always add stress while seen
        if (stressGainPerSecond > 0f)
            stress.ApplyImpulse(stressGainPerSecond * w * dt);

        float s = Mathf.Clamp01(stress.Stress);

        // Latch sanity drain when high stress, release at lower stress
        if (!draining && s >= drainGateHigh) draining = true;
        else if (draining && s <= healGateLow) draining = false;

        if (draining && sanityDrainPerSecond > 0f)
            sanity.ApplyImpulse(-sanityDrainPerSecond * w * dt);

        // Throttled debug
        logTimer += dt;
        if (logTimer >= 0.25f)
        {
            logTimer = 0f;
            Debug.Log($"[EnemyLookAffect] w={w:F2} stress={s:F2} draining={draining}");
        }
    }
}
