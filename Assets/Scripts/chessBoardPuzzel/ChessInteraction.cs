using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChessInteraction : InteractableBase
{
    [SerializeField] private LayerMask pieceMask;
    [SerializeField] private LayerMask boardMask;

    private bool chessPieceSelected = false;
    private Vector3 currentPieceOriginalPosition;
    private GameObject currentGameObject = null;
    void Start()
    {
        itemShouldBeCameraLocked = true;
    }
    void Update()
    {
        MouseHoverChessPiece();
        SelectAndPlaceChessPiece();
    }

    private void PieceToOriginalPosition()
    {
        Vector3 toTarget = currentPieceOriginalPosition - currentGameObject.transform.position;
        float distance = toTarget.magnitude;

        if (distance <= 0.001f)
        {
            currentGameObject.transform.position = currentPieceOriginalPosition;
            chessPieceSelected = false;
            currentGameObject = null;
            return;
        }

        Vector3 direction = toTarget.normalized;
        float step = 1f * Time.deltaTime;

        if (step > distance) step = distance;

        currentGameObject.transform.position += direction * step;
    }
    private void MouseHoverChessPiece()
    {
        if (chessPieceSelected && currentGameObject != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 100f, boardMask))
            {
                float yPos = currentPieceOriginalPosition.y;
                currentGameObject.transform.position = new Vector3(hit.point.x, yPos, hit.point.z);
            }
            else
            {
                PieceToOriginalPosition();
            }
        }
    }
    private void SelectAndPlaceChessPiece()
    {
        if (Input.GetMouseButtonDown(0) && !chessPieceSelected)
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 20f, pieceMask))
            {
                Debug.Log(hit.collider.gameObject.name);
                currentGameObject = hit.collider.gameObject;
                chessPieceSelected = true;
                currentPieceOriginalPosition = currentGameObject.transform.position;
                currentGameObject.transform.SetParent(transform, true);
            }
        }
        else if (Input.GetMouseButtonDown(0) && chessPieceSelected)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Debug.DrawRay(ray.origin, ray.direction * 50f, Color.red, 100f);
            if (Physics.Raycast(ray, out hit, 20f, boardMask))
            {
                if (hit.collider.transform.childCount > 0 )
                {
                    return;
                }
                Transform hitTransform = hit.collider.transform;
                currentGameObject.transform.SetParent(hitTransform, false);
                currentGameObject.transform.position = new Vector3(hitTransform.position.x, currentPieceOriginalPosition.y, hitTransform.position.z);
                chessPieceSelected = false;
                currentGameObject = null;
            }
        }
    }
}
