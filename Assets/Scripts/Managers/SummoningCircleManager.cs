using System.Collections;
using UnityEngine;

public class SummoningCircleManager : MonoBehaviour
{
    ItemPedestal[] pedestals;
    int numberOfCorrectItems;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pedestals = GetComponentsInChildren<ItemPedestal>();
    }

    // Update is called once per frame
    void Update()
    {
        numberOfCorrectItems = 0;
        foreach(ItemPedestal pedestal in pedestals)
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
        //do something once the puzzle is complete ig

    }

}
