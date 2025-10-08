using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using UnityEngine;

public class SafeInteraction : InteractableBase
{
    [SerializeField] private List<GameObject> anchors;
    [SerializeField] private Animator animator;
    [SerializeField] private LayerMask keypadMask;
    private int[] correctCode = new int[] { 1, 9, 6, 9 };

    private int[] code;
    private int currentCodeIndex = 0;

    private void Start()
    {
        itemShouldBeCameraLocked = true;
        code = new int[4];
    }
   
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 20f, keypadMask))
            {
                GameObject hitGO = hit.collider.gameObject;
                
                for(int i = 0; i < anchors.Count; i++)
                {
                    if(anchors[i] == hitGO)
                    {
                        if (hitGO.name == "Minus" )
                        {
                            animator.SetTrigger("Press" + hitGO.name);
                            ResetCode();

                        }
                        else if(hitGO.name == "Plus")
                        {
                            animator.SetTrigger("Press" + hitGO.name);
                            EnterCode();
                        }
                        else
                        {
                            animator.SetTrigger("Press" + hitGO.name.Replace("No", ""));
                            code[currentCodeIndex] = int.Parse(hitGO.name.Replace("No", ""));
                            currentCodeIndex++;
                            if(currentCodeIndex >= code.Length) currentCodeIndex = 0;
                        }
                    }
                }

            }

        }
    }

    private void ResetCode()
    {
        currentCodeIndex = 0;
    }

    private void EnterCode()
    {
        if (code.SequenceEqual(correctCode))
        {
            animator.SetBool("IsNumActive", false);
            animator.SetTrigger("OpenSafe");
        }
        else
        {
            ResetCode();
        }
        
    }
}
