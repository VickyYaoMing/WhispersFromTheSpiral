using UnityEngine;
public enum SurfaceType { Floor, Table, Wall }

public class ItemType : MonoBehaviour
{
    public SurfaceType surfaceType;

    private void Awake()
    {
        foreach (Transform child in transform)
        {
            ItemType type = child.GetComponent<ItemType>();

            if (type == null)
            {
                type= child.gameObject.AddComponent<ItemType>();
            }

            type.surfaceType = surfaceType;//why tho?
        }

    }
}