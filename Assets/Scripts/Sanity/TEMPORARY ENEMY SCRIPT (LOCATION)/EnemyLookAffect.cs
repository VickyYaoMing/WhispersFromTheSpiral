using UnityEngine;
using SanitySystem;
using System;

[DisallowMultipleComponent]
public class EnemyLookAffect : MonoBehaviour
{
    [Header("Affect while the Player is looking at the enemy")]
    [Tooltip("Sanity drain per second (applies only when the stress is >= drainGateHigh and until stress is <= healGateLow)")]
    [Range(0f, 1f)] public float _sanityDrainPerSecond = 0.06f;

    [Tooltip("Stress gain per second (applied whenever seen)")]
    [Range(0f, 1f)] public float _stressGainPerSecond = 0.1f;

    [Header("High stress drain gate (with hysteresis)")]
    [Tooltip("Begin draining Sanity when _stress is at or above this value")]
    [Range(0f, 1f)] public float _drainGateHigh = 0.80f;

    [Tooltip("Stop draining when _stress falls at or below this value")]
    [Range(0f, 1f)] public float _healGateLow = 0.60f;

    [Header("Optional distance falloff")] //This is a test concept, if we would want it or not.
    public bool _useDistanceFalloff = false;
    public float _maxAffectDistance = 20f; //match or exceed our vision range.
    public enum Falloff { None, Linear, Smoothstep }
    public Falloff falloff = Falloff.Smoothstep;

    bool _draining; //latched by hysteresis

    float DistanceWeight(Vector3 playerPos)
    {
        if (!_useDistanceFalloff || _maxAffectDistance <= 0f) return 1f;
        float d = Vector3.Distance(playerPos, transform.position);
        if (d >= _maxAffectDistance) return 0f;

        float t = 1f - d / _maxAffectDistance;
        return falloff switch
        {
            Falloff.Linear => t,
            Falloff.Smoothstep => t * t * (3f - 2f * t),
            _ => 1f
        };
    }

    //Call this when the player is seeing the enemy
    public void TickAffect(ISanityProvider sanity, StressController stress, Vector3 playerPos, float dt)
    {
        if (sanity == null || stress == null || dt <= 0f) return;

        float w = DistanceWeight(playerPos);
        if (w <= 0f) return;

        //Always add stress when seen (so not only when you look at the enemy)
        if (_stressGainPerSecond > 0f)
        {
            stress.ApplyImpulse(_stressGainPerSecond * w * dt); //stress goes up to 0->1 scale
        }
        float s = Mathf.Clamp01(stress.Stress);
        if (!_draining && s >= _drainGateHigh)
        {
            _draining = true;
        }
        else if (_draining && s <= _healGateLow)
        {
            _draining = false;
        }

        //Drain sanity only when in a high stress zone (above a threshold)
        if (_draining && _sanityDrainPerSecond > 0f)
        {
            sanity.ApplyImpulse(-_sanityDrainPerSecond * w * dt); //Negative lowers the sanity
        }
    }
}
