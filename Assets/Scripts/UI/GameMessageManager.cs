using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameMessageManager : MonoBehaviour
{
    [SerializeField] GameObject GameMessageOverlay;
    private UIManager m_uiManager;
    private CanvasGroup m_gameMessageOverlayGroup;
    private FadeAnimator m_fadeAnimator;

    private TextMeshProUGUI m_tutorialMsgText;
    private TextMeshProUGUI m_subtitleMsgText;

    private GameObject m_currentTutorialMsg;
    private GameObject m_currentSubtitleMsg;
    
    private Queue<GameObject> m_tutorialQueue;
    private Queue<GameObject> m_subtitleQueue;
    public Queue<GameObject> TutorialQueue { get { return m_tutorialQueue; } }
    public Queue<GameObject> SubtitleQueue { get { return m_subtitleQueue; } }

    #region Unity Methods
    private void Start()
    {
        m_gameMessageOverlayGroup = GameMessageOverlay.GetComponent<CanvasGroup>();
        m_fadeAnimator = GetComponent<FadeAnimator>();
        m_tutorialMsgText = GameMessageOverlay.GetComponent<TextMeshProUGUI>();
        m_subtitleMsgText = GameMessageOverlay.GetComponentInChildren<TextMeshProUGUI>();
        m_tutorialQueue = new Queue<GameObject>();
        m_subtitleQueue = new Queue<GameObject>();
        m_currentTutorialMsg = null;
        m_currentSubtitleMsg = null;
    }
    private void Update()
    {
        
        if (m_tutorialQueue.Count > 0)
            UpdateCurrentTutorialMessage();

        if (m_subtitleQueue.Count > 0)
            UpdateCurrentSubtitleMessage();
    }
    #endregion


    private void UpdateCurrentTutorialMessage()
    {
        if (m_tutorialQueue.Peek())
        {
            
        }
    }
    private void UpdateCurrentSubtitleMessage()
    {
        if (m_subtitleQueue.Peek())
        {
        }
    }


}
