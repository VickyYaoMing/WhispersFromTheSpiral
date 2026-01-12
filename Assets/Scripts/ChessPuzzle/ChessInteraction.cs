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



    private bool chessPieceSelected = false;
    private Vector3 currentPieceOriginalPosition;
    private GameObject originalParent = null;
    private GameObject currentGameObject = null;
    private Dictionary<GameObject, GameObject> whitePiecesWinningPosition;
    private Dictionary<GameObject, GameObject> whitePiecesCurrentPosition;

    public static EventHandler ChessPuzzleCompleted;
    // used for 'animating' the movemet fo the pieces whe you win the game
    [SerializeField] private GameObject blackKing;
    [SerializeField] private GameObject whiteQueen;

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

  
    private IEnumerator KingPieceFall()
    {
        float timeElapsed = 0f;
        float animationTimr = 1f;


        Vector3 startPos = blackKing.transform.localPosition;
        Quaternion startRot = blackKing.transform.localRotation;

        //Vector3 queenPos = whiteQueen.transform.localPosition;
        //Vector3 queenEndPos = new Vector3(0.15f,0.062f,-0.159f);
        //Quaternion queenRot = whiteQueen.transform.localRotation;

        //for rotation and pos hardcode that sht
        float yRot = 0f;
        float xRot = -79.4f;
        float zRot = -39.7f;

        float xPos = 0.04f;
        float yPos = 0.08f;
        float zPos = -0.05f;


        

        Vector3 endPos = new Vector3(xPos, yPos, zPos);
        Quaternion endRot = Quaternion.Euler(xRot, yRot, zRot);

        //Vector3 queenEndPos = startPos;
        

        while (timeElapsed < animationTimr)
        {

            timeElapsed += Time.deltaTime;
            
             float t = Mathf.Clamp01(timeElapsed / animationTimr);
            //float t = timeElapsed / animationTimr;

           // whiteQueen.transform.localPosition = Vector3.Lerp(queenPos, queenEndPos, t);
            blackKing.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            blackKing.transform.localRotation = Quaternion.Lerp(startRot, endRot, t);
               

            yield return null;

        }

        blackKing.transform.localPosition = endPos;
        blackKing.transform.localRotation = endRot;
        //whiteQueen.transform.localPosition = queenEndPos;  




    }
}
