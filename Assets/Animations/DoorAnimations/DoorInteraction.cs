using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private bool Locked = false;
    float dot;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == StringLiterals.PLAYER_TAG && Locked == false)
        {
            Vector3 doorToPlayer = other.transform.position - transform.position;
            dot = Vector3.Dot(transform.forward, doorToPlayer);

            if (dot > 0)
            {
                animator.SetTrigger("OpenDoorPos");
            }
            else
            {
                animator.SetTrigger("OpenDoorNeg");
            }
        }
        if (other.tag == StringLiterals.KEYSTARTROOM_TAG)
        {

            Locked = false;

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

