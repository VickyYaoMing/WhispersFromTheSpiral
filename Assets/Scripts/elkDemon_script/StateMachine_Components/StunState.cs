using UnityEngine;

public class StunState : StateMachineBehaviour
{
    private ElkDemonAI _elkDemon;
    private bool isStun = false;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_elkDemon == null)
        {
            _elkDemon = animator.GetComponent<ElkDemonAI>();
        }

        _elkDemon.StopMoving();

        isStun = true;
        animator.SetBool("IsStun", true);
        Debug.Log("Entered Stun state!");
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_elkDemon == null || _elkDemon.Player == null) return;

        
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        Debug.Log("Exited Stun State");
    }
}
