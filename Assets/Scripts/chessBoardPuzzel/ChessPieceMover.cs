using UnityEngine;

public class ChessPieceMover : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private float yOffset = 0.5f;
    [SerializeField] private Transform board;    // Parent board object

    private GameObject selectedPiece = null;
    private bool isSelected = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("Casting ray from camera: " + cam.name);

                if (hit.collider.CompareTag("ChessPiece") )
                {
                     selectedPiece = hit.collider.gameObject;                    selectedPiece = hit.collider.GetComponentInParent<Transform>().gameObject;
                    
                    Debug.Log("Selected: " + selectedPiece.name);
                    
                }
                else if (isSelected && hit.collider.CompareTag("ChessSquare"))
                {
                    GameObject selectedSquare = hit.collider.gameObject;
                    Debug.Log(selectedSquare.name);
                   

                    // Use MovePoint child if available

                }
            }
        }
    }
}
