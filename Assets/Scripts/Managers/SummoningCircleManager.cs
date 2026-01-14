using System.Collections;
using UnityEngine;
using SanitySystem;
using System.Linq;
using System.Collections.Generic;


public class SummoningCircleManager : MonoBehaviour
{
    [SerializeField] GameObject doorToOpen;
    ItemPedestal[] pedestals;
    private GamePhaseManager gamePhaseManager;
    private StairBlockEnabler stairBlockEnabler;
    int numberOfCorrectItems;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pedestals = GetComponentsInChildren<ItemPedestal>();
        gamePhaseManager = FindAnyObjectByType<GamePhaseManager>();
        stairBlockEnabler = FindAnyObjectByType<StairBlockEnabler>();
        GameManager.Instance.SummoningCircle = this;
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
        StartCoroutine(stairBlockEnabler.MoveBars(stairBlockEnabler.positionToMoveTo, 2f));
    }


    public void Save(ref SummoningCircleSaveData saveData)
    {
        List<PedestalSaveData> pedestalSaveDataList = new List<PedestalSaveData>();
        for (int i = 0; i < pedestals.Length; i++)
        {
            PedestalSaveData p = new PedestalSaveData
            {
                thisPedestal = pedestals[i],
                correctItem = pedestals[i].CorrectItem,
                item = pedestals[i].ItemOnPedestal,
            };
            pedestalSaveDataList.Add(p);
        }
        saveData.pedestalSaves = pedestalSaveDataList.ToArray();
        Debug.Log(pedestalSaveDataList.Count);
        foreach (var a in pedestalSaveDataList)
        {
            Debug.Log("list " + a.ToString());
        }
        foreach (var a in saveData.pedestalSaves)
        {
            Debug.Log("array" + a.ToString());
        }
    }

    public void Load(SummoningCircleSaveData saveData)
    {
        for (int i = 0; i < pedestals.Length; i++)
        {
            pedestals[i].Load(saveData.pedestalSaves[i]);
        }
    }

}
[System.Serializable]
public struct PedestalSaveData
{
    public ItemPedestal thisPedestal;
    public Default_Item correctItem;
    public Default_Item item;
}

[System.Serializable]
public struct SummoningCircleSaveData
{
    public PedestalSaveData[] pedestalSaves;
}

