using UnityEngine;

public class ChessInteraction : InteractableBase
{
    private LayerMask pieceMask;
    private LayerMask boardMask;

    void Start()
    {
        itemShouldBeCameraLocked = true;
        pieceMask = StringLiterals.CHESS_PIECE_LAYER;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 20f))
            {
                Debug.Log(hit.collider.name);
            }
        }
    }
}
