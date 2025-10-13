using System;
using UnityEngine;

[DisallowMultipleComponent]
public class StressController : MonoBehaviour
{
    [Header("Stress (0...1)")]
    [SerializeField, Range(0f, 1f)] float _stress = 0f;

    [Header("Decay (per second)")]
    [Range(0f, 1f)] public float DecayAtLow = 0.10f;
    [Range(0f, 1f)] public float DecayAtHigh = 0.30f;

    [Header("Episode (short intense window)")] //Like a chase sequence
    [Range(0f, 1f)] public float EpisodeGate = 0.80f;
    public Vector2 EpisodeDuration = new Vector2(3f, 6f);
    public float EpisodeCooldown = 8f;

    public event Action<float> OnStressChanged;
    public event Action OnEpisodeStarted;
    public event Action OnEpisodeEnded;

    float _extraRate; //additive per second while some sort of source is active
    bool _episodeActive;
    float _epTimer;
    float _epCooldownTimer;
    public float Stress => _stress;

    void Update()
    {
        float dt = Time.deltaTime;
        float decay = Mathf.Lerp(DecayAtLow, DecayAtHigh, _stress);
        _stress = Mathf.Clamp01(_stress + (-decay + Mathf.Max(0f, _extraRate)) * dt);
        OnStressChanged?.Invoke(_stress);
        UpdateEpisode(dt);
    }
    void UpdateEpisode(float dt)
    {
        if (_episodeActive)
        {
            _epTimer -= dt;
            if (_epTimer <= 0f)
            {
                _episodeActive = false;
                OnEpisodeEnded?.Invoke();
                _epCooldownTimer = EpisodeCooldown;
                return;
            }
        }
        if (_epCooldownTimer > 0f)
        {
            _epCooldownTimer -= dt;
            return;
        }
        if (_stress >= EpisodeGate)
        {
            _episodeActive = true;
            _epTimer = UnityEngine.Random.Range(EpisodeDuration.x, EpisodeDuration.y);
            OnEpisodeStarted?.Invoke();
        }
    }
    public void ApplyImpulse(float amount)
    {
        if (amount <= 0f) return;

        _stress = Mathf.Clamp01(_stress + amount);
        OnStressChanged?.Invoke(_stress);

        if (!_episodeActive && _epCooldownTimer <= 0f && _stress >= EpisodeGate)
        {
            _episodeActive = true;
            _epTimer = UnityEngine.Random.Range(EpisodeDuration.x, EpisodeDuration.y);
            OnEpisodeStarted?.Invoke();
        }
    }
    public void AddRate(float perSecond)
    {
        _extraRate = Mathf.Max(0f, perSecond);
    }

}
