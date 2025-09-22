using SanitySystem;
using UnityEngine;

public class SanityOnEnterImpulse : MonoBehaviour
{
    public float delta;
    public string playerTag = "Player";
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        var sanity = other.GetComponentInParent<Sanity>();
        if (sanity) sanity.ApplyImpulse(delta);
    }
}
