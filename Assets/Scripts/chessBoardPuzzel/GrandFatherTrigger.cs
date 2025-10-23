using System;
using UnityEngine;

public class GrandFatherTrigger : MonoBehaviour
{

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
        
    }

    void Update()
    {
        
    }

    private void PuzzleTrigger(object e, EventArgs args)
    {
        Debug.Log("Grandfather clock triggered");
    }
}
