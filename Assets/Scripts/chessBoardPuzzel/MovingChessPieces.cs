using UnityEngine;
using UnityEngine.InputSystem;

public class MovingChessPieces : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    /// <summary>
    /// 
    /// fins out the vectors of each corner of he board
    /// calculate the width of each square 
    /// use transform to move pieces
    /// might add in rules for different types of chesspieces
    /// remeber you can move the pices so that the player wont have to move all of them
    /// IT ISNOT A CHESS GAME JUST A LIE!!!
    /// </summary>
    //public GameObject chessBoard;
    [SerializeField] private Transform chessBoard;
    [SerializeField] private Camera cameraView;

    bool isPieceChosen = false;
    public GameObject chessPiece;
    float nrOfTiles = 10f;
    private float tilesize;
    [SerializeField] Vector3 boardPos;



    void Start()
    {
        float boardWidth = chessBoard.GetComponent<Renderer>().bounds.size.x;
        tilesize = boardWidth / nrOfTiles;

        boardPos = chessBoard.position - new Vector3(boardWidth / 2f, 0, boardWidth / 2f);
    }

    // Update is called once per frame
    void Update()
    {
        if (cameraView == null) return; //chrsed for some reason so lets try ths
        if (Input.GetMouseButtonDown(0))
        {

            Ray ray = cameraView.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                
                if (hit.collider.CompareTag("ChessPiece"))
                {
                    chessPiece = hit.collider.gameObject;
                    isPieceChosen = true;
                    Debug.Log("chess piece picked"+ isPieceChosen);
                    Debug.Log("chess Piece chosen: "+ chessPiece.name);


                }
                if (isPieceChosen && hit.collider.CompareTag("ChessBoard"))
                {
                    
                    Vector3 localPos = chessBoard.InverseTransformPoint(hit.point);
                    int x = Mathf.FloorToInt(localPos.x/tilesize);
                    int z = Mathf.FloorToInt(localPos.z/tilesize);
                    Debug.Log("x pos: " + x + " " + "z pos:" + z);

                    x = Mathf.Clamp(x, 0, (int)nrOfTiles - 1);
                    z = Mathf.Clamp(z, 0, (int)nrOfTiles - 1);

                    Vector3 tilePos = chessBoard.TransformPoint(new Vector3(x * tilesize + tilesize / 2, 0, z * tilesize + tilesize / 2));
                    chessPiece.transform.position = tilePos;
                    


                    isPieceChosen = false; ;//once moved it deselectes//might need a bool here to make it work
                    Debug.Log("Is piece selected: "+ isPieceChosen);
                } 
            }
        }



    }

}
