using UnityEngine;
namespace Assets.Scripts.AudioSystem
{
    public class PlaySoundOnTrigger : MonoBehaviour
    {
        public SoundType soundType = SoundType.None;
        public Transform elkTransform;
        public bool onlyOnce = false;
        private bool _hasPlayed = false;
        private void OnTriggerEnter(Collider other)
        {
            if (onlyOnce && _hasPlayed)
            {
                return;
            }
            if (!other.CompareTag("Player"))
            {
                return;
            }
            SoundManager.PlayAt(soundType, elkTransform.position);
            _hasPlayed = true;
        }
    }
}


