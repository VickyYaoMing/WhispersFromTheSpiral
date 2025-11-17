using UnityEngine;

public class gunPickUp : MonoBehaviour
{
    //literly only a setActive for now,
    //check if intergrade scroll thingy should be applied//check other items and tags
    [SerializeField] GameObject realGun;
    [SerializeField] GameObject fakeGun;
    [SerializeField] private LayerMask gunLayer;

    [SerializeField] Animator animator;


    void Start()
    {
        realGun.SetActive(false);
        fakeGun.SetActive(true);
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, gunLayer))
            {
                GameObject go = hit.collider.gameObject;

                if (go == fakeGun)
                {
                    OnClick();
                }

            }

        }


    }
    void OnClick()
    {
        Debug.Log(" in OnClick method");

        if (animator.GetBool("winningCondition") == true)
        {
            Debug.Log("clicked on gun");
            realGun.SetActive(true);
            fakeGun.SetActive(false);
        }
        else
        {
            Debug.Log("Cant pick this up yet");
            return;
        }




    }

}
