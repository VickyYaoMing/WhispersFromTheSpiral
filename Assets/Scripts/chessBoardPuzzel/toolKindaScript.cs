using System.Collections.Generic;
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
    [SerializeField] private List<Vector3> placedPositions = new List<Vector3>();
    //rename probs and not serilized but make it make sense
    //[SerializeField] float hitboxBottom;
    //[SerializeField] float hitboxHeight;

    //[SerializeField] float HitboxEnd, hitboxStart;//migth need for size of room
    private Collider hitbox;

    //hope all items in a room has hitboxes
    void Start()
    {

        int amountOfChosenItems = Random.Range(0, 10);//how many should be chosen this time// might have to alter range
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
                PlaceOnWall();
                break;

            case SurfaceType.Table:
                Debug.Log(surfaceType);
                break;
            case SurfaceType.Floor:
                Debug.Log(surfaceType);
                break;
            default:
                Debug.Log("Not sure what to put here....");
                break;
        }




    }
    /// <summary>
    /// use floats to set the hitboxes width and height and so on
    /// randomize position it spawns from
    /// place out on that pos/ no ridgidbody on walls
    /// cause walls probas won't need more than one item per hitbox it shouldn't be an issue
    /// if issue call vicky
    /// </summary>
    public void PlaceOnWall()
    {


        //will work for like one item, might have to fix for more on wall
        foreach (GameObject item in placeableItems)
        {
            int amountOftries = 0;
            bool placed = false;
            


            while (amountOftries < 5 && !placed)
            {
                Vector3 randomSpawnPlace = RandomPos();

                if (CheckIfSpace(randomSpawnPlace,item) == true)
                {
                    Instantiate(item, randomSpawnPlace, Quaternion.identity);
                    Debug.Log("Could be placed, yaaay"); 
                    placed = true;
                }
                else
                {
                    amountOftries++;
                    Debug.Log("Try new pos");
                }

            }
            if (amountOftries >= 5 && !placed)
            {
                Debug.Log("Failed to place item after 5 attempts: " + item.name);
            }


        }

    }
    public Vector3 RandomPos()
    {

        Bounds b = hitbox.bounds;
        float randX = Random.Range(b.min.x, b.max.x);//might ned +-1 somewhere
        float randY = Random.Range(b.min.y, b.max.y);
        float z = b.center.z;
        Vector3 randomPos = new Vector3(randX, randY, z);
        return randomPos;

    }
    public bool CheckIfSpace(Vector3 pos, GameObject item)
    {
        Collider itemCollider = item.GetComponent<Collider>();

       
        if (itemCollider == null)
        {
            Debug.LogWarning("Item has no collider: " + item.name);
            return false;
        }
        Vector3 halfsize = itemCollider.bounds.extents;
        float padding = 0.05f;
        halfsize += Vector3.one * padding;

        Collider[] hits = Physics.OverlapBox(pos, halfsize, Quaternion.identity);
        Debug.Log(item.name + " checking at " + pos + ", hits count: " + hits.Length);

        foreach(var hit in hits)
        {
            if(hit != hitbox)
            {
                Debug.Log(item.name + " blocked by " + hit.name);
                return false;
            }
        }
        return true;
        //if (hits.Length==0) return true;
        //else return false;

    }
   
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlaceOnWall();
        }
    }
    
}
