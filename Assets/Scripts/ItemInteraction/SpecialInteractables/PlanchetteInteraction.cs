using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlanchetteInteraction : MonoBehaviour
{
    private float speed = 2f;
    private float stopDistance = 0.05f;
    private bool shouldMove = false;
    public bool reachedTarget { get; private set; } = false;
    private Transform targetTransform = null;
    public void MoveToTarget(Transform target)
    {
           targetTransform = target;
            shouldMove = true;
    }

    public void Update()
    {
        if (shouldMove && transform != null)
        {
            Vector3 toTarget = targetTransform.position - transform.position;
            float distance = toTarget.magnitude;
            reachedTarget = false;

            if (distance <= stopDistance)
            {
                transform.position = targetTransform.position;
                shouldMove = false;
                reachedTarget = true;
                return;
            }

            Vector3 direction = toTarget.normalized;
            float step = speed * Time.deltaTime;

            if (step > distance) step = distance;

            transform.position += direction * step;

        } 
    }
}
