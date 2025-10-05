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
