// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// // using System.Numerics;
// using UnityEngine;


// //The sanity system is currently not fully implemented.
// // This is a placeholder script for ironing out details and testing during development.
// // It will be expanded upon in future updates.
// //It will start as a single script, and after testing, branch out into multiple scripts as needed (e.g. SanityManager, SanityEffects, SanityUI, etc. (names are placeholders and subject to change))

// namespace SanitySystem
// {
//     // Enum for different sanity states
//     public enum SanityState
//     {
//         Sane,
//         Uneasy,
//         SlightlyUnstable,
//         Unstable,
//         Insane
//     }
//     public interface ISanityProvider
//     {
//         float GetSanity01();
//         event Action<float> OnSanityChanged01; //Fired when a value changes
//         event Action<SanityState> OnSanityStateChanged; //Fired when the state changes
//         event Action<float> OnSanityImpulse; //Fired when an impulse is applied
//     }
//     [CreateAssetMenu(menuName = "Sanity/Sanity Tuning Profile", fileName = "SanityTuningProfile")]
//     public class SanityTuningProfile : ScriptableObject
//     {
//         [Header("Rates (pre second)")]
//         [Tooltip("Rate at which sanity decreases per second when in a sanity reducing area. Independent of sources, as a function of current sanity level.")]
//         public AnimationCurve ambientDrainRate = new AnimationCurve(new Keyframe(0, 0.00f), new Keyframe(1, 0.00f));

//         [Tooltip("Passive regeneration as a function of the current sanity level. The higher the sanity, the less it regenerates.")]
//         public AnimationCurve regenPerSec = new AnimationCurve(new Keyframe(0, 0.02f), new Keyframe(1f, 0.06f));

//         [Header("Clamps")]
//         public float clampMin = 0f;
//         public float clampMax = 1f;
//         [Header("Impulse settings")]
//         public float impulseScale = 1f;
//         public float impulseMin = -0.5f;
//         public float impulseMax = 0.5f;
//         [Header("State thresholds")]
//         [Range(0, 1)] public float thSane = 0.85f;
//         [Range(0, 1)] public float thUneasy = 0.70f;
//         [Range(0, 1)] public float thSlightlyUnstable = 0.60f;
//         [Range(0, 1)] public float thUnstable = 0.40f;
//         //Insane is anything below thUnstable

//         [Header("Hysterias")]
//         [Tooltip("Hysteresis to prevent rapid state changes. Applied when entering a state, so the exit threshold is lower than the entry threshold.")]
//         public float hysteris = 0.03f; // Subject to change after testing

//         public SanityState EvalState(float sanity, SanityState previousState)
//         {
//             float up = hysteris, down = -hysteris;
//             switch (previousState)
//             {
//                 case SanityState.Sane:
//                     if (sanity < thUneasy + down) return SanityState.Uneasy;
//                     return SanityState.Sane;
//                 case SanityState.Uneasy:
//                     if (sanity >= thSane + up) return SanityState.Sane;
//                     if (sanity < thSlightlyUnstable + down) return SanityState.SlightlyUnstable;
//                     return SanityState.Uneasy;
//                 case SanityState.SlightlyUnstable:
//                     if (sanity >= thUneasy + up) return SanityState.Uneasy;
//                     if (sanity < thUnstable + down) return SanityState.Unstable;
//                     return SanityState.SlightlyUnstable;
//                 case SanityState.Unstable:
//                     if (sanity >= thSlightlyUnstable + up) return SanityState.SlightlyUnstable;
//                     if (sanity < 0f + down) return SanityState.Insane;
//                     if (sanity <= 0.0001f) return SanityState.Insane; // To prevent getting stuck in Unstable state due to float precision issues
//                     return SanityState.Unstable;
//                 case SanityState.Insane:
//                     if (sanity >= thUnstable + up) return SanityState.Unstable;
//                     return SanityState.Insane;
//             }
//             return previousState; // Should never reach here
//         }

//     }
//     public class SanityController : MonoBehaviour, ISanityProvider
//     {
//         [SerializeField] private SanityTuningProfile tuningProfile;
//         [SerializeField, Range(0, 1)] private float sanity = 1f; // Current sanity level (0 to 1)
//         [SerializeField] private Transform playerTransform; // Reference to the player's transform

//         //Global registry of all sanity sources affecting the player
//         public static readonly List<ISanityAffector> affectors = new List<ISanityAffector>();

//         public event Action<float> OnSanityChanged01;
//         public event Action<SanityState> OnSanityStateChanged;
//         public event Action<float> OnSanityImpulse;
//         [Header("Prototype Helpers for now")] public bool spawnTestingRig = false; //when true, spawns a testing rig for sanity impulses
//         SanityState state = SanityState.Sane;

//         void Awake()
//         {
//             if (playerTransform)
//                 playerTransform = transform;
//         }
//         void Start()
//         {
//             if (spawnTestingRig)
//             {
//                 var dbg = gameObject.AddComponent<SanityDebugPanel>();
//                 dbg.controller = this;

//                 var go = new GameObject("Sanity Impulse Tester");
//                 go.transform.position = transform.position + transform.forward * 2f; //value can be changed as needed
//                 var aff = go.AddComponent<SanityAreaAffector>();
//                 aff.maxRange = 5f;
//                 aff.strength01 = 0.08f;
//                 aff.falloff = SanityAreaAffector.Falloff.SmoothStep;
//             }
//         }
//         void OnEnable() { EmitAll(); }
//         void Update()
//         {

//         }
//         public void AddImpulse01(float delta01)
//         {

//         }
//         public void SetSanity01(float v)
//         {

//         }
//         public float GetSanity01() { return sanity; }
//         void EmitAll()
//         {
//             OnSanityChanged01?.Invoke(sanity);
//             OnSanityStateChanged?.Invoke(state);
//         }
//         public interface ISanityAffector
//         {
//             float GetDrain01PerSec(Vector3 playerPos);
//         }
//         public abstract class SanityAffectorBase : MonoBehaviour, ISanityAffector
//         {
//             [Range(0, 1)] public float strength01 = 0.1f;
//             [Tooltip("Max influence distance (meters). 0 = unlimited")]
//             public float maxRange = 10f;


//             protected virtual void OnEnable() { SanityController.affectors.Add(this); } //register on enable
//             protected virtual void OnDisable() { SanityController.affectors.Remove(this); } //unregister on disable


//             public abstract float GetDrain01PerSec(Vector3 playerPos);
//             protected float DistanceTo(Vector3 playerPos) => Vector3.Distance(playerPos, transform.position);
//         }
//         public class SanityAreaAffector : SanityAffectorBase
//         {
//             public enum Falloff { Linear, SmoothStep, InverseSquare }
//             public Falloff falloff = Falloff.SmoothStep;

//             public override float GetDrain01PerSec(Vector3 playerPos)
//             {
//                 if (maxRange <= 0f) //unlimited range
//                     return 0f;
//                 float d = DistanceTo(playerPos);
//                 if (d > maxRange) return 0f; //out of range
//                 float t = Mathf.Clamp01(1f - (d / maxRange));
//                 float w = falloff switch
//                 {
//                     Falloff.Linear => t,
//                     Falloff.SmoothStep => t * t * (3f - 2f * t),
//                     Falloff.InverseSquare => 1f / Mathf.Max(1f, d * d), //avoid infinity at d=0
//                     _ => t
//                 };
//                 return strength01 * w;
//             }
//         }
//         //One shot expanding wave that applies a single impulse when the front crosses the player
//         //Hook this to a kind of sound cue (scream, clang, etc) and then call Fire() on play.
//         public class SanityShockwave : MonoBehaviour
//         {
//             [Range(-1f, 1f)] public float impulseDelta01 = -0.2f; //negative = drain, positive = restore
//             public float startRadius = 0f;
//             public float endRadius = 12f; //can be changed as needed for testing and balancing
//             public float speed = 10f; //meters per second
//             public float thickness = 1.0f; //how thick the wave is

//             [SerializeField] SanityController target; // explicit reference to the target sanity controller
//             double startTime = -1;
//             bool consumed;

//             void OnEnable()
//             {
//                 consumed = false;
//             }
//             public void Fire(SanityController to = null)
//             {
//                 target = to ? to : target;
//                 startTime = AudioSettings.dspTime; //use audio time for better sync with sound effects
//                 consumed = false;
//             }
//             void Update()
//             {
//                 if (startTime < 0) return; //not fired yet
//                 var ctrl = target ? target : FindAnyObjectByType<SanityController>();
//                 if (!ctrl) { enabled = false; return; } //no target, disable

//                 double t = AudioSettings.dspTime - startTime;
//                 float radius = startRadius + speed * (float)t;
//                 if (radius > endRadius + thickness)
//                 {
//                     startTime = -1; //done
//                     return;
//                 }
//                 float dist = Vector3.Distance(ctrl.transform.position, transform.position);
//                 if (!consumed && dist >= radius - thickness * 0.5f && dist <= radius + thickness * 0.5f)
//                 {
//                     ctrl.AddImpulse01(impulseDelta01);
//                     consumed = true; //only once
//                 }
//             }

//         }
//         //Apply a one-time impulse when the player enters a trigger
//         [RequireComponent(typeof(Collider))]
//         public class SanityTriggerAffector : MonoBehaviour
//         {
//             public float delta01 = -0.1f;
//             public string playerTag = "Player"; //tag to identify the player
//             public SanityController controller;

//             void Reset()
//             {
//                 var col = GetComponent<Collider>();
//                 col.isTrigger = true;
//             }
//             void OnTriggerEnter(Collider other)
//             {
//                 if (!other.CompareTag(playerTag)) return;

//                 var ctrl = controller ? controller : other.GetComponent<SanityController>();

//                 if (ctrl)
//                 {
//                     ctrl.AddImpulse01(delta01);
//                 }
//             }
//         }
//         //this is for debugging, so it can, and probably will, be removed later
//         public class SanityDebugPanel : MonoBehaviour
//         {
//             public SanityController controller;
//             [Range(-1f, 1f)] public float editorSanityOverride = -1f; //-1 = off

//             void Update()
//             {
//                 if (!controller) return;
//                 if (editorSanityOverride >= 0f)
//                 {
//                     controller.SetSanity01(editorSanityOverride);
//                 }
//             }
// #if UNITY_EDITOR
//             void OnGUI()
//             {
//                 if (!controller) return;
//                 GUILayout.BeginArea(new Rect(12, 12, 300, 180), GUI.skin.box); //values can be changed to what works best
//                 GUILayout.Label("<b>Sanity Debug</b>");
//                 GUILayout.Label($"Sanity: {(controller.GetSanity01() * 100f):F0}%");

//                 float val = GUILayout.HorizontalSlider(controller.GetSanity01(), 0f, 1f, GUILayout.Width(260));
//                 controller.SetSanity01(val);
//                 if (GUILayout.Button("Test jumpscare (-0.15)")) controller.AddImpulse01(-0.15f); //value just has to be changed as needed
//                 GUILayout.EndArea();
//             }
// #endif
//         }
//     }
// }
