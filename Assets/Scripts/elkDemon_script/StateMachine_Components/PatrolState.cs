using UnityEngine;

public class PatrolState : StateMachineBehaviour
{
    private ElkDemonAI elkDemon;
    private Transform[] patrolRoutes;
    private int currentPatrolIndex; 

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(elkDemon == null)
        {
            elkDemon = animator.GetComponent<ElkDemonAI>();
        }

        if (elkDemon == null)
        {
            Debug.LogError("Could not find ElkDemonAI component on " + animator.gameObject.name);
            return;
        }

        patrolRoutes = elkDemon.patrolPoints;
        currentPatrolIndex = 0;

        animator.SetBool("IsHunting", false);
        animator.SetFloat("Speed", elkDemon.moveSpeed); 
        //Debug.Log("Patrol Mode Activated");
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
      
        Vector3 targetPosition = patrolRoutes[currentPatrolIndex].position;
        elkDemon.MoveTowards(targetPosition, elkDemon.moveSpeed);

        if (Vector3.Distance(elkDemon.transform.position, targetPosition) < 0.5f)
        {
            currentPatrolIndex = Random.Range(0, patrolRoutes.Length);
        }

        if (elkDemon.canSeePlayer())
        {
            animator.SetTrigger("PlayerSpotted");
        }
    }
}
