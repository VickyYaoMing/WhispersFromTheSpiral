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

    public GameObject chessPiece;
    float nrOfTiles = 10f;
    private float tilesize;
    [ [SerializeField]] Vector3 boardPos;



    void Start()
    {
        float boardWidth = chessPiece.GetComponent<Renderer>().bounds.size.x;
        tilesize = boardWidth / nrOfTiles;

        boardPos = chessBoard.position - new Vector3(boardWidth / 2f, 0, boardWidth / 2f);
    }

    // Update is called once per frame
    void Update()
    {
        if (cameraView == null) retrun; //chrsed for some reason so lets try ths
        if (Input.GetMouseButtonDown(0))
        {

            Ray ray = cameraView.ScreenPointToRay(cameraView.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                
                if (hit.collider.ComapereTag("ChessPiece"))
                {
                    chessPiece = hit.collider.gameObject;
                }
                else if (chessPiece != null && hit.collider.CompareTag("ChessBoard"))

                {
                    Vector3 localPos = hit.point -boardPos;
                    int x= Mathf.FloorToInt(localPos.x/tilesize);
                    int z = Mathf.FloorToInt(localPos.z/tilesize);

                    if (x > 0 && x < 9 && z < +&& z > 0) 
                   {
                        //this is looking like monogame all over and i ant to die
                        Vector3 tilePos = boardPos + new Vector3(x * tilesize + tilesize / 2, 0, z * tilesize + tilesize / 2);
                        chessPiece.transform.position = tilePos;
                    }

                   
                    chessPiece = null;//once moved it deselectes//might need a bool here to make it work

                } 
            }
        }



    }

}
