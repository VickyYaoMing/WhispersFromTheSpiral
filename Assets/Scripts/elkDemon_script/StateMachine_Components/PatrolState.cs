using UnityEngine;
using UnityEngine.AI;

public class PatrolState : StateMachineBehaviour
{
    private ElkDemonAI _elkDemon;
    private NavMeshAgent _agent;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(_elkDemon == null)
        {
            _elkDemon = animator.GetComponent<ElkDemonAI>();
        }

        _agent = _elkDemon.GetComponent<NavMeshAgent>();

        Vector3 wanderTarget = GetCenteredRandomNavMeshPoint(10f);
        _elkDemon.MoveTowards(wanderTarget, _elkDemon.MoveSpeed);

        Debug.Log("Entered patrol state");

        animator.SetBool("IsHunting", false);
        animator.SetFloat("Speed", _elkDemon.MoveSpeed);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //if (!_agent.pathPending && _agent.remainingDistance < 1f)
        //{
        //    _elkDemon.StopMoving();
        //
        //    animator.SetTrigger("Patrol");
        //
        //    animator.SetTrigger("LookAround");
        //}

        // DEBUG: Draw the path
        if (_agent.hasPath)
        {
            for (int i = 0; i < _agent.path.corners.Length - 1; i++)
            {
                Debug.DrawLine(_agent.path.corners[i], _agent.path.corners[i + 1], Color.white);
            }
        }

        // DEBUG: Show current destination
        Debug.DrawLine(_elkDemon.transform.position, _agent.destination, Color.green);

        if (!_elkDemon.GetComponent<NavMeshAgent>().pathPending && _elkDemon.GetComponent<NavMeshAgent>().remainingDistance < 0.5f)
        {
            //_currentPatrolIndex = Random.Range(0, _patrolRoutes.Length);
            Vector3 newTarget = GetCenteredRandomNavMeshPoint(30f);
            _elkDemon.MoveTowards(newTarget, _elkDemon.MoveSpeed);
        }

        if (_elkDemon.CanSeePlayer())
        {
            animator.SetTrigger("PlayerSpotted");
        }
    }


    private Vector3 GetCenteredRandomNavMeshPoint(float radius)
    {
        for (int i = 0; i < 30; i++) 
        {
            Vector3 randomDir = Random.insideUnitSphere * radius;
            randomDir += _elkDemon.transform.position;

            if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, radius, NavMesh.AllAreas))
            {
                if (hit.distance > 1.0f) 
                {
                    return hit.position;
                }
            }
        }

        return _elkDemon.transform.position;
    }
}
