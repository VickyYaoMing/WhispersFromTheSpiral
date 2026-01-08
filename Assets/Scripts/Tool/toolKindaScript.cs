using System.Collections.Generic;
using UnityEngine;

//Set a float t rotate the y axis depeding on the wall so wallitems show up, should be able to do specifically for walls
public class toolKindaScript : MonoBehaviour
{
    /// <summary>
    /// creates a list that has certain items in it
    /// use enums to define surfaces
    /// place them out depending on what kind
    /// if surface is table and floor maybe rotate items  randomly 
    /// might give items scripts if they are wall deco or flat surface items
    /// </summary>

     List<GameObject> placeableItems = new List<GameObject>();//list of all objects 
    [SerializeField] private GameObject listItems;
    List<GameObject> chosenItems = new List<GameObject>();//list of chosen items to place
   
    [SerializeField]int amountOfChosenItems;
    private Collider hitbox;

    [SerializeField] float rotationFloat;
    private void Awake()
    {
        placeableItems.Clear();
        foreach (Transform child in listItems.transform)
        {
            placeableItems.Add(child.gameObject);
        }

    }
    //hope all items in a room has hitboxes
    void Start()
    {

        //how many should be chosen this time// might have to alter range
        hitbox = GetComponent<Collider>();
        chosenItems = RandomizedList(amountOfChosenItems);
        Place();

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
    


    public void Place(/*SurfaceType surface*/)
    {
        
        foreach (GameObject item in chosenItems)
        {
            ItemType type = item.GetComponent<ItemType>();
            Debug.Log(" items is of surfacetype " + type);//just checcking

            int amountOftries = 0;//just for the whileloop
            bool placed = false;


            while (amountOftries < 5 && !placed)
            {
                Vector3 randomSpawnPlace = RandomPos(type.surfaceType);
                bool canPlace = CheckIfSpace(randomSpawnPlace, item, type.surfaceType);
                Debug.Log(canPlace);
                if (canPlace)
                {
                    Quaternion rotation;
                    //fix roation here
                    if (type.surfaceType == SurfaceType.Wall)
                    {
                        rotationFloat = Quaternion.LookRotation(transform.forward).eulerAngles.y;
                        rotation = Quaternion.Euler(0f,rotationFloat,0f);
                    }
                    else
                    {
                     // randomly rotates the item when instanziating ish   
                        float rotationX = Random.Range(0,360);
                        float rotationY = Random.Range(0, 360);
                        float rotationZ = Random.Range(0, 360);
                        rotation = Quaternion.Euler(rotationX,rotationY,rotationZ);
                    }

                    item.SetActive(true);
                    item.transform.position = randomSpawnPlace;
                    item.transform.rotation = rotation;
                    
                    // Instantiate(item, randomSpawnPlace, rotation);

                    //Debug.Log("Could be placed, yaaay"); 
                    canPlace = true;
                    placed= true;
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
                //SERIALISED FIELD FLOT THINGY SUCK MY DICK

                return new Vector3(randX, randY, b.center.z);

            case SurfaceType.Floor:
                return new Vector3(randX, b.max.y, randZ);

            default:
                return transform.position;


        }

    }
    public bool CheckIfSpace(Vector3 pos, GameObject item, SurfaceType surface)
    {
      
        Collider itemCollider = item.GetComponent<Collider>();
        

        if (itemCollider == null)
        {
            Debug.Log("is null");
            return false;
        }

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
        

        //check if oveerlaps
        Collider[] hits = Physics.OverlapBox(center, halfsize, Quaternion.identity);

       
        foreach (var hit in hits)
        {
           
            if (hit != hitbox && hit.tag != "ToolTag")
            {
                Debug.Log(item.name + " blocked by " + hit.name);
                return false;
            }
        }
       
      
        return true;


    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
           
           
        }
    }

}
