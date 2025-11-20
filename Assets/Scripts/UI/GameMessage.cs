using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public enum MessageType
{
    Tutorial,
    Subtitle
}

[RequireComponent(typeof(Collider))]
[RequireComponent (typeof(Text))]
public class GameMessage : MonoBehaviour
{
    [SerializeField] float m_duration;
    [SerializeField] MessageType m_type;
    [SerializeField] GameMessageManager PlayerMessageManager;
    public AudioClip[] AudioClips;
    public Text Description { get; private set; }
    public string[] DescriptionAsPages { get; private set; }
    public float[] Durations { get; private set; }
    public bool HasAudio {  get; private set; }
    public MessageType Type { get; private set; }

    private bool m_isTriggered;

    #region Unity Methods
    private void Start()
    {
        Description = GetComponent<Text>();
        Type = m_type;
        DescriptionAsPages = SplitDescription(Description.text);

        Durations = new float[AudioClips.Length];
        if(AudioClips.Length != 0)
        {
            Durations = new float[AudioClips.Length];
            for (int i = 0; i < AudioClips.Length; i++)
            {
                Durations[i] = AudioClips[i].length;
            }
            HasAudio = true;
        }
        else
        {
            Durations = new float[DescriptionAsPages.Length];
            for (int i = 0; i < DescriptionAsPages.Length; i++)
            {
                Durations[i] = m_duration;
            }
            HasAudio = false;
        }
        m_isTriggered = false;
    }
    #endregion

    public void OnFinishedPlaying()
    {
        Destroy(gameObject);
    }

    private string[] SplitDescription(string description)
    {
        List<string> pages = new();
        string[] lines = description.Split(new[] { "\n\n" }, StringSplitOptions.None);
        foreach (string line in lines)
        {
            pages.Add(line);
        }
        return pages.ToArray();
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (!collider.CompareTag(StringLiterals.PLAYER_TAG) || m_isTriggered)
            return;
        if (m_type == MessageType.Tutorial)
        {
            PlayerMessageManager.TutorialQueue.Enqueue(gameObject);
        } 
        if (m_type == MessageType.Subtitle)
        {
            PlayerMessageManager.SubtitleQueue.Enqueue(gameObject);
        }
        m_isTriggered = true;
    }
}
