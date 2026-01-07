using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ChessInteraction : InteractableBase
{
    [SerializeField] private LayerMask pieceMask;
    [SerializeField] private LayerMask boardMask;
    [SerializeField] private List<GameObject> whitePieces;
    [SerializeField] private List<GameObject> whitePiecesThatShouldBeMoved;
    [SerializeField] private List<GameObject> placesWhitePiecesShouldBeMovedTo;
    [SerializeField] private GameObject blackKing;


    private bool chessPieceSelected = false;
    private Vector3 currentPieceOriginalPosition;
    private GameObject originalParent = null;
    private GameObject currentGameObject = null;
    private Dictionary<GameObject, GameObject> whitePiecesWinningPosition;
    private Dictionary<GameObject, GameObject> whitePiecesCurrentPosition;

    public static EventHandler ChessPuzzleCompleted;


    void Start()
    {
        whitePiecesWinningPosition = new Dictionary<GameObject, GameObject>();
        whitePiecesCurrentPosition = new Dictionary<GameObject, GameObject>();
        itemShouldBeCameraLocked = true;
        for (int i = 0; i < whitePieces.Count; i++)
        {
            whitePiecesCurrentPosition[whitePieces[i]] = whitePieces[i].transform.parent.gameObject;

            for (int j = 0; j < whitePiecesThatShouldBeMoved.Count; j++)
            {
                if (whitePieces[i] == whitePiecesThatShouldBeMoved[j])
                {
                    whitePiecesWinningPosition[whitePieces[i]] = placesWhitePiecesShouldBeMovedTo[j];
                    break;
                }
                else
                {
                    whitePiecesWinningPosition[whitePieces[i]] = whitePieces[i].transform.parent.gameObject;
                }
            }

        }
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
            currentGameObject.transform.SetParent(originalParent.transform, true);
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
                currentGameObject = hit.collider.gameObject;
                chessPieceSelected = true;
                currentPieceOriginalPosition = currentGameObject.transform.position;
                originalParent = currentGameObject.transform.gameObject;
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
                if (hit.collider.transform.childCount > 0)
                {
                    return;
                }
                Transform hitTransform = hit.collider.transform;
                currentGameObject.transform.SetParent(hitTransform, false);
                currentGameObject.transform.position = new Vector3(hitTransform.position.x, currentPieceOriginalPosition.y, hitTransform.position.z);
                chessPieceSelected = false;
                whitePiecesCurrentPosition[currentGameObject] = hitTransform.gameObject;
                if (whitePiecesWinningPosition[currentGameObject] == hitTransform.gameObject) WinningCondition();
                currentGameObject = null;
            }
        }
    }

    private void WinningCondition()
    {
        bool hasCompletedPuzzle = whitePiecesWinningPosition.All(kvp => whitePiecesCurrentPosition.TryGetValue(kvp.Key, out var v) && EqualityComparer<GameObject>.Default.Equals(kvp.Value, v));
        if (hasCompletedPuzzle)
        {
            StartCoroutine(KingPieceFall());
           
            ChessPuzzleCompleted?.Invoke(this, EventArgs.Empty);
        }

    }

    /// <summary>
    /// have the king piece, 
    /// take its vector into a position/angle
    /// take the roation and postion you wish to have(hard code that shit)
    /// have floats that count betweeen time
    /// add in a float t for ....
    /// use Lerp to make it fall between the postions
    /// after while is done set transforms to end pos
    /// </summary>
    private IEnumerator KingPieceFall()
    {
        float timeElapsed = 0f;
        float animationTimr = 1f;
        //for rotation and pos hardcode that sht
        float yRot = 0f;
        float xRot = -79.4f;
        float zRot = -39.7f;

        float xPos = 0.04f;
        float yPos = 0.08f;
        float zPos = -0.05f;


        Vector3 kingPos = blackKing.transform.localPosition;
        Quaternion kingRot = blackKing.transform.localRotation;

        Vector3 endPos =new Vector3(xPos,yPos, zPos);
        Quaternion endRot = Quaternion.Euler(xRot, yRot,zRot);

        while (timeElapsed < animationTimr)
        {

            timeElapsed += Time.deltaTime;
            //clamp that shit
            float t = Mathf.Clamp01(timeElapsed / animationTimr);

            blackKing.transform.localPosition = Vector3.Lerp(kingPos, endPos, t);
            blackKing.transform.localRotation = Quaternion.Lerp(kingRot, endRot, t);

            yield return null;

        }
        blackKing.transform.localPosition = endPos;
        blackKing.transform.localRotation = endRot;




    }
}
