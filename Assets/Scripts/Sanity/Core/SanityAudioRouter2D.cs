using UnityEngine;
using SanitySystem;
using System.Collections.Generic;

public class SanityAudioRouter2D : MonoBehaviour
{
    public Sanity sanity;
    public AudioSource heartbeat2D, breathing2D, voices2D;
    [Range(0f, 1f)] public float heartbeatAtCalm = 0.10f, heartbeatAtBroken = 1.00f;
    [Range(0f, 1f)] public float breathingAtCalm = 0.15f, breathingAtBroken = 0.80f;
    [Range(0f, 1f)] public float voicesAtCalm = 0.00f, voicesAtBroken = 0.70f;
    void Update()
    {
        if (!sanity) return;
        float s = sanity.Sanity01;      // 1 = calm
        float t = 1f - s;               // 0→1 = stress amount
        if (heartbeat2D) heartbeat2D.volume = Mathf.Lerp(heartbeatAtCalm, heartbeatAtBroken, t);
        if (breathing2D) breathing2D.volume = Mathf.Lerp(breathingAtCalm, breathingAtBroken, t);
        if (voices2D) voices2D.volume = Mathf.Lerp(voicesAtCalm, voicesAtBroken, t);
    }
}
