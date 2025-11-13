using UnityEngine;
using UnityEngine.AI;

public class LookAroundState : StateMachineBehaviour
{
    private ElkDemonAI _elkDemon;
    private NavMeshAgent _agent;
    private float _lookTimer;
    private float _lookDuration = 16f; 
    private float _turnSpeed = 60f;   

    private int _rotationDirection;   

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_elkDemon == null)
            _elkDemon = animator.GetComponent<ElkDemonAI>();

        if (_agent == null)
            _agent = _elkDemon.GetComponent<NavMeshAgent>();

        _elkDemon.StopMoving();
        _lookTimer = 0f;
        _rotationDirection = Random.value > 0.5f ? 1 : -1;

        animator.SetFloat("Speed", 0f);
        animator.ResetTrigger("LookAround");

        Debug.Log("Entered LOOK AROUND state");
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _lookTimer += Time.deltaTime;
     
        _elkDemon.transform.Rotate(Vector3.up, _rotationDirection * _turnSpeed * Time.deltaTime);
   
        if (_elkDemon.CanSeePlayer())
        {
            animator.SetTrigger("PlayerSpotted");
            return;
        }

        if (_lookTimer >= _lookDuration)
        {
            animator.SetTrigger("ResumePatrol");
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("Exiting LOOK AROUND state");
    }
}
