using UnityEngine;

public class StressDebugKeys : MonoBehaviour
{
    public StressController stress;
    void Update()
    {
        if (!stress) return;
        if (Input.GetKeyDown(KeyCode.Alpha1)) stress.ApplyImpulse(0.10f);
        if (Input.GetKeyDown(KeyCode.Alpha2)) stress.ApplyImpulse(0.30f);
        if (Input.GetKeyDown(KeyCode.Alpha0)) stress.ApplyImpulse(-1f); // calm instantly
    }
}
