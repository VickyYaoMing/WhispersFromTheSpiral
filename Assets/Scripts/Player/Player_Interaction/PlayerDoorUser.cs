using UnityEngine;


public class PlayerDoorUser : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        // Can change the key if conflict with anything
        if (Input.GetKeyDown(KeyCode.E))
        {
            SmartDoorInteraction door = other.GetComponent<SmartDoorInteraction>();
            if (door != null)
            {
                door.TryOpen(transform, false);
            }
        }
    }
}