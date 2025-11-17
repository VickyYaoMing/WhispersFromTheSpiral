using UnityEngine;
using UnityEngine.AI;

public class PatrolState : StateMachineBehaviour
{
    private ElkDemonAI _elkDemon;
    private NavMeshAgent _agent;

    private bool _isLookingAround = false;
    private float _lookAroundTimer = 0f;
    private float _lookAroundDuration = 16f;


    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(_elkDemon == null)
        {
            _elkDemon = animator.GetComponent<ElkDemonAI>();
        }

        _agent = _elkDemon.GetComponent<NavMeshAgent>();

        Vector3 wanderTarget = GetRandomNavMeshPoint(100f);
        _elkDemon.MoveTowards(wanderTarget, _elkDemon.MoveSpeed);

        animator.SetBool("IsHunting", false);
        animator.SetFloat("Speed", _elkDemon.MoveSpeed);
        _isLookingAround = false;
        _lookAroundTimer = 0f;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!_agent.pathPending && _agent.remainingDistance < 1f)
        {
            if (!_isLookingAround)
            {
                _elkDemon.StopMoving();

                animator.SetTrigger("LookAround");
                _isLookingAround = true;
                _lookAroundTimer = 0f;
            }
            else
            {
                _lookAroundTimer += Time.deltaTime;
                if (_lookAroundTimer >= _lookAroundDuration)
                {
                    Vector3 newTarget = GetRandomNavMeshPoint(100f);
                    _elkDemon.MoveTowards(newTarget, _elkDemon.MoveSpeed);
                    _isLookingAround = false;
                    _lookAroundTimer = 0f;
                }
            }
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
