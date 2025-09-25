using UnityEngine;
using UnityEngine.AI;

public class PatrolState : StateMachineBehaviour
{
    private ElkDemonAI _elkDemon;
    private Transform[] _patrolRoutes;
    private int _currentPatrolIndex;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(_elkDemon == null)
        {
            _elkDemon = animator.GetComponent<ElkDemonAI>();
        }

        if (_elkDemon == null)
        {
            Debug.LogError("Could not find ElkDemonAI component on " + animator.gameObject.name);
            return;
        }

        //_patrolRoutes = _elkDemon.PatrolPoints;
        //_currentPatrolIndex = 0;

        Vector3 wanderTarget = GetRandomNavMeshPoint(10f);
        _elkDemon.MoveTowards(wanderTarget, _elkDemon.MoveSpeed);

        animator.SetBool("IsHunting", false);
        animator.SetFloat("Speed", _elkDemon.MoveSpeed); 
        //Debug.Log("Patrol Mode Activated");
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!_elkDemon.GetComponent<NavMeshAgent>().pathPending && _elkDemon.GetComponent<NavMeshAgent>().remainingDistance < 0.5f)
        {
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
