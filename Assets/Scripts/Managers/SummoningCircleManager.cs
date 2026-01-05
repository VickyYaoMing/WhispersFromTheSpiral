using System.Collections;
using UnityEngine;

public class SummoningCircleManager : MonoBehaviour
{
    [SerializeField] GameObject doorToOpen;
    ItemPedestal[] pedestals;
    private GamePhaseManager gamePhaseManager;
    int numberOfCorrectItems;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pedestals = GetComponentsInChildren<ItemPedestal>();
        gamePhaseManager = FindAnyObjectByType<GamePhaseManager>();
    }

    // Update is called once per frame
    void Update()
    {
        numberOfCorrectItems = 0;
        foreach (ItemPedestal pedestal in pedestals)
        {
            if (pedestal.IsCorrectItem)
            {
                numberOfCorrectItems++;
            }
        }
        PuzzleComplete();
    }

    public void PuzzleComplete()
    {
        if (numberOfCorrectItems != pedestals.Length) return;

        Debug.Log("Puzzle complete bruh");
        doorToOpen.SetActive(false);
        //do something once the puzzle is complete ig

        //Added to advance game phase
        gamePhaseManager.NextPhase();
    }

}
