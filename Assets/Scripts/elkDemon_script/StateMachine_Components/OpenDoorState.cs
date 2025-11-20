using System;
using UnityEngine;
using UnityEngine.AI;


public class OpenDoorState : MonoBehaviour
{
    private NavMeshAgent _navAgent;
    private bool openingDoor;

    private void Awake()
    {
        _navAgent = GetComponent<NavMeshAgent>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (openingDoor) return;

        //SmartDoorInteraction door = other.GetComponent<SmartDoorInteraction>();
        //if (door != null)
        //{
        //    openingDoor = true;
        //    _navAgent.isStopped = true;

        //    Vector3 lookPos = other.transform.position - transform.position;
        //    lookPos.y = 0;
        //    transform.rotation = Quaternion.LookRotation(lookPos);

        //    door.TryOpen(transform, true);

        //    Invoke(nameof(ResumeMovement), 2.5f);
        //}
    }

    private void ResumeMovement()
    {
        _navAgent.isStopped = false;
        openingDoor = false;
    }
}
