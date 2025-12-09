using UnityEngine;
using UnityEngine.Playables;

public class ScriptedTest : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private bool triggerOnce = true;

    [Header("Cutscene Timeline")]
    [SerializeField] private PlayableDirector cutsceneDirector;

    [Header("Elk Demon Settings")]
    [SerializeField] private GameObject elkDemonPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private ElkDemonAI.BehaviorType spawnBehavior = ElkDemonAI.BehaviorType.Idle;
    [SerializeField] private ElkDemonAI.BehaviorType roarBehavior = ElkDemonAI.BehaviorType.Roar;

    private bool triggered = false;
    private ElkDemonAI elkAI;

    // Called when the player enters the trigger
    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && triggered) return;
        if (!other.CompareTag(targetTag)) return;

        triggered = true;

        if (cutsceneDirector != null)
        {
            cutsceneDirector.Play();
            Debug.Log("Cutscene started.");
        }
        else
        {
            Debug.LogWarning("PlayableDirector not assigned to ScriptedTest!");
        }
    }

    // Called by a Timeline Signal to spawn the demon
    public void SpawnDemon()
    {
        if (elkAI != null)
        {
            Debug.Log("Elk Demon already exists. Ignoring spawn signal.");
            return;
        }

        if (elkDemonPrefab == null || spawnPoint == null)
        {
            Debug.LogWarning("Missing Elk Demon prefab or spawn point reference!");
            return;
        }

        var demonInstance = Instantiate(elkDemonPrefab, spawnPoint.position, spawnPoint.rotation);
        elkAI = demonInstance.GetComponent<ElkDemonAI>();

        if (elkAI != null)
        {
            elkAI.ChangeBehavior(spawnBehavior);
        }

        Debug.Log("Elk Demon spawned by Timeline signal.");
    }

    // Called by a Timeline Signal to trigger the roar
    public void DemonRoar()
    {
        if (elkAI == null)
        {
            Debug.LogWarning("No Elk Demon to roar!");
            return;
        }

        elkAI.ChangeBehavior(roarBehavior);
        Debug.Log("Elk Demon roar triggered by Timeline signal.");
    }

    // Despawn demon when player leaves
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
