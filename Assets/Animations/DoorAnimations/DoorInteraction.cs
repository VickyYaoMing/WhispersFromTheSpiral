using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == StringLiterals.PLAYER_TAG)
        {
            Vector3 doorToPlayer = other.transform.position - transform.position;
            float dot = Vector3.Dot(transform.forward, doorToPlayer);

            if (dot > 0)
            {
                animator.SetTrigger("OpenDoorPos");
            }
            else
            {
                animator.SetTrigger("OpenDoorNeg");
            }
        }
    }

}
