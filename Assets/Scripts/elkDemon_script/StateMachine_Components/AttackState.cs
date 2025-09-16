using UnityEngine;

public class AttackState : StateMachineBehaviour
{
    private ElkDemonAI elkDemon;
    private float coolDownTimer = 0f;
    private bool hasAttacked = false;
    private bool animationFinished = false;

    [Header("Attack Settings")]
    public float attackCD = 2f;
    public float attackRange = 2f;
    public float attackAngleThreshold = 0.7f;
    public float attackWindupTime = 0.5f;
    public float attackAnimationDuration = 1.5f;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (elkDemon == null)
        {
            elkDemon = animator.GetComponent<ElkDemonAI>();
        }

        elkDemon.StopMoving();
        hasAttacked = false;
        animationFinished = false;
        coolDownTimer = 0f;

        animator.SetBool("IsAttacking", true);
        Debug.Log("Entered Attack state!");
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (elkDemon == null || elkDemon.player == null) return;

        coolDownTimer += Time.deltaTime;

        // Face the player
        if (elkDemon.canSeePlayer())
        {
            Vector3 lookDirection = new Vector3(elkDemon.player.position.x, elkDemon.transform.position.y, elkDemon.player.position.z);
            elkDemon.transform.LookAt(lookDirection);
        }

        if (!hasAttacked && coolDownTimer >= attackWindupTime)
        {
            PerformAttack();
            hasAttacked = true;
        }

        CheckExitConditions(animator);
        DebugAttackInfo();
    }

    private void CheckExitConditions(Animator animator)
    {
        if (elkDemon.player == null) return;

        float distanceToPlayer = Vector3.Distance(elkDemon.transform.position, elkDemon.player.position);

        if (distanceToPlayer > attackRange * 1.5f)
        {
            animator.SetTrigger("PlayerOutOfRange");
            return;
        }

        if (!elkDemon.canSeePlayer())
        {
            animator.SetTrigger("LostSight");
            return;
        }

        if (coolDownTimer >= attackCD)
        {
            animator.SetTrigger("AttackComplete");
            coolDownTimer = 0;
        }
    }


    private void PerformAttack()
    {
        Debug.Log("Elk Demon Attacks!");

        Vector3 directionToPlayer = (elkDemon.player.position - elkDemon.transform.position).normalized;
        float dotProduct = Vector3.Dot(elkDemon.transform.forward, directionToPlayer);

        if (dotProduct > attackAngleThreshold)
        {
            float distance = Vector3.Distance(elkDemon.transform.position, elkDemon.player.position);
            if (distance < attackRange)
            {
                Debug.Log("Player got hit by Mario's Attack!");
            }
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("IsAttacking", false);
        ResetTriggers(animator);
        Debug.Log("Exited Attack State");
    }

    private void ResetTriggers(Animator animator)
    {
        animator.ResetTrigger("AttackComplete");
        animator.ResetTrigger("PlayerOutOfRange");
        animator.ResetTrigger("LostSight");
    }

    void DebugAttackInfo()
    {
        if (elkDemon.player == null) return;

        float distance = Vector3.Distance(elkDemon.transform.position, elkDemon.player.position);
        Vector3 direction = (elkDemon.player.position - elkDemon.transform.position).normalized;
        float dot = Vector3.Dot(elkDemon.transform.forward, direction);

        Debug.Log($"Attack Info - Distance: {distance}, Dot: {dot}, CanSee: {elkDemon.canSeePlayer()}");
    }
}