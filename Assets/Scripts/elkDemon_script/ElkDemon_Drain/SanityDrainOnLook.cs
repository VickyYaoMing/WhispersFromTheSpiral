using UnityEngine;
using SanitySystem;

public class SanityDrainOnLook : MonoBehaviour
{
    [Header("Sanity Drain Settings")]
    [SerializeField] private float drainPerSecond = -0.15f;

    private ISanityProvider playerSanity;    

    void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerSanity = player.GetComponent<ISanityProvider>();
        }
    }

    public void DrainTick()
    {
        if (playerSanity == null)
        {
            return;
        }

        playerSanity.ApplyImpulse(drainPerSecond * Time.deltaTime);
    } 
}
