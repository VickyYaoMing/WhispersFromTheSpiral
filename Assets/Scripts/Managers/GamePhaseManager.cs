using UnityEngine;
using SanitySystem;
using UnityEngine.Events;
using UnityEngine.Audio;

[DisallowMultipleComponent]
public class GamePhaseManager : MonoBehaviour
{
    public GamePhaseManager Instance { get; set; }
    public Sanity sanity;
    public bool keepRelativeOnChange = true;
    void Awake()
    {
        if (sanity == null)
        {
            sanity = FindAnyObjectByType<Sanity>();
        }
    }

    public void SetPhase(int index)
    {
        if (!sanity) return;
        sanity.SetPhaseIndex(index, keepRelativeOnChange);
    }
    public void SetPhaseByName(string id)
    {
        if (!sanity) return;
        sanity.SetPhaseId(id, keepRelativeOnChange);
    }
    public void NextPhase()
    {
        if (!sanity || sanity.phaseProfile == null) return;
        int next = Mathf.Clamp(sanity.PhaseIndex + 1, 0, sanity.phaseProfile.phases.Length - 1);
        sanity.SetPhaseIndex(next, keepRelativeOnChange);
    }
    public void PrevPhase()
    {
        if (!sanity) return;
        int prev = Mathf.Max(0, sanity.PhaseIndex - 1);
        sanity.SetPhaseIndex(prev, keepRelativeOnChange);
    }

}
