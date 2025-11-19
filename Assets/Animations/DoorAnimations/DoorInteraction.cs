using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    //REWRITE THIS 
    [SerializeField] LayerMask doorLayer;
    [SerializeField] Camera cam;
    private LockedObject doorLock;
    private Transform selectedDoor;
    private GameObject dragPointGameobject;
    private int leftDoor = 0;
    
    void Update()
    {
        //Raycast
        RaycastHit hit;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 20, doorLayer))
        {
            if (Input.GetMouseButtonDown(0))
            {
                selectedDoor = hit.collider.gameObject.transform;
                
                if (selectedDoor.parent.GetComponentInParent<LockedObject>() != null)
                {
                    doorLock = selectedDoor.parent.GetComponentInParent<LockedObject>();
                    Debug.Log(doorLock);
                }

                //If door is locked, just stop running
                if (doorLock != null && doorLock.Locked)
                {
                    doorLock.SecondaryInteraction();
                }
            }
        }

        if (selectedDoor != null)
        {
            HingeJoint joint = selectedDoor.GetComponent<HingeJoint>();
            JointMotor motor = joint.motor;

            if (doorLock && doorLock.Locked)
            {
                return;
            }


            //Create drag point object for reference where players mouse is pointing
            if (dragPointGameobject == null)
            {
                dragPointGameobject = new GameObject("Ray door");
                dragPointGameobject.transform.parent = selectedDoor;
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            dragPointGameobject.transform.position = ray.GetPoint(Vector3.Distance(selectedDoor.position, transform.position));
            dragPointGameobject.transform.rotation = selectedDoor.rotation;


            float delta = Mathf.Pow(Vector3.Distance(dragPointGameobject.transform.position, selectedDoor.position), 3);

            // Deciding if it is left or right door
            if (selectedDoor.GetComponent<MeshRenderer>().localBounds.center.x > selectedDoor.localPosition.x)
            {
                leftDoor = -1;
            }
            else
            {
                leftDoor = 1;
            }

            // Applying velocity to door motor
            float speedMultiplier = 60000;

            if (Mathf.Abs(selectedDoor.parent.forward.z) > 0.5f)
            {
                if (dragPointGameobject.transform.position.x > selectedDoor.position.x)
                {
                    motor.targetVelocity = delta * speedMultiplier * Time.deltaTime * leftDoor;
                }
                else
                {
                    motor.targetVelocity = delta * -speedMultiplier * Time.deltaTime * leftDoor;
                }
            }
            else if (Mathf.Abs(selectedDoor.parent.forward.x) > 0.5f)
            {
                if (dragPointGameobject.transform.position.z > selectedDoor.position.z)
                {
                    motor.targetVelocity = delta * speedMultiplier * Time.deltaTime * leftDoor;
                }
                else
                {
                    motor.targetVelocity = delta * -speedMultiplier * Time.deltaTime * leftDoor;
                }
            }

            joint.motor = motor;

            if (Input.GetMouseButtonUp(0))
            {
                selectedDoor = null;
                motor.targetVelocity = 0;
                joint.motor = motor;
                Destroy(dragPointGameobject);
            }
        }
    }
}

