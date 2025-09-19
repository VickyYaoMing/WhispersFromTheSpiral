using System;
using UnityEngine;

namespace SanitySystem
{
    public enum SanityState { Sane, Uneasy, Afraid, Panicked, Insane }
    public interface ISanityProvider
    {
        float Sanity01 { get; }
        void SetSanity(float value);
        void ApplyImpulse(float impulse);

        event Action<float> OnSanityChanged;
        event Action<SanityState> OnSanityStateChanged;
        event Action<float> OnImpulseApplied;
    }
    public class Sanity : MonoBehaviour, ISanityProvider
    {
        [Header("Sanity Value (0..1)")]
        [Tooltip("1 = calm/sane, 0 = overwhelmed/insane phase")]
        [Range(0f, 1f)][SerializeField] float _sanity01 = 1f;
        public float Sanity01 => _sanity01;


        [Header("Regeneration (per second)")]
        [Tooltip("Regeneration per second at sanity = 1 (calm)")]
        [Range(0f, 0.5f)] public float RegenRateAtFull = 0.06f;
        [Tooltip("Regeneration per second at sanity = 0 (broken)")]
        [Range(0f, 0.5f)] public float RegenRateAtEmpty = 0.02f;
        [Tooltip("Curve shaping: >1 means slower regen at low sanity")]
        [Range(0.2f, 3f)] public float RegenCurveExponent = 1.0f;


        [Header("Ambient Pressure (per second)")]
        [Tooltip("Baseline global drain per second at sanity = 1")]
        [Range(0f, 0.5f)] public float AmbientDrainAtFull = 0.00f;
        [Tooltip("Baseline global drain per second at sanity = 0")]
        [Range(0f, 0.5f)] public float AmbientDrainAtEmpty = 0.00f;
        [Tooltip("Curve shaping: >1 makes drain rise faster as sanity drops")]
        [Range(0.2f, 3f)] public float AmbientCurveExponent = 1.0f;


        [Header("Impulses (one-shots)")]
        [Tooltip("Scales ApplyImpulse values. Negative reduces sanity.")]
        public float ImpulseScale = 1f;
        public float ImpulseMin = -0.5f; // e.g., jumpscare = -0.15
        public float ImpulseMax = 0.5f;


        [Header("Clamps")]
        [Range(0f, 1f)] public float ClampMin = 0f;
        [Range(0f, 1f)] public float ClampMax = 1f;


        [Header("State Thresholds + Hysteresis (on Sanity)")]
        [Tooltip("Entry thresholds from high to low sanity. Hysteresis prevents flicker on recovery.")]
        [Range(0, 1)] public float ThresholdUneasy = 0.85f;
        [Range(0, 1)] public float ThresholdAfraid = 0.65f;
        [Range(0, 1)] public float ThresholdPanicked = 0.45f;
        [Range(0, 1)] public float ThresholdInsane = 0.20f;
        [Min(0f)] public float Hysteresis = 0.03f;


        // [Header("Debug UI")] public bool ShowDebugPanel = true;


        public event Action<float> OnSanityChanged;
        public event Action<SanityState> OnSanityStateChanged;
        public event Action<float> OnImpulseApplied;


        SanityState _state = SanityState.Sane;


        void OnEnable() { EmitAll(); }

        void Update()
        {
            float dt = Mathf.Max(0f, Time.deltaTime);


            float regen = Mathf.Lerp(RegenRateAtEmpty, RegenRateAtFull, Pow01(_sanity01, RegenCurveExponent));
            float ambient = Mathf.Lerp(AmbientDrainAtEmpty, AmbientDrainAtFull, Pow01(_sanity01, AmbientCurveExponent));


            SetSanity(_sanity01 + (regen - ambient) * dt);
        }
        public void SetSanity(float value)
        {
            float v = Mathf.Clamp(value, ClampMin, ClampMax);
            if (!Mathf.Approximately(v, _sanity01))
            {
                _sanity01 = v;
                OnSanityChanged?.Invoke(_sanity01);


                var next = EvaluateState(_sanity01, _state);
                if (next != _state)
                {
                    _state = next;
                    OnSanityStateChanged?.Invoke(_state);
                }
            }
        }
        public void ApplyImpulse(float impulse)
        {
            float d = Mathf.Clamp(impulse * ImpulseScale, ImpulseMin, ImpulseMax);
            SetSanity(_sanity01 + d);
            OnImpulseApplied?.Invoke(d);
        }
        SanityState EvaluateState(float s, SanityState previous)
        {
            float up = Hysteresis, down = -Hysteresis;
            switch (previous)
            {
                case SanityState.Sane:
                    if (s < ThresholdUneasy + down) return SanityState.Uneasy;
                    return SanityState.Sane;
                case SanityState.Uneasy:
                    if (s >= 1f - up) return SanityState.Sane;
                    if (s < ThresholdAfraid + down) return SanityState.Afraid;
                    return SanityState.Uneasy;
                case SanityState.Afraid:
                    if (s >= ThresholdUneasy + up) return SanityState.Uneasy;
                    if (s < ThresholdPanicked + down) return SanityState.Panicked;
                    return SanityState.Afraid;
                case SanityState.Panicked:
                    if (s >= ThresholdAfraid + up) return SanityState.Afraid;
                    if (s < ThresholdInsane + down) return SanityState.Insane;
                    return SanityState.Panicked;
                case SanityState.Insane:
                    if (s >= ThresholdPanicked + up) return SanityState.Panicked;
                    return SanityState.Insane;
            }
            return previous;
        }
        static float Pow01(float x, float p)
        {
            return Mathf.Pow(Mathf.Clamp01(x), Mathf.Max(0.0001f, p));
        }
        void EmitAll()
        {
            OnSanityChanged?.Invoke(_sanity01);
            OnSanityStateChanged?.Invoke(_state);
        }

    }
}