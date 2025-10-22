using UnityEngine;

public class ScriptedTest : MonoBehaviour
{
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private GameObject elkDemonPrefab; 
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool triggerOnce = true;

    [Header("Optional AI Control")]
    [SerializeField] private ElkDemonAI elkAI; 
    [SerializeField] private ElkDemonAI.BehaviorType behaviorOnTrigger;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && triggered) return;
        if (!other.CompareTag(targetTag)) return;

        triggered = true;

        // If no Elk Demon exists, spawn one
        if (elkAI == null && elkDemonPrefab && spawnPoint)
        {
            var demonInstance = Instantiate(elkDemonPrefab, spawnPoint.position, spawnPoint.rotation);
            elkAI = demonInstance.GetComponent<ElkDemonAI>();
        }

        // If Elk Demon exists, command it to change behavior
        if (elkAI != null)
        {
            elkAI.ChangeBehavior(behaviorOnTrigger);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(targetTag)) return;

        if (elkAI != null)
        {
            Destroy(elkAI.gameObject);
            elkAI = null;
            Debug.Log("Elk Demon despawned.");
        }
    }
}
