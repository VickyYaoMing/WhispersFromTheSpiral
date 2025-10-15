using UnityEngine;

public class ChessInteraction : InteractableBase
{
    private LayerMask pieceMask;
    //private LayerMask boardMask;

    void Start()
    {
        itemShouldBeCameraLocked = true;
        pieceMask = LayerMask.NameToLayer(StringLiterals.CHESS_PIECE_LAYER);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("click");

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 20f, pieceMask))
            {
                Debug.Log(hit.collider.name);
            }
        }
    }
}
