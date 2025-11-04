using System;
using UnityEngine;

public class Lightbulb : Default_Item
{
    [SerializeField] bool isUV;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public override GameObject PickedUp()
    {
        return base.PickedUp();
    }

    public bool IsUV {  get { return isUV; } }

    

    // Update is called once per frame
    void Update()
    {
        
    }
}
