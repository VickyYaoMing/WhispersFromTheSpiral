using SanitySystem;
using UnityEngine;

public class SanityTestKeys : MonoBehaviour
{
    public Sanity sanity;
    void Update()
    {
        if (!sanity) return;
        if (Input.GetKeyDown(KeyCode.J)) sanity.ApplyImpulse(-0.10f); // small scare
        if (Input.GetKeyDown(KeyCode.K)) sanity.ApplyImpulse(-0.30f); // big scare
        if (Input.GetKeyDown(KeyCode.L)) sanity.ApplyImpulse(+0.20f); // regain composure
    }
}
