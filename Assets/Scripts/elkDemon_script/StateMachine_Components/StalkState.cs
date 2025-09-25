//using UnityEngine;

//public class StalkState : StateMachineBehaviour
//{
//    private ElkDemonAI elkDemon;
//    private float stalkTimer;
//    private float timeBetweenMovements = 8f;

//    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
//    {
//        if (elkDemon == null)
//        {
//            elkDemon = animator.GetComponent<ElkDemonAI>();
//        }

//        stalkTimer = timeBetweenMovements;

//        //animator.SetBool("IsStalking", true);
//        //animator.SetFloat("Speed", elkDemon.huntSpeed);
//        Debug.Log("Entered STALKING state!");
//    }

//    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
//    {
//        if (elkDemon == null) return;

//        // Move toward the current observation point (slower movement)
//        elkDemon.MoveTowards(elkDemon.GetObservationPoint().position, elkDemon.moveSpeed * 0.7f);

//        // Constantly update the tracking info while in STALK zone
//        // Ensure known player position/looking direction/moving direction
//        Vector3 directionToPlayer = (elkDemon.player.position - elkDemon.transform.position);
//        elkDemon.UpdatePlayerTrackingInfo(elkDemon.player.position, directionToPlayer);

//        elkDemon.transform.LookAt(elkDemon.player.position);

//        stalkTimer -= Time.deltaTime;
//        if (stalkTimer <= 0)
//        {
//            stalkTimer = timeBetweenMovements;
//            // Logic to choose new observation point could go here
//        }
//    }

//    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
//    {
//        // When leaving stalk state, we preserve the last tracking info
//        // This will be used if we need to hunt
//        //animator.SetBool("IsStalking", false);
//    }

//}
