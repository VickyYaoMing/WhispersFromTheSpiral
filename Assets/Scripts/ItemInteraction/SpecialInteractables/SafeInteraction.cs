using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class SafeInteraction : InteractableBase
{
    [SerializeField] private List<GameObject> anchors;
    [SerializeField] private Animator animator;
    [SerializeField] private LayerMask keypadMask;
    [SerializeField] private int[] correctCode;
    [SerializeField] private GameObject returnGameObject;
    [SerializeField] private LayerMask interactMask;
    [SerializeField] private float rayHitDistance = 100f;
    private bool safeOpened = false;
    private InteractionManager interactionManager;

    private int[] code;
    private int currentCodeIndex = 0;

    private void Start()
    {
        interactionManager = GameManager.Instance.InteractionManager;
        itemShouldBeCameraLocked = true;
        code = new int[4];
        returnGameObject.SetActive(false);
    }
   
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !safeOpened)
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, rayHitDistance, keypadMask))
            {
                GameObject hitGO = hit.collider.gameObject;

                for (int i = 0; i < anchors.Count; i++)
                {
                    if (anchors[i] == hitGO)
                    {
                        if (hitGO.name == "Minus")
                        {
                            animator.SetTrigger("Press" + hitGO.name);
                            ResetCode();

                        }
                        else if (hitGO.name == "Plus")
                        {
                            animator.SetTrigger("Press" + hitGO.name);
                            EnterCode();
                        }
                        else
                        {
                            animator.SetTrigger("Press" + hitGO.name.Replace("No", ""));
                            code[currentCodeIndex] = int.Parse(hitGO.name.Replace("No", ""));
                            currentCodeIndex++;
                            if (currentCodeIndex >= code.Length) currentCodeIndex = 0;
                        }
                    }
                }

            }
        }
        else if (Input.GetMouseButtonDown(0) && safeOpened)
        {
            Ray rayItem = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitItem;


            if (Physics.Raycast(rayItem, out hitItem, rayHitDistance, interactMask))
            {
                if (hitItem.collider.gameObject.CompareTag("Summoning_Puzzle"))
                {
                    returnGameObject.SetActive(false);
                    interactionManager.OnPickUp(returnGameObject);
                    interactionManager.ReleaseCameraLock();
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
            safeOpened = true;
            returnGameObject.SetActive(true);
        }
        else
        {
            ResetCode();
        }
        
    }
}
