using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using SanitySystem;

public class URP_SanityStressVFX : MonoBehaviour
{
    public Sanity _sanity;
    public StressController _stress;
    public Volume _volume;
    [Range(0f, 1f)] public float fadeTime = 0.12f;

    //Vignette (base from sanity + overlay from stress/panic)
    [Range(0, 1)] public float vigBaseCalm = 0.10f, vigBaseEmpty = 0.25f;
    [Range(0, 1)] public float vigOverlayStress = 0.45f;
    [Range(0, 1)] public float vigSmoothness = 0.9f;
    public bool vignetteRounded = true;

    //Lens distortion (-pinching edges)
    [Range(-1, 1)] public float ldCalm = -0.05f, ldEmpty = -0.20f, ldOverlayStress = -0.10f;

    //Chromatic Aberration
    [Range(0, 1)] public float caCalm = 0.00f, caEmpty = 0.15f, caOverlayStress = 0.35f;

    //Blur with stress (Depth of field)
    public bool driveDoF = true;
    [Range(0.1f, 32f)] public float dofAperatureCalm = 4f, dofAperatureStress = 12f;
    [Range(0.1f, 300f)] public float dofFocusDistance = 4f;
    [Range(1f, 300f)] public float dofFocalLength = 50f;

    Vignette vig;
    LensDistortion ld;
    ChromaticAberration ca;
    DepthOfField dof;
    float vVel, ldVel, caVel, dofVel;

    void Reset()
    {
        if (!_sanity) _sanity = GetComponent<Sanity>();
        if (!_stress) _stress = GetComponent<StressController>();
        if (!_volume) _volume = GetComponent<Volume>();
    }
    void Start()
    {
        if (!_volume) return;

        if (!_volume.profile)
            _volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();

        // Create/Fetch overrides (no ??= on TryGet; it's a bool)
        if (!_volume.profile.TryGet(out vig))
            vig = _volume.profile.Add<Vignette>(true);

        if (!_volume.profile.TryGet(out ld))
            ld = _volume.profile.Add<LensDistortion>(true);

        if (!_volume.profile.TryGet(out ca))
            ca = _volume.profile.Add<ChromaticAberration>(true);

        if (driveDoF)
        {
            if (!_volume.profile.TryGet(out dof))
                dof = _volume.profile.Add<DepthOfField>(true);

            dof.mode.Override(DepthOfFieldMode.Bokeh);
        }

        // Enable and mark overrides
        if (vig)
        {
            vig.active = true;
            vig.intensity.overrideState = true;
            vig.smoothness.overrideState = true;
        }

        if (ld)
        {
            ld.active = true;
            ld.intensity.overrideState = true;
            ld.center.overrideState = true;
            ld.scale.overrideState = true;
            ld.center.value = new Vector2(0.5f, 0.5f);
            ld.scale.value = 1f;
        }

        if (ca)
        {
            ca.active = true;
            ca.intensity.overrideState = true;
        }

        if (dof)
        {
            dof.active = true;
            dof.aperture.overrideState = true;
            dof.focusDistance.overrideState = true;
            dof.focalLength.overrideState = true;
        }
    }
    void LateUpdate()
    {
        if (!_sanity || !_volume) return;

        float tStress = _stress ? Mathf.Clamp01(_stress.Stress) : 0f;

        // Get phase-driven bases; fall back to local sliders if profile missing
        float baseV = 0.10f, baseCA = 0.00f, baseLD = -0.05f; // fallback defaults
        _sanity.TryGetVfxFloors(out baseV, out baseCA, out baseLD);

        // Vignette
        if (vig)
        {
            float addV = Mathf.Lerp(0f, vigOverlayStress, tStress);
            float target = Mathf.Clamp01(baseV + addV);

            vig.intensity.value = Smooth(vig.intensity.value, target, ref vVel);
            vig.smoothness.value = vigSmoothness;
            vig.rounded.value = vignetteRounded; // bool in URP
        }

        // Lens Distortion
        if (ld)
        {
            float addLD = Mathf.Lerp(0f, ldOverlayStress, tStress);
            float target = Mathf.Clamp(baseLD + addLD, -1f, 1f);
            ld.intensity.value = Smooth(ld.intensity.value, target, ref ldVel);
        }

        // Chromatic Aberration
        if (ca)
        {
            float addCA = Mathf.Lerp(0f, caOverlayStress, tStress);
            float target = Mathf.Clamp01(baseCA + addCA);
            ca.intensity.value = Smooth(ca.intensity.value, target, ref caVel);
        }

        // Depth of Field
        if (dof && driveDoF)
        {
            float targetA = Mathf.Lerp(dofAperatureCalm, dofAperatureStress, tStress);
            dof.aperture.value = Smooth(dof.aperture.value, targetA, ref dofVel);
            dof.focusDistance.value = dofFocusDistance;
            dof.focalLength.value = dofFocalLength;
        }
    }
    float Smooth(float current, float target, ref float vel)
    {
        if (fadeTime <= 0f) return target;
        float a = 1f - Mathf.Exp(-Time.deltaTime / fadeTime);
        return Mathf.Lerp(current, target, a);
    }
}
