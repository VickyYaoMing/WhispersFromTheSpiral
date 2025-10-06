using UnityEngine;
using SanitySystem;
using System.Collections.Generic;

public class SanityAudioRouter2D : MonoBehaviour
{
    [Header("Refs")]
    public Sanity _sanity;
    public StressController _stress;
    public AudioSource _heartbeat2D, _breathing2D, _voices2D;
    [Header("Smoothing")]
    [Tooltip("Seconds to reach new target volume/pitch (~0.1–0.3 feels good). Set 0 to disable.")]
    [Range(0f, 1f)] public float fadeTime = 0.15f;
    [Header("Heartbeat (driven by Stress 0→1)")]
    [Range(0f, 1f)] public float _heartbeatVolAtCalm = 0.10f;
    [Range(0f, 1f)] public float _heartbeatVolAtMax = 1.00f;
    [Range(0.1f, 3f)] public float _heartbeatPitchCalm = 1.00f;
    [Range(0.1f, 3f)] public float _heartbeatPitchMax = 1.20f;

    [Header("Breathing (driven by Stress 0→1)")]
    [Range(0f, 1f)] public float _breathingVolAtCalm = 0.15f;
    [Range(0f, 1f)] public float _breathingVolAtMax = 0.80f;
    [Range(0.1f, 3f)] public float _breathingPitchCalm = 1.00f;
    [Range(0.1f, 3f)] public float _breathingPitchMax = 1.12f;

    [Header("Debug")]
    public bool logDebug = false;

    // internal smoothing caches
    float _heartbeatVolVel, _heartbeatPitchVel, _breathingVolVel, _breathingPitchVel, _voicesVolVel;

    void Reset()
    {
        if (!_sanity) _sanity = GetComponentInParent<Sanity>();
        if (!_stress) _stress = GetComponentInParent<StressController>();
    }

    void OnEnable()
    {
        // Ensure 2D loops are actually 2D and playing
        Setup2D(_heartbeat2D);
        Setup2D(_breathing2D);
        Setup2D(_voices2D);
    }

    void Update()
    {
        if (!_sanity) return;

        // --- 1) VOICES (phase floors + cap-relative within-phase drop) ---
        _sanity.GetVoices(out float bedVol, out float _ /*densityPerMin ignored */);
        float targetVo = Mathf.Clamp01(bedVol);
        if (_voices2D)
        {
            _voices2D.volume = Smooth(_voices2D.volume, targetVo, ref _voicesVolVel);
        }

        // --- 2) HEARTBEAT & BREATHING (reactive: Stress01) ---
        float tStress = _stress ? Mathf.Clamp01(_stress.Stress)
                                : 1f - CapRelative(_sanity.Sanity01, _sanity.Cap01); // fallback if no Stress

        if (_heartbeat2D)
        {
            float v = Mathf.Lerp(_heartbeatVolAtCalm, _heartbeatVolAtMax, tStress);
            float p = Mathf.Lerp(_heartbeatPitchCalm, _heartbeatPitchMax, tStress);
            _heartbeat2D.volume = Smooth(_heartbeat2D.volume, v, ref _heartbeatVolVel);
            _heartbeat2D.pitch = Smooth(_heartbeat2D.pitch, p, ref _heartbeatPitchVel);
        }

        if (_breathing2D)
        {
            float v = Mathf.Lerp(_breathingVolAtCalm, _breathingVolAtMax, tStress);
            float p = Mathf.Lerp(_breathingPitchCalm, _breathingPitchMax, tStress);
            _breathing2D.volume = Smooth(_breathing2D.volume, v, ref _breathingVolVel);
            _breathing2D.pitch = Smooth(_breathing2D.pitch, p, ref _breathingPitchVel);
        }

        if (logDebug)
        {
            float calmRel = CapRelative(_sanity.Sanity01, _sanity.Cap01);
            float tSanity = 1f - calmRel;
            Debug.Log($"[Router2D] cap={_sanity.Cap01:F2} sanity={_sanity.Sanity01:F2} tSanity={tSanity:F2} stress={tStress:F2} " +
                      $"VO={(_voices2D ? _voices2D.volume : -1f):F2} HB={(_heartbeat2D ? _heartbeat2D.volume : -1f):F2} BR={(_breathing2D ? _breathing2D.volume : -1f):F2}");
        }
    }

    // --- helpers for the effects ---
    static void Setup2D(AudioSource s)
    {
        if (!s) return;
        s.spatialBlend = 0f;
        s.loop = true;
        if (!s.isPlaying) s.Play();
    }

    static float CapRelative(float sanity, float cap01)
    {
        if (cap01 <= 0f) return 0f;
        return Mathf.Clamp01(sanity / cap01); // 1 at cap (calm), 0 near empty
    }

    float Smooth(float current, float target, ref float velocity)
    {
        if (fadeTime <= 0f) return target;
        // Exponential smoothing to be framerate-independent
        float a = 1f - Mathf.Exp(-Time.deltaTime / fadeTime);
        return Mathf.Lerp(current, target, a);
    }
}
