using UnityEngine;

public class StressImpulseOnEvent : MonoBehaviour
{
    StressController _stress;
    public float _delta;
    public string _tag = "Player";
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == _tag)
        {
            _stress = GetComponent<StressController>();
            _stress.ApplyImpulse(_delta);
        }
    }
}
