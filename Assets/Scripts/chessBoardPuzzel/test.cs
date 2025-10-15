using UnityEngine;

public class test : MonoBehaviour
{
    ChessPieceMover pieceMover;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if ( pieceMover.wonthegame )
        {
            Debug.Log("Yaay the clock open, you got a gun");
        }
    }
}
