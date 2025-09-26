using UnityEngine;

public class topDownVIew : MonoBehaviour
{
    public Camera playerView;
    public Camera topDownCamera;
    float distanceToChessBoard = 3f; //radnom values et now to check it works
    private bool isCameraActive = false;
    private Transform player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        topDownCamera.enabled=false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            float dist = Vector3.Distance(player.position, transform.position);
            if (dist <= distanceToChessBoard) 
            {
                ChangeCamera();
            }
        }
        
    }
    void ChangeCamera()
    {
        isCameraActive=false;
        playerView.enabled=!isCameraActive;
        topDownCamera.enabled=isCameraActive;
    }
}