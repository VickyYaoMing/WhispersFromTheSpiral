using UnityEngine;
using SanitySystem;

public class SanityDrainOnLook : MonoBehaviour
{
    [Header("Sanity Drain Settings")]
    [SerializeField] private float drainSanityPerSecond = -0.06f;
    [Header("Stress Drain Settings")]
    [SerializeField] private float addStressPerSecond = 0.1f;

    private ISanityProvider playerSanity;
    private StressController stress;
    

    void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerSanity = player.GetComponent<ISanityProvider>();
            stress = player.GetComponent<StressController>();
        }
    }

    public void DrainTick()
    {
        if (playerSanity == null)
        {
            return;
        }

        stress.ApplyImpulse(addStressPerSecond * Time.deltaTime);
        if(stress.Stress > 0.8f)
        {
            playerSanity.ApplyImpulse(drainSanityPerSecond * Time.deltaTime);
        }

    } 
}
