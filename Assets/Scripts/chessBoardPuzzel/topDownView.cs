using System.Threading;
using UnityEngine;

public class topDownVIew : MonoBehaviour
{
    public Camera playerView;
    public Camera topDownCamera;
    public GameObject gameObjectPlayer;
    public float activationDistance = 10f; //radnom values et now to check it works
    private bool isTopDownCameraActive = false;
    private Transform player;//might need
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        topDownCamera.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        float distanceToChessBoard = Vector3.Distance(player.position, transform.position);

        if (Input.GetKeyDown(KeyCode.K) && distanceToChessBoard <= activationDistance)
        {
            switch (isTopDownCameraActive)//state of bool
            {
                case true://if true then set the playerview to false and topdaown to true
                    playerView.enabled = false;
                    topDownCamera.enabled = true;
                    gameObjectPlayer.SetActive(false);
                    if (Input.GetKeyDown(KeyCode.K)) 
                    {
                        isTopDownCameraActive = false;
                        break;
                    }

                    break;
                case false:
                    playerView.enabled = true;
                    topDownCamera.enabled = false;
                    gameObjectPlayer.SetActive(true);
                    if (Input.GetKeyDown(KeyCode.K))
                    {
                        isTopDownCameraActive = true;
                        break;
                    }
                    break;
            }
            Debug.Log("pressed the key");
            if (player == null)
            {
                Debug.Log("Player not found");
            }

            //isCameraActive = true;

            //if (isCameraActive)
            //{
            //    ChangeCamera();


            //}
            //if (!isCameraActive)
            //{
            //    //something to chnge it 
            //}
        }

    }
    
}