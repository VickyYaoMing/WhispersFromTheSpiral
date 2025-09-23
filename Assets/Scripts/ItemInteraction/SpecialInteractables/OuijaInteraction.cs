using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;

public class OuijaInteraction : InteractableBase
{
    [SerializeField] private List<GameObject> anchors;
    private PlanchetteInteraction planchette;
    private KeyCode[] allKeys;
    [SerializeField] private TMP_Text inputText;
    private string questionText = string.Empty;
    void Start()
    {
        itemShouldBeCameraLocked = true;
        planchette = GetComponentInChildren<PlanchetteInteraction>();
        allKeys = (KeyCode[])System.Enum.GetValues(typeof(KeyCode));
        this.enabled = false;
    }

    public override GameObject PickedUp()
    {
        return base.PickedUp();
    }

    public void Update()
    {

        for (int i = 0; i < allKeys.Length; i++)
        {
            if (Input.GetKeyDown(allKeys[i]))
            {
                if (allKeys[i] >= KeyCode.A && allKeys[i] <= KeyCode.Z)
                {
                    questionText += allKeys[i];
                    inputText.text = questionText;
                }
                if(allKeys[i] == KeyCode.Space)
                {
                    questionText += " ";
                    inputText.text = questionText;
                }
                if (allKeys[i] == KeyCode.Backspace && questionText.Length > 0)
                {
                    string newText = questionText.Substring(0, questionText.Length - 1);
                    questionText = newText;
                    inputText.text = questionText;
                }
                if(questionText.Length > 0 && allKeys[i] == KeyCode.Return)
                {
                    questionText = string.Empty;
                    inputText.text = questionText;
                    Answer();
                }
            }
        }
    }

    private void Answer()
    {
        StartCoroutine(SpellWord("Larry"));
    }

    private IEnumerator SpellWord(string word)
    {
        word = word.ToUpperInvariant();

        for (int i = 0; i < word.Length; i++)
        {
            string letter = word[i].ToString();

            int j = -1;
            for (int k = 0; k < anchors.Count; k++)
            {
                if (anchors[k].name == letter)
                {
                    j = k;
                    break;
                }
            }
            if (j < 0) continue; 

            
            planchette.MoveToTarget(anchors[j].transform);
            yield return new WaitUntil(() => planchette.reachedTarget);

            Debug.Log("Reached target " + anchors[j].name);

            yield return new WaitForSeconds(0.8f);
        }
    }



    public void OnDisable()
    {
        inputText.text = string.Empty;
        questionText = string.Empty;
        inputText.enabled = false;
    }

    public void OnEnable()
    {
        inputText.enabled= true;
    }


}
