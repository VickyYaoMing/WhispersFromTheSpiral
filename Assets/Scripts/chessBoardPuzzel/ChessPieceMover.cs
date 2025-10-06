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
                if (hit.collider.CompareTag("ChessPiece") && !isSelected)
                {
                    selectedPiece = hit.collider.gameObject;
                    isSelected = true;
                    Debug.Log("Selected: " + selectedPiece.name);
                }
                else if (isSelected && hit.collider.CompareTag("ChessSquare"))
                {
                    GameObject selectedSquare = hit.collider.gameObject;

                    // Use MovePoint child if available
                    Vector3 targetWorld;
                    if (selectedSquare.transform.childCount > 0)
                        targetWorld = selectedSquare.transform.GetChild(0).position;
                    else
                        targetWorld = selectedSquare.transform.position;

                    // Convert to board-local coordinates and back to world space
                    Vector3 targetLocal = board.InverseTransformPoint(targetWorld);
                    Vector3 finalWorldPos = board.TransformPoint(targetLocal);

                    // Apply Y offset
                    finalWorldPos.y += yOffset;

                    selectedPiece.transform.position = finalWorldPos;
                    Debug.Log($"Moved {selectedPiece.name} to {finalWorldPos}");

                    // Reset selection
                    selectedPiece = null;
                    isSelected = false;
                }
            }
        }
    }
}
