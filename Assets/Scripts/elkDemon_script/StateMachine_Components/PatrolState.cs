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

        Vector3 wanderTarget = GetRandomNavMeshPoint(10f);
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

        if (!_elkDemon.GetComponent<NavMeshAgent>().pathPending && _elkDemon.GetComponent<NavMeshAgent>().remainingDistance < 0.5f)
        {
            //_currentPatrolIndex = Random.Range(0, _patrolRoutes.Length);
            Vector3 newTarget = GetRandomNavMeshPoint(10f);
            _elkDemon.MoveTowards(newTarget, _elkDemon.MoveSpeed);
        }

        if (_elkDemon.CanSeePlayer())
        {
            animator.SetTrigger("PlayerSpotted");
        }
    }


    private Vector3 GetRandomNavMeshPoint(float radius)
    {
        Vector3 randomDir = Random.insideUnitSphere * radius;
        randomDir += _elkDemon.transform.position;

        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, radius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return _elkDemon.transform.position; 
    }
}
