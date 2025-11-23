using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public enum SurfaceType { Floor, Table, Wall }//bookshelf??

public class toolKindaScript : MonoBehaviour
{
    /// <summary>
    /// createa list that has certain items in it
    /// use enums to define surfaces
    /// place them out depending on what kind
    /// if surface is table and floor maybe rotate items  randomly 
    /// might give items scripts if they are wall deco or flat surface items
    /// </summary>

    [SerializeField] List<GameObject> placeableItems = new List<GameObject>();//list of all objects 
    List<GameObject> chosenItems = new List<GameObject>();//list of chosen items to place
   // [SerializeField] private List<Vector3> placedPositions = new List<Vector3>();
    [SerializeField]int amountOfChosenItems;
    private Collider hitbox;


    //hope all items in a room has hitboxes
    void Start()
    {

        //how many should be chosen this time// might have to alter range
        hitbox = GetComponent<Collider>();


    }
    /// <summary>
    /// send in the number of items we want
    /// randomize index 
    /// take items from listlist index
    /// add to new list
    /// return list of chosen items
    /// </summary>
    /// <param name="amountOfChosenItems"></param>
    /// <returns></returns>
    private List<GameObject> RandomizedList(int amountOfChosenItems)
    {
        //reason for sending in list might be because dependng on code structure might need mulitple lists
        chosenItems.Clear();//saftey for each time its called
        for (int i = 0; i < amountOfChosenItems; i++)
        {
            int randomIndex = Random.Range(0, placeableItems.Count);
            GameObject go = placeableItems[randomIndex];
            //might need to add in a templist that willuse instaeed so we can remove item already chosen
            chosenItems.Add(go);   //add in random object i list to be placed
        }
        return chosenItems;
    }
    /// <summary>
    /// depening on the surface certain items can be placed
    /// calle method for eocisifc enums logic
    /// </summary>
    private void PlaceItems(SurfaceType surfaceType)
    {

        switch (surfaceType)
        {
            case SurfaceType.Wall:
                Debug.Log(surfaceType);
                Place(surfaceType);
                break;

            case SurfaceType.Table:
                Debug.Log(surfaceType);
                break;
            case SurfaceType.Floor:
                Debug.Log(surfaceType);
                Place(surfaceType);
                break;
            default:
                Debug.Log("Not sure what to put here....");
                break;
        }




    }


    public void Place(SurfaceType surface)
    {
        //will work for like one item, might have to fix for more on wall
        foreach (GameObject item in chosenItems)
        {
            int amountOftries = 0;
            bool placed = false;

            bool tempBool;

            while (amountOftries < 5 && !placed)
            {
                Vector3 randomSpawnPlace = RandomPos(surface);
                tempBool = CheckIfSpace(randomSpawnPlace, item, surface);
                Debug.Log(tempBool);
                if (tempBool)
                {
                    Instantiate(item, randomSpawnPlace, Quaternion.identity);
                    //Debug.Log("Could be placed, yaaay"); 
                    placed = true;
                }
                else
                {
                    amountOftries++;
                    Debug.Log("Try new pos");
                }

            }
            if (!placed)
            {
                Debug.Log("Failed to place item after 5 attempts: " + item.name);
            }


        }

    }
    public Vector3 RandomPos(SurfaceType surface)
    {

        Bounds b = hitbox.bounds;

        float randX = Random.Range(b.min.x, b.max.x);//might ned +-1 somewhere
        float randY = Random.Range(b.min.y, b.max.y);
      
        float randZ = Random.Range(b.min.z, b.max.z);
        switch (surface)
        {
            case SurfaceType.Wall:

                return new Vector3(randX, randY, b.center.z);
            case SurfaceType.Floor:
                return new Vector3(randX, b.max.y, randZ);

            default:
                return transform.position;


        }

    }
    public bool CheckIfSpace(Vector3 pos, GameObject item, SurfaceType surface)
    {
        Debug.Log("In the checkIFspace method");
        Collider itemCollider = item.GetComponent<Collider>();
        Vector3 spawnPos = Vector3.zero;//why tho??

        //if (itemCollider == null)
        //{
        //    Debug.Log("is null");
        //    return false;
        //}
        Vector3 halfsize = itemCollider.bounds.extents;
        float padding = 0.05f;
        halfsize += Vector3.one * padding;

        Vector3 center = pos;

        if (surface == SurfaceType.Floor || surface == SurfaceType.Table)
        {
            Debug.Log("ITS NOT A WAALLLLLL");
            center = new Vector3(pos.x, pos.y + halfsize.y, pos.z);
        }
        else if(surface == SurfaceType.Wall)
        {
            Debug.Log("ITS A WAALLLLLL");
            center = new Vector3(pos.x + halfsize.x, pos.y, pos.z);

        }
        Debug.Log("Did we leave the if statments?");

        //check if oveerlaps
        Collider[] hits = Physics.OverlapBox(center, halfsize, Quaternion.identity);

        Debug.Log("above the foreach  loop");
        foreach (var hit in hits)
        {
            Debug.Log("its in the foreach  loop");
            if (hit != hitbox && hit.tag != "ToolTag")
            {
                Debug.Log(item.name + " blocked by " + hit.name);
                return false;
            }
        }
        Debug.Log("below the foreach  loop");
        // spawnPos = center;
        return true;


    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //int amountOfChosenItems = Random.Range(0, placeableItems.Count);
            Debug.Log("amount randomized in list: " + amountOfChosenItems);
            chosenItems = RandomizedList(amountOfChosenItems);
            Place(SurfaceType.Wall);
        }
    }

}
