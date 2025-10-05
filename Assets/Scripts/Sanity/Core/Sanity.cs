using System;
using System.Collections;
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
    [DisallowMultipleComponent]
    public class Sanity : MonoBehaviour, ISanityProvider
    {
        //------------------------- PHASE OF GAME -------------------------
        [Header("Phase Profile (slow, narrative)")]
        public SanityPhaseProfile phaseProfile;
        public int startPhaseIndex = 0;
        [Header("Phase Cap (computed)")]
        [SerializeField, Range(0f, 1f)] float _cap01 = 1f;
        public float Cap01 => _cap01;
        public int PhaseIndex { get; private set; } = -1;
        //----------------------------------------------------------------------------

        [Header("Sanity Value (0..1)")]
        [Tooltip("1 = calm/sane, 0 = overwhelmed/insane phase")]
        [Range(0f, 1f)][SerializeField] float _baseSanity = 1f;
        public float Sanity01 => _baseSanity;


        [Header("Regeneration (per second)")]
        [Tooltip("Regeneration per second at sanity = 1 (calm)")]
        [Range(0f, 0.5f)] public float _regenRateAtFull = 0.01f;
        [Tooltip("Regeneration per second at sanity = 0 (broken)")]
        [Range(0f, 0.5f)] public float _regenRateAtEmpty = 0.02f;
        [Tooltip("Curve shaping: >1 means slower regen at low sanity")]
        [Range(0.2f, 3f)] public float _regenCurveExponent = 1.0f;


        //The ambient drain is a constant drain on sanity that occurs over time.
        //This simulates the natural stress of the environment and situation.
        // Note: For this work in a good and balanced way, it should be limited to "acts" as discussed before.
        // Capping it to certian min and max values depending on how far along the player is in the game.
        [Header("Ambient Pressure (per second)")]
        [Tooltip("Baseline global drain per second at sanity = 1")]
        [Range(0f, 0.5f)] public float _ambientDrainAtFull = 0.00f; //Used to decrease the passive sanity (passive sanity drain)
        [Tooltip("Baseline global drain per second at sanity = 0")]
        [Range(0f, 0.5f)] public float _ambientDrainAtEmpty = 0.00f;
        [Tooltip("Curve shaping: >1 makes drain rise faster as sanity drops")]
        [Range(0.2f, 3f)] public float _ambientCurveExponent = 1.0f;

        // External ambient control (global knobs)
        [SerializeField, Min(0f)] float _ambientMultiplier = 1f;
        [SerializeField, Min(0f)] float _ambientAdditivePerSec = 0f;
        public float AmbientMultiplier { get => _ambientMultiplier; set => _ambientMultiplier = Mathf.Max(0f, value); }
        public void SetAmbientAdditive(float perSecond) { _ambientAdditivePerSec = Mathf.Max(0f, perSecond); }
        public void AddAmbientForSeconds(float perSecond, float seconds)
        {
            if (perSecond > 0f && seconds > 0f)
            {
                StartCoroutine(AmbientBurst(perSecond, seconds));
            }

            IEnumerator AmbientBurst(float perSecond, float seconds)
            {
                _ambientAdditivePerSec += perSecond;
                yield return new WaitForSeconds(seconds);
                _ambientAdditivePerSec = Mathf.Max(0f, _ambientAdditivePerSec - perSecond);
            }
        }

        [Header("External Sanity Drain (e.g., enemy stare/chase)")] //WILL be used
        [Tooltip("Extra drain/sec set by other systems (SanityDrainOnLook).")] //will be implemented when in call with thiti so no errors occur T_T
        [SerializeField, Min(0f)] float _externalDrainPerSec = 0f;
        [Tooltip("If Stress/Panic is ABOVE this, we DO NOT drain sanity (panic dominates).")] //So that stress is the main component but sanity can still be affected by the enemy.
        [Range(0f, 1f)] public float stressGateForDrain = 0.35f;
        public void SetExternalDrain(float perSecond) => _externalDrainPerSec = Mathf.Max(0f, perSecond);


        [Header("Impulses (one-shots)")]
        [Tooltip("Scales ApplyImpulse values. Negative reduces sanity.")]
        //Negative values are sanity loss, positive values are sanity gain.
        public float _impulseScale = 1f; //Scales the impulse applied. (1 is default, can be tuned up or down as needed)
        public float _impulseMin = -0.5f; //Min value that can be reduced.
        public float _impulseMax = 0.5f; //Max value that can be increased.


        [Header("Clamps")]
        [Range(0f, 1f)] public float _clampMin = 0f;
        [Range(0f, 1f)] public float _clampMax = 1f;


        [Header("State Thresholds + Hysteresis (on Sanity)")]
        [Tooltip("Entry thresholds from high to low sanity. Hysteresis prevents flicker on recovery.")]
        [Range(0, 1)] public float _thresholdUneasy = 0.85f;
        [Range(0, 1)] public float _thresholdAfraid = 0.65f;
        [Range(0, 1)] public float _thresholdPanicked = 0.45f;
        [Range(0, 1)] public float _thresholdInsane = 0.20f;
        [Min(0f)] public float _hysteresis = 0.03f;


        //Different events used to notify other systems of changes in sanity.
        public event Action<float> OnSanityChanged;
        public event Action<SanityState> OnSanityStateChanged;
        public event Action<float> OnImpulseApplied;


        SanityState _state = SanityState.Sane;


        void OnEnable() { EmitAll(); }
        void Update()
        {
            float dt = Mathf.Max(0f, Time.deltaTime);
            float regenBase = Mathf.Lerp(_regenRateAtEmpty, _regenRateAtFull, Pow01(_baseSanity, _regenCurveExponent));
            float ambientBase = Mathf.Lerp(_ambientDrainAtEmpty, _ambientDrainAtFull, Pow01(_baseSanity, _ambientCurveExponent));
            float ambient = Mathf.Max(0f, ambientBase * _ambientMultiplier + _ambientAdditivePerSec);

            // external drain (enemy stare/chase) is applied by other systems; we just integrate it
            float extDrain = _externalDrainPerSec;

            SetSanity(_baseSanity + (Mathf.Max(0f, regenBase) - (ambient + extDrain)) * dt);
            Debug.Log($"Current State: {_state}"); //Debug statement.

        }
        // --- Phase control (call from your act/puzzle manager) ---
        public void SetPhaseIndex(int index, bool keepRelativeLevel = true)
        {
            PhaseIndex = index;
            float oldCap = _cap01;
            _cap01 = 1f;
            if (phaseProfile && index >= 0 && index < phaseProfile.phases.Length)
                _cap01 = Mathf.Clamp01(phaseProfile.phases[index].maxSanityCap);

            if (keepRelativeLevel && oldCap > 0f)
            {
                float frac = _baseSanity / Mathf.Max(0.0001f, oldCap);
                SetSanity(frac * _cap01);
            }
            else
            {
                SetSanity(Mathf.Min(_baseSanity, _cap01));
            }
        }
        public void SetPhaseId(string id, bool keepRelativeLevel = true)
        {
            if (!phaseProfile) return;
            int idx = phaseProfile.IndexOf(id);
            if (idx >= 0) SetPhaseIndex(idx, keepRelativeLevel);
        }
        public void SetSanity(float value)
        {
            float _value = Mathf.Clamp(value, _clampMin, _clampMax);
            if (!Mathf.Approximately(_value, _baseSanity))
            {
                _baseSanity = _value;
                OnSanityChanged?.Invoke(_baseSanity);


                var _next = EvaluateState(_baseSanity, _state);
                if (_next != _state)
                {
                    _state = _next;
                    OnSanityStateChanged?.Invoke(_state);
                }
            }
        }
        // Used to apply sudden changes to sanity, e.g., from events. 
        // When you want to apply it, call ApplyImpulse(delta), or sanity.ApplyImpulse(delta);
        // Where delta is a float value representing the change in sanity.
        public void ApplyImpulse(float impulse)
        {
            float _d = Mathf.Clamp(impulse * _impulseScale, _impulseMin, _impulseMax);
            SetSanity(_baseSanity + _d);
            OnImpulseApplied?.Invoke(_d);
        }

        // --- Phase-driven FX helpers ---
        public float GetBaseVignette()
        {
            if (!phaseProfile || PhaseIndex < 0) return 0f;
            return phaseProfile.phases[PhaseIndex].vignetteBase;
        }

        public void GetVoices(out float bedVolume, out float densityPerMin)
        {
            if (!phaseProfile || PhaseIndex < 0) { bedVolume = 0f; densityPerMin = 0f; return; }
            var ph = phaseProfile.phases[PhaseIndex];

            float t = 1f - (_cap01 <= 0f ? 0f : (_baseSanity / _cap01)); // 0 calm→1 stressed (within cap)
            bedVolume = Mathf.Lerp(ph.voicesBedVolumeAtCalm, ph.voicesBedVolumeAtMin, t);
            densityPerMin = Mathf.Lerp(ph.voicesDensityPerMinAtCalm, ph.voicesDensityPerMinAtMin, t);
        }



        // Checking the different thresholds
        SanityState EvaluateState(float _sanity, SanityState _previousState)
        {
            float _up = _hysteresis, _down = -_hysteresis;
            switch (_previousState)
            {
                case SanityState.Sane:
                    if (_sanity < _thresholdUneasy + _down) return SanityState.Uneasy; return SanityState.Sane;
                case SanityState.Uneasy:
                    if (_sanity >= 1f - _up) return SanityState.Sane;
                    if (_sanity < _thresholdAfraid + _down) return SanityState.Afraid;
                    return SanityState.Uneasy;
                case SanityState.Afraid:
                    if (_sanity >= _thresholdUneasy + _up) return SanityState.Uneasy;
                    if (_sanity < _thresholdPanicked + _down) return SanityState.Panicked;
                    return SanityState.Afraid;
                case SanityState.Panicked:
                    if (_sanity >= _thresholdAfraid + _up) return SanityState.Afraid;
                    if (_sanity < _thresholdInsane + _down) return SanityState.Insane;
                    return SanityState.Panicked;
                case SanityState.Insane:
                    if (_sanity >= _thresholdPanicked + _up) return SanityState.Panicked;
                    return SanityState.Insane;
            }
            return _previousState;
        }
        static float Pow01(float x, float p)
        {
            return Mathf.Pow(Mathf.Clamp01(x), Mathf.Max(0.0001f, p));
        }
        void EmitAll()
        {
            OnSanityChanged?.Invoke(_baseSanity);
            OnSanityStateChanged?.Invoke(_state);
        }

    }
}