using System;
using UnityEngine;

public class GrandFatherTrigger : MonoBehaviour
{
    Animator animator;

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
        animator = GetComponent<Animator>();

    }

    void Update()
    {
        
    }

    private void PuzzleTrigger(object e, EventArgs args)
    {
        Debug.Log("Grandfather clock triggered");
        animator.SetBool("winningCondition", true);
    }
}
