using UnityEngine;

public class SelectChessPiece : MonoBehaviour
{
    private GameObject selectedPiece, selectedSquare = null;
    [SerializeField] private Transform square;
    private bool isSelected=false;
    public Camera cam;
    [SerializeField] private float yOffset = 0.5f;

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetMouseButton(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray,out hit)) 
            
            {
                if (!isSelected)
                {
                    if (hit.collider.CompareTag("ChessPiece"))
                    {
                        selectedPiece = hit.collider.gameObject;
                        isSelected = true;
                        Debug.Log("Selected piece: " + selectedPiece.name);

                    }
                    else 
                    {
                        Debug.Log("this is not a chessPiece");
                    }
                }
                else if (isSelected) 
                
                {
                    if (hit.collider.CompareTag("ChessSquare")) 
                    {
                        selectedSquare=hit.collider.gameObject;
                        Debug.Log("selcted square " + selectedSquare.name);

                       
                        Vector3 targetPos = hit.collider.transform.position;
                        // Uncomment this if your board moves backwards visually:
                      //  targetPos.z = -targetPos.z;

                        
                        selectedPiece.transform.position = targetPos + Vector3.up * yOffset;
                        Debug.DrawLine(selectedPiece.transform.position, hit.collider.transform.position, Color.red, 2f);
                        Debug.Log("Moved " + selectedPiece.name + " to " + selectedSquare.name);

                        
                        selectedPiece = null;
                        isSelected = false;
                    }
                }
            }
        } 

        
    }
}
