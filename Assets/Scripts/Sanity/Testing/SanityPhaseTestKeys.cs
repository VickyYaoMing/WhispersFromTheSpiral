using SanitySystem;
using UnityEngine;

public class SanityPhaseTestKeys : MonoBehaviour
{
    public Sanity _sanity;

    private void Update()
    {
        if (!_sanity) return;
        if (Input.GetKeyDown(KeyCode.I)) _sanity.SetPhaseIndex(0, true);
        if (Input.GetKeyDown(KeyCode.O)) _sanity.SetPhaseIndex(1, true);
        if (Input.GetKeyDown(KeyCode.P)) _sanity.SetPhaseIndex(2, true);
    }
}
