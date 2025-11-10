using UnityEngine;

public class LockedObject : SecondaryInteractionItem
{
    //change this to an object with a key class
    //This is the key that will open this door
    [SerializeField] GameObject key;
    bool locked;

    public bool Locked { get { return locked; } }

    void Start()
    {
        HasSecondaryInteraction = true;
    }

    public override void SecondaryInteraction()
    {
        
        if (!GameManager.Instance.InteractionManager.GetItemInHand() == key) return;

        GameManager.Instance.InteractionManager.PlaceItemInHand(Vector3.zero, Quaternion.identity);
        Destroy(key);
        locked = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
