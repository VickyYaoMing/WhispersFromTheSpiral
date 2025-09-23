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
        //for (int i = 0; i < anchors.Count; i++)
        //{
        //    KeyCode key = (KeyCode)System.Enum.Parse(typeof(KeyCode), anchors[i].name);
        //    if (Input.GetKeyDown(key))
        //    {
        //        planchette.MoveToTarget(anchors[i].transform);
        //    }
        //}

        for (int i = 0; i < allKeys.Length; i++)
        {
            if (Input.GetKeyDown(allKeys[i]))
            {
                if (allKeys[i] >= KeyCode.A && allKeys[i] <= KeyCode.Z || allKeys[i] == KeyCode.Question)
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
        
        string answer = "chillin";

       //for(int i = 0; i < answer.Length; i++)
       // {
       //     for(int j = 0; j < anchors.Count; j++)
       //     {
       //         if(answer[j].ToString().ToUpper() == anchors[j].name)
       //         {
       //         }
       //     }
       // }

    }

    private IEnumerator ReachLetter(int j)
    {
        // Tell the planchette to move
        planchette.MoveToTarget(anchors[j].transform);

        // Wait until planchette.reachedTarget becomes true
        yield return new WaitUntil(() => planchette.reachedTarget);

        Debug.Log("Reached target " + anchors[j].name);
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
