using System;
using UnityEngine;

public class GrandFatherTrigger : MonoBehaviour
{
    Animator animator;
    private InteractionManager interactionManager;
    private bool hasClockBeenOpened = false;
    private bool hasGunBeenTaken = false;

    [SerializeField] private LayerMask itemMask;
    [SerializeField] private float rayHitDistance;
    [SerializeField] private GameObject returnGameObject;

    private void OnEnable()
    {
        ChessInteraction.ChessPuzzleCompleted += PuzzleTrigger;   
    }

    private void OnDisable()
    {
        ChessInteraction.ChessPuzzleCompleted -= PuzzleTrigger;

    }
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        interactionManager = GameManager.Instance.InteractionManager;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && hasClockBeenOpened && !hasGunBeenTaken)
        {
            GetComponent<BoxCollider>().enabled = false;
            Ray rayItem = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitItem;


            if (Physics.Raycast(rayItem, out hitItem, rayHitDistance, itemMask))
            {
                if (hitItem.collider.gameObject.CompareTag("ItemInsideSafe"))
                {
                    returnGameObject.SetActive(false);
                    interactionManager.OnPickUp(returnGameObject);
                    interactionManager.ReleaseCameraLock();
                    GetComponent<BoxCollider>().enabled = true;
                    hasGunBeenTaken = true;

                }
            }
        }
    }

    private void PuzzleTrigger(object e, EventArgs args)
    {
        Debug.Log("Grandfather clock triggered");
        animator.SetBool("winningCondition", true);
        hasClockBeenOpened = true;
    }
}
