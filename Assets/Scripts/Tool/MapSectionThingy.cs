using UnityEngine;

public class MapSectionThingy : MonoBehaviour
{
    public bool sectionActive;
    //toolKindaScript toolList;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sectionActive= true;
    }

    // Update is called once per frame
    void Update()
    {
        //if ()
        //{
        //    //toolList.chosenItems.Clear();
        //}

        if (Input.GetKeyDown(KeyCode.T))
        {
            sectionActive = false;
            // if(Input.GetKeyDown(KeyCode.T) ) { toolToggle = false; }
        }
    }
}
