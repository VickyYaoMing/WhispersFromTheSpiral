using UnityEngine;

[CreateAssetMenu(menuName = "Sanity/Sanity Phase Profile", fileName = "SanityPhaseProfile")]
public class SanityPhaseProfile : ScriptableObject
{
    [System.Serializable]
    public struct Phase
    {
        public string id;
        [Range(0f, 1f)] public float maxSanityCap; // 1.00 -> 0.8 -> 0.6 etc (depending on the phase max)

        [Header("Base vignette (persistent)")]
        [Range(0f, 1f)] public float vignetteBase;

        [Header("Voices (bed + density)")]
        [Range(0f, 1f)] public float voicesBedVolumeAtCalm;
        [Range(0f, 1f)] public float voicesBedVolumeAtMin;
        [Range(0f, 60f)] public float voicesDensityPerMinAtCalm;
        [Range(0f, 60f)] public float voicesDensityPerMinAtMin;

        //Vignette, Chromic Aberration, lens distortion
        [Header("VFX floors (base from Sanity within this phase)")]
        [Range(0, 1)] public float vignetteAtCalm;   // e.g. 0.08 → 0.12 → 0.18 per phase
        [Range(0, 1)] public float vignetteAtMin;    // e.g. 0.28 → 0.34 → 0.40

        [Range(0, 1)] public float chromAbAtCalm;    // e.g. 0.00 → 0.03 → 0.06
        [Range(0, 1)] public float chromAbAtMin;     // e.g. 0.10 → 0.18 → 0.25

        [Header("Lens Distortion base (optional)")]
        [Range(-1, 1)] public float lensDistAtCalm;  // e.g. -0.05 → -0.08 → -0.12
        [Range(-1, 1)] public float lensDistAtMin;   // e.g. -0.15 → -0.18 → -0.22

        [Header("Depth of field")]
        public bool dofEnabled;
        [Range(0f, 32f)] public float dofAperturef; //Base blue (idk exact value, but play around with this.... I think
    }
    public Phase[] phases;

    public int IndexOf(string phaseId)
    {
        for (int i = 0; i < (phases?.Length ?? 0); i++)
        {
            if (phases[i].id == phaseId)
            {
                return i;
            }
        }
        return -1;
    }
    public Phase Get(int index)
    {
        if (phases == null || index < 0 || index >= phases.Length)
        {
            return default;
        }
        return phases[index];
    }
}
