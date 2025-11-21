using UnityEngine;

[RequireComponent (typeof(Rigidbody))]
public class TransitionEventAct1 : MonoBehaviour
{
    private Rigidbody m_body;
    private float m_fallPower = 10000;

    #region Unity Method
    private void Start()
    {
        m_body = GetComponent<Rigidbody>();
    }
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Bullet")) { return; }
        m_body.AddForce(transform.forward * -m_fallPower);
        Destroy(this);
    }
}
