using SanitySystem;
using UnityEngine;

public class SanityPhaseTestKeys : MonoBehaviour
{
    public Sanity _sanity;

    private void Update()
    {
        if (!_sanity) return;
        if (Input.GetKeyDown(KeyCode.I)) _sanity.SetPhaseId("s_Phase0", true);
        if (Input.GetKeyDown(KeyCode.O)) _sanity.SetPhaseId("s_Phase1", true);
        if (Input.GetKeyDown(KeyCode.P)) _sanity.SetPhaseId("s_Phase2", true);
    }
}
