using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;

public class OuijaInteraction : InteractableBase
{
    [SerializeField] private List<GameObject> anchors;
    [SerializeField] private TMP_Text inputText;

    private PlanchetteInteraction planchette;
    private KeyCode[] allKeys;
    private string questionText = string.Empty;
    private string sonName = "oliver";
    private string deathYear = "1969";

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
                    Answer();
                }
            }
        }
    }

    private void Answer()
    {
        if(questionText.ToLower().Contains(sonName) && questionText.ToLower().Contains("die") && questionText.ToLower().Contains("when") || questionText.ToLower().Contains("what"))
        {
            StartCoroutine(SpellWord(deathYear));
        }
        else
        {
            StartCoroutine(PlanchetteMovement(anchors[anchors.Count - 1].transform));
        }

        questionText = string.Empty;
        inputText.text = questionText;
    }

    private IEnumerator PlanchetteMovement(Transform transform)
    {
        planchette.MoveToTarget(transform);
        yield return new WaitUntil(() => planchette.reachedTarget);


        yield return new WaitForSeconds(0.9f);
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


            yield return new WaitForSeconds(0.9f);
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
