using UnityEngine;

public class StunState : StateMachineBehaviour
{
    [Header("Stun Settings")]
    [SerializeField] private float stunDuration = 3f;
    [SerializeField] private float stunTimer = 0f;

    private bool _isStun;
    private ElkDemonAI _elkDemon;
    public bool IsStun { get { return _isStun; } }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_elkDemon == null)
        {
            _elkDemon = animator.GetComponent<ElkDemonAI>();
        }

        animator.ResetTrigger("Attack");
        animator.SetBool("IsAttacking", false);

        _elkDemon.StopMoving();
        stunTimer = 0f;

        _isStun = true;
        animator.SetBool("IsStun", true);
        Debug.Log("Entered Stun state!");
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_elkDemon == null || _elkDemon.Player == null) return;

        stunTimer += Time.deltaTime;

        if (stunTimer >= stunDuration)
        {
            animator.SetTrigger("StunComplete");
            animator.SetBool("IsStun", false);
            stunTimer = 0f;
        }

        
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("IsStun", false);
        animator.ResetTrigger("StunComplete");
        Debug.Log("Exited Stun State");
    }
}
