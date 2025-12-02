using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Audio;

namespace Assets.Scripts.AudioSystem
{
    [CreateAssetMenu(menuName = "Audio/Sound Database", fileName = "SoundDatabase")]
    public class SoundDatabase : ScriptableObject
    {
        [Serializable]
        public class SoundEntry
        {
            public SoundType type;

            [Tooltip("One will be chosen at random when played")]
            public AudioClip[] clips;

            [Header("Mix and Volume")]
            public AudioMixerGroup mixerGroup;
            [Range(0f, 1f)] public float baseVolume = 1f;
            [Range(0.1f, 3f)] public float minPitch = 1f;
            [Range(0.1f, 3f)] public float maxPitch = 1f;

            [Header("3D Sound Settings")]
            public bool spatial = false;
            [Range(0f, 1f)] public float spatialBlend = 1f;
            public float maxDistance = 50f;
            public AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;

            [Header("Looping")]
            public bool loop = false;
            public AudioClip GetRandomClip()
            {
                if (clips == null || clips.Length == 0)
                {
                    return null;
                }
                if (clips.Length == 1)
                {
                    return clips[0];
                }
                int index = UnityEngine.Random.Range(0, clips.Length);
                return clips[index];
            }
            public float GetRandomPitch()
            {
                if (Mathf.Approximately(minPitch, maxPitch))
                {
                    return minPitch;
                }
                return UnityEngine.Random.Range(minPitch, maxPitch);
            }
        }
        [SerializeField] private List<SoundEntry> sounds = new();
        private Dictionary<SoundType, SoundEntry> _lookup;
        void OnEnable()
        {
            BuildLookup();
        }
        private void BuildLookup()
        {
            _lookup = new Dictionary<SoundType, SoundEntry>();
            foreach (var s in sounds)
            {
                if (s == null)
                {
                    continue;
                }
                if (_lookup.ContainsKey(s.type))
                {
                    Debug.LogWarning($"[SoundDatabase] Duplicate SoundType: {s.type}", this);
                    continue;
                }
                _lookup.Add(s.type, s);
            }
        }
        public SoundEntry Get(SoundType type)
        {
            if (_lookup == null || _lookup.Count == 0)
                BuildLookup();

            _lookup.TryGetValue(type, out var entry);
            return entry;
        }
    }
}


