using UnityEngine;

public class HallucinationInstance : MonoBehaviour
{
    private Transform visualRoot;
    public void BuildVisualIfNeeded(GameObject prefab, int layer)
    {
        if (visualRoot) Destroy(visualRoot.gameObject);
        visualRoot = null;

        if (!prefab) return;

        var go = Instantiate(prefab, transform);
        go.name = "Visual";
        visualRoot = go.transform;

        SetLayerRecursive(go, layer);

        // Safety: visuals only
        foreach (var col in go.GetComponentsInChildren<Collider>(true)) col.enabled = false;
        foreach (var rb in go.GetComponentsInChildren<Rigidbody>(true))
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }
    }

    public void ResetInstance()
    {
        // Nothing persistent yet
    }

    private static void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform c in obj.transform)
            SetLayerRecursive(c.gameObject, layer);
    }
}
