using UnityEngine;

public class StunState : StateMachineBehaviour
{
    private ElkDemonAI elkDemon;
    private bool isStun = false;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (elkDemon == null)
        {
            elkDemon = animator.GetComponent<ElkDemonAI>();
        }

        elkDemon.StopMoving();

        isStun = true;
        animator.SetBool("IsStun", true);
        Debug.Log("Entered Stun state!");
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (elkDemon == null || elkDemon.player == null) return;

        
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        Debug.Log("Exited Stun State");
    }
}
