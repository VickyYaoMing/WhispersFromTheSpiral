using UnityEngine;

public class HuntState : StateMachineBehaviour
{
    private ElkDemonAI _elkDemon;
    private float _timeSinceLastSeen;
    private float _memoryDuration = 3f;

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

        if (_elkDemon.IsGrabbingPlayer)
            return;

        _elkDemon.CheckForAttack(animator);

        if (_elkDemon.CanSeePlayer())
        {
            _timeSinceLastSeen = 0f;
            _elkDemon.MoveTowards(_elkDemon.Player.position, _elkDemon.HuntSpeed);

            Debug.DrawLine(_elkDemon.transform.position, _elkDemon.Player.position, Color.red);
        }
        else if (_elkDemon.HasRecentPlayerInfo && _timeSinceLastSeen < _memoryDuration)
        {
            _timeSinceLastSeen += Time.deltaTime;

            float predictionDistance = 5f * (_memoryDuration - _timeSinceLastSeen) / _memoryDuration;
            Vector3 predictedPosition = _elkDemon.PlayerLastKnownPosition +
                                      (_elkDemon.PlayerLastKnownDirection * predictionDistance);

            Debug.DrawLine(_elkDemon.transform.position, predictedPosition, Color.yellow);
            _elkDemon.MoveTowards(predictedPosition, _elkDemon.HuntSpeed * 0.8f);

            float distanceToLastKnown = Vector3.Distance(_elkDemon.transform.position, _elkDemon.PlayerLastKnownPosition);
            if (distanceToLastKnown < 2f && _timeSinceLastSeen > 1f)
            {
                animator.SetTrigger("LostSight");
            }
        }
        else
        {
            animator.SetTrigger("LostSight");
            animator.SetBool("IsHunting", false);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("IsHunting", false);
        animator.ResetTrigger("LostSight");
    }
}