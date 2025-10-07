using UnityEngine;
using SanitySystem;

public class AttackState : StateMachineBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 2f;     
    [SerializeField] private float attackWindupTime = 0.5f; 
    [SerializeField] private float attackDuration = 1.2f;   
    [SerializeField] private float timer = 0f;
    [SerializeField] private float sanityDmg = 0.5f;


    private ElkDemonAI _elkDemon;
    private bool _hasAttacked;
    private SanityEffectOnPlayer _playerSanityEffect;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_elkDemon == null)
            _elkDemon = animator.GetComponent<ElkDemonAI>();

        if (_playerSanityEffect == null && _elkDemon.Player != null)
            _playerSanityEffect = _elkDemon.Player.GetComponentInParent<SanityEffectOnPlayer>();

        _elkDemon.StopMoving();

        animator.ResetTrigger("LostSight");
        animator.ResetTrigger("PlayerSpotted");
        animator.SetBool("IsAttacking", true);

        _hasAttacked = false;
        timer = 0f;

        Debug.Log("Entered ATTACK state!");
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_elkDemon == null || _elkDemon.Player == null)
            return;

        timer += Time.deltaTime;

        Vector3 direction = (_elkDemon.Player.position - _elkDemon.transform.position).normalized;
        direction.y = 0;
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction);
            _elkDemon.transform.rotation = Quaternion.Slerp(_elkDemon.transform.rotation, lookRot, Time.deltaTime * 8f);
        }

        if (!_hasAttacked && timer >= attackWindupTime)
        {
            PerformAttack(animator);
            _hasAttacked = true;
        }

        if (timer >= attackDuration + attackCooldown)
        {
            animator.SetBool("IsAttacking", false);

            if (_elkDemon.CanSeePlayer())
            {
                animator.SetTrigger("AttackComplete");
                animator.SetTrigger("PlayerSpotted"); 
            }
            else
            {
                animator.SetTrigger("AttackComplete");
                animator.SetTrigger("LostSight"); 
            }
        }
    }

    private void PerformAttack(Animator animator)
    {
        Debug.Log("Elk Demon Attacks!");

        Vector3 directionToPlayer = (_elkDemon.Player.position - _elkDemon.transform.position).normalized;
        float dotProduct = Vector3.Dot(_elkDemon.transform.forward, directionToPlayer);

        if (dotProduct > _elkDemon.AttackAngleThreshold)
        {
            float distance = Vector3.Distance(_elkDemon.transform.position, _elkDemon.Player.position);

            // Can be modify to balance the Elk demon attack range
            // Right now it seems a bit hard for the Elk demon to REALLY hit the player
            // Play Testing require!
            // Need to fix so the ATTACK sync with ANIMATION
            // Right now attack count even before animation finishes
            if (distance < _elkDemon.AttackRange)
            {
                Debug.Log("Player got hit by Mario's Attack!");

                var sanity = _elkDemon.Player.GetComponentInParent<SanitySystem.Sanity>();
                if (sanity != null)
                {
                    sanity.ApplyImpulse(sanityDmg);

                    Debug.Log($"Sanity reduced by {sanityDmg}. New sanity: {sanity.Sanity01}");

                    if (sanity.Sanity01 <= 0f)
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

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("IsAttacking", false);
        animator.ResetTrigger("AttackComplete");
        Debug.Log("Exited ATTACK state!");
    }
}
