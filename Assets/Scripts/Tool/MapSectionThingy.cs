using UnityEngine;

public class MapSectionThingy : MonoBehaviour
{
    public bool sectionActive;
    public Transform player;
    float inrange = 40;
    //toolKindaScript toolList;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sectionActive= true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, player.position) < inrange)
        {
            sectionActive = true;
        }
        else
        {
            sectionActive = false;
        }

        //if ()
        //{
        //    //toolList.chosenItems.Clear();
        //}
     
    }
}
