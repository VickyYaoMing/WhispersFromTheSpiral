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

    List<GameObject> placeableItems = new List<GameObject>();//list of all objects 
    List<GameObject> chosenItems = new List<GameObject>();//list of chosen items to place
   [SerializeField] float bottomOfWall;
    [SerializeField] float topOfWall;

    [SerializeField] float roomWidht, roomLength;//migth need for size of room
    //hope all items in a room has hitboxes
    void Start()
    {
        
        int amountOfChosenItems = Random.Range(0, 10);//how many should be chosen this time// might have to alter range

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
            chosenItems.Add (go);   //add in random object i list to be placed
        }
        return chosenItems;
    }
    /// <summary>
    /// depening on the surface certain items can be placed
    /// </summary>
    private void PlaceItems()
    {
       // if (GameObject go is something)// if an item is a wall or floo item then place in certain rotation/height

    }

    // Update is called once per frame
    void Update()
    {

    }
}
