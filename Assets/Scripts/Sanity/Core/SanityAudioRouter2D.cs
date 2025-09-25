using UnityEngine;
using SanitySystem;
using System.Collections.Generic;

public class SanityAudioRouter2D : MonoBehaviour
{
    public Sanity _sanity;
    public AudioSource _heartbeat2D, _breathing2D, _voices2D;
    [Range(0f, 1f)] public float _heartbeatAtCalm = 0.10f, _heartbeatAtBroken = 1.00f;
    [Range(0f, 1f)] public float _breathingAtCalm = 0.15f, _breathingAtBroken = 0.80f;
    [Range(0f, 1f)] public float _voicesAtCalm = 0.00f, _voicesAtBroken = 0.70f;
    void Update()
    {
        if (!_sanity) return;
        float s = _sanity.Sanity01;      // 1 = calm
        float t = 1f - s;               // 0→1 = stress amount
        if (_heartbeat2D) _heartbeat2D.volume = Mathf.Lerp(_heartbeatAtCalm, _heartbeatAtBroken, t);
        if (_breathing2D) _breathing2D.volume = Mathf.Lerp(_breathingAtCalm, _breathingAtBroken, t);
        if (_voices2D) _voices2D.volume = Mathf.Lerp(_voicesAtCalm, _voicesAtBroken, t);

        Debug.Log("SanityAudioRouter2D: Sanity is " + s + ", heartbeat volume set to " + (_heartbeat2D ? _heartbeat2D.volume.ToString() : "N/A") + ", breathing volume set to " + (_breathing2D ? _breathing2D.volume.ToString() : "N/A") + ", voices volume set to " + (_voices2D ? _voices2D.volume.ToString() : "N/A"));
    }
}
