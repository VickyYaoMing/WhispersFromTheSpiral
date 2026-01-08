using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
namespace Assets.Scripts.AudioSystem
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }
        [Header("Database")]
        [SerializeField] private SoundDatabase soundDatabase;

        [Header("Mixer")]
        public AudioMixer audioMixer;
        [Tooltip("Exposed parameter name for master volume, e.g. \"MasterVolume\"")]
        public string masterVolumeParam = "Master";
        public string musicVolumeParam = "Music";
        public string sfxVolumeParam = "SFX";

        [Header("SFX pool")]
        [SerializeField] private int initialSfxSources = 8;
        [SerializeField] private int maxSfxSources = 16;
        [SerializeField] private AudioMixerGroup defaultSfxGroup;

        [Header("UI Audio")]
        [SerializeField] private AudioSource uiAudioSource;
        [SerializeField] private AudioMixerGroup uiMixerGroup;

        [Header("Music")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioMixerGroup musicMixerGroup;
        [SerializeField, Range(0f, 1f)] private float defaultMusicVolume = 1f;

        private readonly List<AudioSource> _sfxSources = new();

        #region Unity
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitSfxPool();
            InitUiSource();
            InitMusicSource();
        }
        #endregion
        #region Init
        private void InitSfxPool()
        {
            for (int i = 0; i < initialSfxSources; i++)
            {
                CreateSfxSource();
            }
        }
        private AudioSource CreateSfxSource()
        {
            if (_sfxSources.Count >= maxSfxSources)
            {
                return null;
            }
            var go = new GameObject($"SFX_AudioSource_{_sfxSources.Count}");
            go.transform.SetParent(transform, false);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.outputAudioMixerGroup = defaultSfxGroup;
            _sfxSources.Add(source);
            return source;
        }
        private void InitUiSource()
        {
            if (uiAudioSource == null)
            {
                var go = new GameObject("UI_AudioSource");
                go.transform.SetParent(transform, false);
                uiAudioSource = go.AddComponent<AudioSource>();
                uiAudioSource.playOnAwake = false;
                uiAudioSource.spatialBlend = 0f;
            }
            if (uiMixerGroup != null)
            {
                uiAudioSource.outputAudioMixerGroup = uiMixerGroup;
            }
        }
        private void InitMusicSource()
        {
            if (musicSource == null)
            {
                var go = new GameObject("Music_AudioSource");
                go.transform.SetParent(transform, false);
                musicSource = go.AddComponent<AudioSource>();
                musicSource.playOnAwake = false;
                musicSource.loop = true;
            }
            if (musicMixerGroup != null)
            {
                musicSource.outputAudioMixerGroup = musicMixerGroup;
            }
            musicSource.volume = defaultMusicVolume;
        }
        #endregion

        #region SFX API (stataiac)
        //2D SFX
        public static void Play(SoundType type, float volumeMultiplier = 1f)
        {
            if (Instance == null)
            {
                return;
            }
            Instance.PlaySfxInternal(type, Vector3.zero, null, false, volumeMultiplier);
        }
        //3D SFX
        public static void PlayAt(SoundType type, Vector3 position, float volumeMultiplier = 1f, bool v = false)
        {
            if (Instance == null)
            {
                return;
            }
            Instance.PlaySfxInternal(type, position, null, true, volumeMultiplier);
        }
        public static AudioSource PlayAttached(SoundType type, Transform followTarget, float volumeMultiplier = 1f)
        {
            if (Instance == null)
            {
                return null;
            }
            return Instance.PlaySfxInternal(type, followTarget.position, followTarget, true, volumeMultiplier);
        }
        #endregion

        #region UI API (static)
        public static void PlayUI(SoundType type, float volumeMultiplier = 1f)
        {
            if (Instance == null)
            {
                return;
            }
            Instance.PlayUiInternal(type, volumeMultiplier);
        }
        #endregion

        #region Music API (static)
        public static void PlayMusic(SoundType type, float fadeDuration = 1f)
        {
            if (Instance == null)
            {
                return;
            }
            Instance.PlayMusicInternal(type, fadeDuration);
        }
        public static void StopMusic(float fadeDuration = 1f)
        {
            if (Instance == null)
            {
                return;
            }

            Instance.StopMusicInternal(fadeDuration);
        }
        #endregion

        #region Volume API (static)
        public static void SetMasterVolume(float volume01)
        {
            if (Instance == null || Instance.audioMixer == null)
            {
                return;
            }
            Instance.SetVolume01(Instance.masterVolumeParam, volume01);
        }

        public static void SetMusicVolume(float volume01)
        {
            if (Instance == null || Instance.audioMixer == null)
            {
                return;
            }
            Instance.SetVolume01(Instance.musicVolumeParam, volume01);
        }

        public static void SetSfxVolume(float volume01)
        {
            if (Instance == null || Instance.audioMixer == null)
            {
                return;
            }
            Instance.SetVolume01(Instance.sfxVolumeParam, volume01);
        }
        #endregion

        #region  Internal SFX
        private AudioSource PlaySfxInternal(SoundType type, Vector3 position, Transform attachTo, bool as3D, float volumeMultiplier)
        {
            var entry = soundDatabase != null ? soundDatabase.Get(type) : null;
            if (entry == null)
            {
                Debug.LogWarning($"[SoundManager] No SoundEntry found for {type}");
                return null;
            }

            var clip = entry.GetRandomClip();
            if (clip == null)
            {
                Debug.LogWarning($"[SoundManager] SoundEntry {type} has no AudioClip assigned.");
                return null;
            }

            var source = GetFreeSfxSource();
            if (source == null) return null;

            if (attachTo != null)
            {
                source.transform.SetParent(attachTo, worldPositionStays: false);
                source.transform.localPosition = Vector3.zero;
            }
            else
            {
                source.transform.SetParent(transform, worldPositionStays: false);
                source.transform.position = position;
            }

            // 2D / 3D
            if (entry.spatial && as3D)
            {
                source.spatialBlend = entry.spatialBlend;
                source.maxDistance = entry.maxDistance;
                source.rolloffMode = entry.rolloffMode;
            }
            else
            {
                source.spatialBlend = 0f;
            }

            // Mixer + properties
            source.outputAudioMixerGroup = entry.mixerGroup ?? defaultSfxGroup;
            source.clip = clip;
            source.loop = entry.loop;
            source.pitch = entry.GetRandomPitch();
            source.volume = entry.baseVolume * Mathf.Clamp01(volumeMultiplier);

            source.Play();
            return source;
        }
        private AudioSource GetFreeSfxSource()
        {
            foreach (var src in _sfxSources)
            {
                if (!src.isPlaying)
                    return src;
            }

            // If none free, try to create a new one (if under max)
            return CreateSfxSource();
        }
        #endregion

        #region Internal UI

        private void PlayUiInternal(SoundType type, float volumeMultiplier)
        {
            if (uiAudioSource == null)
            {
                Debug.LogWarning("[SoundManager] UI AudioSource is null.");
                return;
            }

            var entry = soundDatabase != null ? soundDatabase.Get(type) : null;
            if (entry == null)
            {
                Debug.LogWarning($"[SoundManager] No SoundEntry found for UI sound {type}");
                return;
            }

            var clip = entry.GetRandomClip();
            if (clip == null)
            {
                Debug.LogWarning($"[SoundManager] UI SoundEntry {type} has no AudioClip.");
                return;
            }
            var group = entry.mixerGroup ?? uiMixerGroup ?? defaultSfxGroup;
            Debug.Log($"[SoundManager] UI Play type={type}, clip={clip.name}, baseVol={entry.baseVolume}, mixerGroup={(group != null ? group.name : "NULL")}");

            uiAudioSource.outputAudioMixerGroup = group;
            uiAudioSource.pitch = entry.GetRandomPitch();
            uiAudioSource.volume = entry.baseVolume * Mathf.Clamp01(volumeMultiplier);
            uiAudioSource.PlayOneShot(clip);
        }
        #endregion

        #region Internal Music

        private Coroutine _musicRoutine;

        private void PlayMusicInternal(SoundType type, float fadeDuration)
        {
            var entry = soundDatabase != null ? soundDatabase.Get(type) : null;
            if (entry == null)
            {
                Debug.LogWarning($"[SoundManager] No SoundEntry found for music {type}");
                return;
            }

            var clip = entry.GetRandomClip();
            if (clip == null)
            {
                Debug.LogWarning($"[SoundManager] Music SoundEntry {type} has no AudioClip.");
                return;
            }

            if (_musicRoutine != null)
                StopCoroutine(_musicRoutine);

            _musicRoutine = StartCoroutine(FadeToMusic(clip, entry, fadeDuration));
        }
        private void StopMusicInternal(float fadeDuration)
        {
            if (musicSource.clip == null || !musicSource.isPlaying)
                return;

            if (_musicRoutine != null)
                StopCoroutine(_musicRoutine);

            _musicRoutine = StartCoroutine(FadeOutMusic(fadeDuration));
        }

        private IEnumerator FadeToMusic(AudioClip newClip, SoundDatabase.SoundEntry entry, float fadeDuration)
        {
            float half = Mathf.Max(0.01f, fadeDuration * 0.5f);
            float t = 0f;
            float startVolume = musicSource.volume;

            // Fade out current
            while (t < half && musicSource.clip != null)
            {
                t += Time.unscaledDeltaTime;
                float k = t / half;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, k);
                yield return null;
            }

            // Switch clip
            musicSource.clip = newClip;
            musicSource.loop = true;
            musicSource.outputAudioMixerGroup = entry.mixerGroup ?? musicMixerGroup;
            musicSource.pitch = entry.GetRandomPitch();
            musicSource.volume = 0f;
            musicSource.Play();

            // Fade in
            t = 0f;
            while (t < half)
            {
                t += Time.unscaledDeltaTime;
                float k = t / half;
                musicSource.volume = Mathf.Lerp(0f, entry.baseVolume, k);
                yield return null;
            }

            musicSource.volume = entry.baseVolume;
            _musicRoutine = null;
        }
        private IEnumerator FadeOutMusic(float fadeDuration)
        {
            float t = 0f;
            float startVolume = musicSource.volume;

            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                float k = t / fadeDuration;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, k);
                yield return null;
            }

            musicSource.Stop();
            musicSource.clip = null;
            _musicRoutine = null;
        }

        #endregion
        #region Mixer Volume Helpers

        private void SetVolume01(string parameterName, float volume01)
        {
            if (string.IsNullOrEmpty(parameterName)) return;
            float v01 = Mathf.Clamp01(volume01);
            float dB = Mathf.Lerp(-80f, 0f, v01);
            audioMixer.SetFloat(parameterName, dB);
        }

        #endregion
    }
}
