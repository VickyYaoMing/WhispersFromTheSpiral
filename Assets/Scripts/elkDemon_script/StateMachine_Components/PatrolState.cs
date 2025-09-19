using UnityEngine;

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

        _patrolRoutes = _elkDemon.PatrolPoints;
        _currentPatrolIndex = 0;

        animator.SetBool("IsHunting", false);
        animator.SetFloat("Speed", _elkDemon.MoveSpeed); 
        //Debug.Log("Patrol Mode Activated");
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
      
        Vector3 targetPosition = _patrolRoutes[_currentPatrolIndex].position;
        _elkDemon.MoveTowards(targetPosition, _elkDemon.MoveSpeed);

        if (Vector3.Distance(_elkDemon.transform.position, targetPosition) < 0.5f)
        {
            _currentPatrolIndex = Random.Range(0, _patrolRoutes.Length);
        }

        if (_elkDemon.CanSeePlayer())
        {
            animator.SetTrigger("PlayerSpotted");
        }
    }
}
