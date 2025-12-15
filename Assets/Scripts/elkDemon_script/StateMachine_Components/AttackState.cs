using UnityEngine;
using SanitySystem;

public class AttackState : StateMachineBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackCD = 2f;
    [SerializeField] private float attackWindupTime = 0.5f;
    [SerializeField] private float sanityDmg = 0.5f;

    private ElkDemonAI _elkDemon;
    private float _coolDownTimer;
    private float _windupTimer;
    private bool _hasAttacked;
    private bool _attackCompleted;
    private SanityEffectOnPlayer _playerSanityEffect;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_elkDemon == null)
        {
            _elkDemon = animator.GetComponent<ElkDemonAI>();
        }

        if (_playerSanityEffect == null && _elkDemon != null && _elkDemon.Player != null)
        {
            _playerSanityEffect = _elkDemon.Player.GetComponentInParent<SanityEffectOnPlayer>();
        }

        if (animator.GetBool("IsStun"))
        {
            animator.ResetTrigger("Attack");
            return;
        }

        _elkDemon.StopMoving();
        _hasAttacked = false;
        _attackCompleted = false;
        _coolDownTimer = 0f;
        _windupTimer = 0f;

        animator.SetBool("IsAttacking", true);
        Debug.Log("Entered Attack state!");
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_elkDemon == null) return;

        _windupTimer += Time.deltaTime;
        _coolDownTimer += Time.deltaTime;

        if (_elkDemon.CanSeePlayer())
        {
            Vector3 lookDirection = new Vector3(_elkDemon.Player.position.x, _elkDemon.transform.position.y, _elkDemon.Player.position.z);
            _elkDemon.transform.LookAt(lookDirection);
        }

        if (!_hasAttacked && _windupTimer >= attackWindupTime)
        {
            PerformAttack();
            _hasAttacked = true;
        }

        if (_hasAttacked && !_attackCompleted && _coolDownTimer >= attackWindupTime + 0.2f)
        {
            _attackCompleted = true;
            animator.SetTrigger("AttackComplete");
        }

        CheckExitConditions(animator);
        DebugAttackInfo();
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("IsAttacking", false);
        ResetTriggers(animator);
        Debug.Log("Exited Attack State");
    }

    private void CheckExitConditions(Animator animator)
    {
        if (_elkDemon.Player == null)
        {
            animator.SetTrigger("LostSight");
            return;
        }

        float distanceToPlayer = Vector3.Distance(_elkDemon.transform.position, _elkDemon.Player.position);

        if (distanceToPlayer > _elkDemon.AttackRange * 1.5f)
        {
            animator.SetTrigger("PlayerOutOfRange");
            return;
        }

        if (!_elkDemon.CanSeePlayer())
        {
            animator.SetTrigger("LostSight");
            return;
        }

    }

    private void PerformAttack()
    {
        Debug.Log("Elk Demon Attacks!");
        Vector3 directionToPlayer = (_elkDemon.Player.position - _elkDemon.transform.position).normalized;
        float dotProduct = Vector3.Dot(_elkDemon.transform.forward, directionToPlayer);

        if (dotProduct > _elkDemon.AttackAngleThreshold)
        {
            float distance = Vector3.Distance(_elkDemon.transform.position, _elkDemon.Player.position);

            if (distance < _elkDemon.AttackRange)
            {
                Debug.Log("Player got hit by Elk Demon's Attack!");
                var sanity = _elkDemon.Player.GetComponentInParent<SanitySystem.Sanity>();
                if (sanity != null)
                {
                    sanity.ApplyImpulse(sanityDmg);
                    Debug.Log($"Sanity reduced by {sanityDmg}. New sanity: {sanity.Sanity01}");

                    if (sanity.Sanity01 <= 0.4f)
                    {
                        Debug.Log("Death is Running!");
                        if (_playerSanityEffect != null)
                        {
                            _playerSanityEffect.ZeroSanityDeath();
                        }
                    }
                }
                else
                {
                    Debug.Log("No sanity was found so I'm Schizo as hell");
                }
            }
        }
    }

    private void ResetTriggers(Animator animator)
    {
        animator.ResetTrigger("AttackComplete");
        animator.ResetTrigger("PlayerOutOfRange");
        animator.ResetTrigger("LostSight");
        animator.ResetTrigger("Attack"); 
    }

    void DebugAttackInfo()
    {
        if (_elkDemon.Player == null) return;

        float distance = Vector3.Distance(_elkDemon.transform.position, _elkDemon.Player.position);
        Vector3 direction = (_elkDemon.Player.position - _elkDemon.transform.position).normalized;
        float dot = Vector3.Dot(_elkDemon.transform.forward, direction);

        Debug.Log($"Attack Info - Distance: {distance}, Dot: {dot}, Windup: {_windupTimer:F2}, CD: {_coolDownTimer:F2}, HasAttacked: {_hasAttacked}");
    }
}