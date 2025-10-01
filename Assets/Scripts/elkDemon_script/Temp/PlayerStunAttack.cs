using UnityEngine;

public class PlayerStunAttack : MonoBehaviour
{
    [Header("Stun Setting ")]
    [SerializeField] private float stunRange = 10f;
    [SerializeField] private float stunCd = 2f;
    [SerializeField] private LayerMask enemy;

    private KeyCode stunKey = KeyCode.G;
    private float _lastStunTimer = 0;

    private void Update()
    {
        if (Input.GetKeyDown(stunKey) && Time.time > _lastStunTimer + stunCd)
        {
            TryStunEnemy();
            _lastStunTimer = Time.time;
        }
    }
    private void TryStunEnemy()
    {
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 1f;

        Debug.DrawRay(rayStart, transform.forward * stunRange, Color.blue, 1f);

        if (Physics.Raycast(rayStart, transform.forward,out hit, stunRange, enemy))
        {
            ElkDemonAI elkDemon = hit.collider.GetComponent<ElkDemonAI>();
            if (elkDemon != null)
            {
                elkDemon.GetStunned();
                Debug.Log("Stun Hit: " + hit.collider.name);
            }
        }
    }
}
