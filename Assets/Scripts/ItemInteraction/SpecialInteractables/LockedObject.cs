using UnityEngine;

public class LockedObject : SecondaryInteractionItem
{
    //change this to an object with a key class
    //This is the key that will open this door
    [SerializeField] GameObject key;
    [SerializeField] bool locked = true;

    public bool Locked { get { return locked; } }

    private void Awake()
    {
        interactionManager = GameManager.Instance.InteractionManager;
    }

    public override void SecondaryInteraction()
    {
        //interactionManager = GameManager.Instance.InteractionManager;

        //if (interactionManager.GetItemInHand() != key) return;
        //interactionManager.PlaceItemInHand(Vector3.zero, Quaternion.identity);
        //Destroy(key);
        //locked = false;
        if (interactionManager == null) { Debug.Log("AAAAAAAAA");  return; }
        Debug.Log(interactionManager.GetItemInHand());
        Debug.Log(key);
        var itemInHand = interactionManager.GetItemInHand();
        if(itemInHand != key) { return; }
        interactionManager.PlaceItemInHand(Vector3.zero, Quaternion.identity);
        Destroy(key);
        locked = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
