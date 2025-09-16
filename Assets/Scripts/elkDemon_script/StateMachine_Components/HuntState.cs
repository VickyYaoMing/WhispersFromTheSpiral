using UnityEngine;


public class HuntState : StateMachineBehaviour
{
    private ElkDemonAI elkDemon;
    private float timeSinceLastSeen;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (elkDemon == null)
        {
            elkDemon = animator.GetComponent<ElkDemonAI>();
        }

        timeSinceLastSeen = 0f;

        animator.SetBool("IsHunting", true);
        animator.SetFloat("Speed", elkDemon.huntSpeed); 
        Debug.Log("Entered HUNT state!");
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (elkDemon == null) return;

        elkDemon.CheckForAttack(animator);

        if (elkDemon.canSeePlayer())
        {
            // Hunt them directly!
            timeSinceLastSeen = 0f;
            elkDemon.MoveTowards(elkDemon.player.position, elkDemon.huntSpeed);
        }
        else
        {
            timeSinceLastSeen += Time.deltaTime;

            // 5 second memory
            if (elkDemon.HasRecentPlayerInfo && timeSinceLastSeen < 5f) 
            {
                // Move toward player's last known position
                // AND continue in the direction they were moving
                // Predict 3 units ahead
                Vector3 predictedPosition = elkDemon.PlayerLastKnownPosition + (elkDemon.PlayerLastKnownDirection * 3f); 

                Debug.DrawLine(elkDemon.transform.position, predictedPosition, Color.yellow);
                elkDemon.MoveTowards(predictedPosition, elkDemon.huntSpeed * 0.8f); 
            }
            else
            {
                animator.SetTrigger("LostSight");
            }
        }
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("IsHunting", false);
    }
}
