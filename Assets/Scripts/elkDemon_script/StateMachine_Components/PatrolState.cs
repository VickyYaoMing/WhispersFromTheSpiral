using UnityEngine;
using UnityEngine.AI;

public class PatrolState : StateMachineBehaviour
{
    private ElkDemonAI _elkDemon;
    private NavMeshAgent _agent;
    private float _timeAtCurrentDestination;
    private Vector3 _currentDestination;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(_elkDemon == null)
        {
            _elkDemon = animator.GetComponent<ElkDemonAI>();
        }

        _agent = _elkDemon.GetComponent<NavMeshAgent>();
        _timeAtCurrentDestination = 0f;

        _currentDestination = GetRandomPatrolPoint();
        _elkDemon.MoveTowards(_currentDestination, _elkDemon.MoveSpeed);

        Debug.Log("Entered patrol state - Moving to: " + _currentDestination);

        animator.SetBool("IsHunting", false);
        animator.SetFloat("Speed", _elkDemon.MoveSpeed);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_elkDemon == null || _agent == null) return;
        
        _timeAtCurrentDestination += Time.deltaTime;

        if (_elkDemon.CanSeePlayer())
        {
            animator.SetTrigger("PlayerSpotted");
            return;
        }

        if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
        {
            _currentDestination = GetRandomPatrolPoint();
            _elkDemon.MoveTowards(_currentDestination, _elkDemon.MoveSpeed);
            _timeAtCurrentDestination = 0f;
        }
        else if (_timeAtCurrentDestination > 10f) 
        {
            _currentDestination = GetRandomPatrolPoint();
            _elkDemon.MoveTowards(_currentDestination, _elkDemon.MoveSpeed);
            _timeAtCurrentDestination = 0f;
        }

        Debug.DrawLine(_elkDemon.transform.position, _agent.destination, Color.green);
    }

    private Vector3 GetRandomPatrolPoint()
    {
        if (_elkDemon.PatrolPoints != null && _elkDemon.PatrolPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, _elkDemon.PatrolPoints.Length);
            return _elkDemon.PatrolPoints[randomIndex].position;
        }
        
        return GetCenteredRandomNavMeshPoint(40f);
    }

    private Vector3 GetCenteredRandomNavMeshPoint(float radius)
    {
        for (int i = 0; i < 30; i++) 
        {
            Vector3 randomDir = Random.insideUnitSphere * radius;
            randomDir += _elkDemon.transform.position;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDir, out hit, radius, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        return _elkDemon.transform.position;
    }
    
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger("PlayerSpotted");
    }
}