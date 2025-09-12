using UnityEngine;


public class HuntState : StateMachineBehaviour
{
    private ElkDemonAI elkDemon;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (elkDemon == null)
        {
            elkDemon = animator.GetComponent<ElkDemonAI>();
        }

        animator.SetBool("IsHunting", true);
        animator.SetFloat("Speed", elkDemon.huntSpeed); 
        Debug.Log("Entered HUNT state!");
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // In the HUNT state, we continuously move toward the player's CURRENT position.
        if (elkDemon != null)
        {
            elkDemon.MoveTowards(elkDemon.player.position, elkDemon.huntSpeed);
        }

        // THE CRITICAL NEW LOGIC: Check if we have LOST sight of the player.
        if (elkDemon != null && !elkDemon.canSeePlayer())
        {
            // If we can't see the player anymore, transition back to PATROL.
            animator.SetTrigger("LostSight");
        }
    }

}
