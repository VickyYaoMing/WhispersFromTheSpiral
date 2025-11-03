using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameMessageManager : MonoBehaviour
{
    [SerializeField] GameObject GameMessageOverlay;
    private UIManager m_uiManager;
    private CanvasGroup m_gameMessageOverlayGroup;
    private FadeAnimator m_fadeAnimator;

    private bool m_shouldPauseExecution;

    private TextMeshProUGUI m_tutorialMsgTextObj;
    private TextMeshProUGUI m_subtitleMsgTextObj;

    private GameObject m_currentTutorialMsgObj;
    private GameObject m_currentSubtitleMsgObj;
    private bool m_isTutorialMessagePlaying;
    private bool m_isSubtitleMessagePlaying;
    private float m_currentTutorialTimeLeft;
    private float m_currentSubtitleTimeLeft;

    private Queue<GameObject> m_tutorialQueue;
    private Queue<GameObject> m_subtitleQueue;
    public Queue<GameObject> TutorialQueue { get { return m_tutorialQueue; } }
    public Queue<GameObject> SubtitleQueue { get { return m_subtitleQueue; } }

    #region Unity Methods
    private void Start()
    {
        m_gameMessageOverlayGroup = GameMessageOverlay.GetComponent<CanvasGroup>();
        m_uiManager = GetComponent<UIManager>();
        m_fadeAnimator = GetComponent<FadeAnimator>();
        m_tutorialMsgTextObj = GameMessageOverlay.GetComponent<TextMeshProUGUI>();
        m_subtitleMsgTextObj = GameMessageOverlay.GetComponentInChildren<TextMeshProUGUI>();
        m_tutorialQueue = new Queue<GameObject>();
        m_subtitleQueue = new Queue<GameObject>();
        m_currentTutorialMsgObj = null;
        m_currentSubtitleMsgObj = null;
    }
    private void Update()
    {
        if (m_uiManager.IsPaused || m_uiManager.IsNotebookActive || m_uiManager.IsViewingCollectible)
        {
            m_shouldPauseExecution = true;
            return;
        }

        m_shouldPauseExecution = false;
        if (m_isSubtitleMessagePlaying || m_isTutorialMessagePlaying)
        {
            m_fadeAnimator.FadeIn(m_gameMessageOverlayGroup, 0.5f);
        }
        if (!m_isTutorialMessagePlaying && !m_isSubtitleMessagePlaying)
        {
            m_fadeAnimator.FadeOut(m_gameMessageOverlayGroup, 0.5f);
        }
        if (m_tutorialQueue.Count > 0)
            UpdateCurrentTutorialMessage();

        if (m_subtitleQueue.Count > 0)
            UpdateCurrentSubtitleMessage();
    }
    #endregion


    private void UpdateCurrentTutorialMessage()
    {
        if (m_tutorialQueue.Peek() != null && m_currentTutorialMsgObj == null)
        {
            m_isTutorialMessagePlaying = true;
            m_currentTutorialMsgObj = m_tutorialQueue.Peek();

            m_tutorialMsgTextObj.text = m_currentTutorialMsgObj.GetComponent<GameMessage>().Description.text;
            m_currentTutorialTimeLeft = m_currentTutorialMsgObj.GetComponent<GameMessage>().Duration;    
        }
        if (!m_shouldPauseExecution)
        {
            m_currentTutorialTimeLeft -= Time.deltaTime;
        }
        if (m_currentTutorialTimeLeft < 0)
        {
            m_tutorialMsgTextObj.text = string.Empty;
            m_isTutorialMessagePlaying = false;
            m_currentTutorialTimeLeft = 0;
            m_tutorialQueue.Dequeue();
            m_currentTutorialMsgObj.GetComponent<GameMessage>().OnFinishedPlaying();
            m_currentTutorialMsgObj = null;
        }
    }
    private void UpdateCurrentSubtitleMessage()
    {
        if (m_subtitleQueue.Peek())
        {
        }
    }


}
