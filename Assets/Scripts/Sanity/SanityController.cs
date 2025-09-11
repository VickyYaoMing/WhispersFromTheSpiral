using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;


//The sanity system is currently not fully implemented.
// This is a placeholder script for ironing out details and testing during development.
// It will be expanded upon in future updates.
//It will start as a single script, and after testing, branch out into multiple scripts as needed (e.g. SanityManager, SanityEffects, SanityUI, etc. (names are placeholders and subject to change))

namespace SanitySystem
{
    // Enum for different sanity states
    public enum SanityState
    {
        Sane,
        Uneasy,
        SlightlyUnstable,
        Unstable,
        Insane
    }
    public interface ISanityProvider
    {
        float GetSanity01();
        event Action<float> OnSanityChanged01; //Fired when a value changes
        event Action<SanityState> OnSanityStateChanged; //Fired when the state changes
        event Action<float> OnSanityImpulse; //Fired when an impulse is applied
    }
    [CreateAssetMenu(menuName = "Sanity/Sanity Tuning Profile", fileName = "SanityTuningProfile")]
    public class SanityTuningProfile : ScriptableObject
    {
        [Header("Rates (pre second)")]
        [Tooltip("Rate at which sanity decreases per second when in a sanity reducing area. Independent of sources, as a function of current sanity level.")]
        public AnimationCurve ambientDrainRate = new AnimationCurve(new Keyframe(0, 0.00f), new Keyframe(1, 0.00f));

        [Tooltip("Passive regeneration as a function of the current sanity level. The higher the sanity, the less it regenerates.")]
        public AnimationCurve regenPerSec = new AnimationCurve(new Keyframe(0, 0.02f), new Keyframe(1f, 0.06f));

        [Header("Clamps")]
        public float clampMin = 0f;
        public float clampMax = 1f;
        [Header("Impulse settings")]
        public float impulseScale = 1f;
        public float impulseMin = -0.5f;
        public float impulseMax = 0.5f;
        [Header("State thresholds")]
        [Range(0, 1)] public float thSane = 0.85f;
        [Range(0, 1)] public float thUneasy = 0.70f;
        [Range(0, 1)] public float thSlightlyUnstable = 0.60f;
        [Range(0, 1)] public float thUnstable = 0.40f;
        //Insane is anything below thUnstable

        [Header("Hysterias")]
        [Tooltip("Hysteresis to prevent rapid state changes. Applied when entering a state, so the exit threshold is lower than the entry threshold.")]
        public float hysteris = 0.03f; // Subject to change after testing

        public SanityState EvalState(float sanity, SanityState previousState)
        {
            float up = hysteris, down = -hysteris;
            switch (previousState)
            {
                case SanityState.Sane:
                    if (sanity < thUneasy + down) return SanityState.Uneasy;
                    return SanityState.Sane;
                case SanityState.Uneasy:
                    if (sanity >= thSane + up) return SanityState.Sane;
                    if (sanity < thSlightlyUnstable + down) return SanityState.SlightlyUnstable;
                    return SanityState.Uneasy;
                case SanityState.SlightlyUnstable:
                    if (sanity >= thUneasy + up) return SanityState.Uneasy;
                    if (sanity < thUnstable + down) return SanityState.Unstable;
                    return SanityState.SlightlyUnstable;
                case SanityState.Unstable:
                    if (sanity >= thSlightlyUnstable + up) return SanityState.SlightlyUnstable;
                    if (sanity < 0f + down) return SanityState.Insane;
                    if (sanity <= 0.0001f) return SanityState.Insane; // To prevent getting stuck in Unstable state due to float precision issues
                    return SanityState.Unstable;
                case SanityState.Insane:
                    if (sanity >= thUnstable + up) return SanityState.Unstable;
                    return SanityState.Insane;
            }
            return previousState; // Should never reach here
        }

    }
    public class SanityController : MonoBehaviour, ISanityProvider
    {
        [SerializeField] private SanityTuningProfile tuningProfile;
        [SerializeField, Range(0, 1)] private float sanity = 1f; // Current sanity level (0 to 1)
        [SerializeField] private Transform playerTransform; // Reference to the player's transform

        //Global registry of all sanity sources affecting the player
        public static readonly List<ISanityAffector> affectors = new List<ISanityAffector>();

        public event Action<float> OnSanityChanged01;
        public event Action<SanityState> OnSanityStateChanged;
        public event Action<float> OnSanityImpulse;
        [Header("Prototype Helpers for now")] public bool spawnTestingRig = false; //when true, spawns a testing rig for sanity impulses
        SanityState state = SanityState.Sane;

        void Awake()
        {
            if (playerTransform)
                playerTransform = transform;
        }
        void Start()
        {
            if (spawnTestingRig)
            {
                var dbg = gameObject.AddComponent<SanityDebugPanel>();
                dbg.controller = this;

                var go = new GameObject("Sanity Impulse Tester");
                go.transform.position = transform.position + transform.forward * 2f; //value can be changed as needed
                var aff = go.AddComponent<SanityAreaAffector>();
                aff.maxRange = 5f;
                aff.strength01 = 0.08f;
                aff.falloff = sanityAreaAffector.FalloffType.SmoothStep;
            }
        }
        void OnEnable() { EmitAll(); }
        void Update()
        {

        }
        public void AddImpulse01(float delta01)
        {

        }
        public void SetSanity01(float v)
        {

        }
        public float GetSanity01() { return sanity; }
        void EmitAll()
        {
            OnSanityChanged01?.Invoke(sanity);
            OnSanityStateChanged?.Invoke(state);
        }
        public interface ISanityAffector
        {
            float GetDrain01PerSec(Vector3 playerPos);
        }
        public abstract class SanityAffectorBase : MonoBehaviour, ISanityAffector
        {
            [Range(0, 1)] public float strength01 = 0.1f;
            [Tooltip("Max influence distance (meters). 0 = unlimited")]
            public float maxRange = 10f;


            protected virtual void OnEnable() { SanityController.affectors.Add(this); } //register on enable
            protected virtual void OnDisable() { SanityController.affectors.Remove(this); } //unregister on disable


            public abstract float GetDrain01PerSec(Vector3 playerPos);
            protected float DistanceTo(Vector3 playerPos) => Vector3.Distance(playerPos, transform.position);
        }
        public class SanityAreaAffector : SanityAffectorBase
        {

        }
    }
}
