using UnityEngine;


public class HuntState : StateMachineBehaviour
{
    private ElkDemonAI _elkDemon;
    private float _timeSinceLastSeen;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_elkDemon == null)
        {
            _elkDemon = animator.GetComponent<ElkDemonAI>();
        }

        _timeSinceLastSeen = 0f;

        animator.SetBool("IsHunting", true);
        animator.SetFloat("Speed", _elkDemon.HuntSpeed); 
        Debug.Log("Entered HUNT state!");
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_elkDemon == null) return;

        _elkDemon.CheckForAttack(animator);

        if (_elkDemon.CanSeePlayer())
        {
            // Hunt them directly!
            _timeSinceLastSeen = 0f;
            _elkDemon.MoveTowards(_elkDemon.Player.position, _elkDemon.HuntSpeed);
        }
        else
        {
            _timeSinceLastSeen += Time.deltaTime;

            // 1 second memory
            if (_elkDemon.HasRecentPlayerInfo && _timeSinceLastSeen < 1f) 
            {
                // Move toward player's last known position
                // AND continue in the direction they were moving
                // Predict 10 units ahead
                Vector3 predictedPosition = _elkDemon.PlayerLastKnownPosition + (_elkDemon.PlayerLastKnownDirection * 10f); 

                Debug.DrawLine(_elkDemon.transform.position, predictedPosition, Color.yellow);
                _elkDemon.MoveTowards(predictedPosition, _elkDemon.HuntSpeed * 0.9f); 
            }
            else
            {
                animator.SetTrigger("LostSight");
                animator.SetBool("IsHunting", false);
            }
        }
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("IsHunting", false);
    }
}
