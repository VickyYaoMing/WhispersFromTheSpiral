using SanitySystem;
using UnityEngine;

public class SanityTestKeys : MonoBehaviour
{
    public Sanity _sanity;
    void Update() //For testing 
    {
        if (!_sanity) return;
        if (Input.GetKeyDown(KeyCode.J)) _sanity.ApplyImpulse(-0.10f); // small scare
        if (Input.GetKeyDown(KeyCode.K)) _sanity.ApplyImpulse(-0.30f); // big scare
        if (Input.GetKeyDown(KeyCode.L)) _sanity.ApplyImpulse(+0.20f); // regain composure

        Debug.Log("SanityTestKeys: Current sanity is " + _sanity.name);
    }
}
