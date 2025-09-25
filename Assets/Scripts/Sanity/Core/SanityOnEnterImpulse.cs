using SanitySystem;
using UnityEngine;

public class SanityOnEnterImpulse : MonoBehaviour
{
    public float _delta;
    public string _playerTag = "Player";
    void OnTriggerEnter(Collider _other)
    {
        if (!_other.CompareTag(_playerTag)) return;
        var sanity = _other.GetComponentInParent<Sanity>();
        if (sanity) sanity.ApplyImpulse(_delta);

        Debug.Log("SanityOnEnterImpulse: Applied impulse of " + _delta + " to " + _other.name);
    }
}
