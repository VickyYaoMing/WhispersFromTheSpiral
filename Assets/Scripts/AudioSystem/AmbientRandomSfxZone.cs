using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.AudioSystem;
using UnityEditor.EditorTools;
namespace AudioSystem
{
    /// <summary>
    /// Plays random ambient sound effects within a defined zone.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class AmbientRandomSfxZone : MonoBehaviour
    {
        [Header("Sounds")]
        public SoundType[] soundTypes;

        [Range(0f, 2f)] public float minVolume = 0.8f;
        [Range(0f, 2f)] public float maxVolume = 1.2f;

        [Header("Timing")]
        public Vector2 delayRange = new Vector2(8f, 20f);

        [Header("Activation")]
        public bool requirePlayerInside = true;
        public string playerTag = "Player";
        [Header("Placement")]
        public bool useChildSpawnPoints = false;
        [Tooltip("If no child spawn points are found, random points within the collider will be used.")]
        public float radius = 5f;
        [Tooltip("if true, match y to player height")]
        public bool alignToPlayerHeight = true;

        //internal 
        private readonly List<Transform> spawnPoints = new();
        private bool playerInside = false;
        private Transform playerTransform;
        private Coroutine loopRoutine;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }
        private void Awake()
        {
            if (useChildSpawnPoints)
            {
                spawnPoints.Clear();
                foreach (Transform child in transform)
                {
                    spawnPoints.Add(child);
                }
            }
        }
        private void Start()
        {
            if (playerTransform == null && !string.IsNullOrEmpty(playerTag))
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
                if (playerObj != null)
                {
                    playerTransform = playerObj.transform;
                }
            }
            if (soundTypes == null || soundTypes.Length == 0)
            {
                Debug.LogWarning("No sound types assigned to AmbientRandomSfxZone on " + gameObject.name);
                return;
            }
            loopRoutine = StartCoroutine(AmbientLoop());
        }
        private void OnEnable()
        {
            if (loopRoutine == null && Application.isPlaying && soundTypes != null && soundTypes.Length > 0)
            {
                loopRoutine = StartCoroutine(AmbientLoop());
            }
        }
        private void OnDisable()
        {
            if (loopRoutine != null)
            {
                StopCoroutine(loopRoutine);
                loopRoutine = null;
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            if (!requirePlayerInside) return;
            if (other.CompareTag(playerTag))
            {
                playerInside = true;
                if (playerTransform == null)
                {
                    playerTransform = other.transform;
                }
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (!requirePlayerInside) return;
            if (other.CompareTag(playerTag))
            {
                playerInside = false;
            }
        }
        private IEnumerator AmbientLoop()
        {
            while (true)
            {
                float delay = Random.Range(delayRange.x, delayRange.y);
                yield return new WaitForSeconds(delay);

                //Condition if we need the player inside
                if (requirePlayerInside && !playerInside)
                {
                    continue;
                }
                if (soundTypes == null || soundTypes.Length == 0)
                {
                    continue;
                }
                SoundType type = soundTypes[Random.Range(0, soundTypes.Length)];
                if (type == SoundType.None)
                {
                    continue;
                }
                Vector3 pos = GetSpawnPosition();

                //random volume
                float volume = Random.Range(minVolume, maxVolume);

                SoundManager.PlayAt(type, pos, volume);
            }
        }
        //position helpers
        private Vector3 GetSpawnPosition()
        {
            if (useChildSpawnPoints && spawnPoints.Count > 0)
            {
                Transform t = spawnPoints[Random.Range(0, spawnPoints.Count)];
                return AlignYIfNeeded(t.position);
            }
            //otherwise random point in collider
            Vector3 circle = Random.insideUnitCircle * radius;
            Vector3 pos = transform.position + new Vector3(circle.x, 0f, circle.y);
            return AlignYIfNeeded(pos);
        }
        private Vector3 AlignYIfNeeded(Vector3 pos)
        {
            if (!alignToPlayerHeight || playerTransform == null)
            {
                return pos;
            }
            pos.y = playerTransform.position.y;
            return pos;
        }
        //For editor visualization
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, radius);

            if (useChildSpawnPoints)
            {
                Gizmos.color = new Color(0.3f, 1f, 0.3f, 0.8f);
                foreach (Transform child in transform)
                {
                    Gizmos.DrawSphere(child.position, 0.2f);
                }
            }
        }
#endif
    }
}
