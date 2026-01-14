using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuNavigation : MonoBehaviour
{
    [Header("Canvas Groups")]
    [SerializeField] CanvasGroup m_mainCanvasGroup;
    [SerializeField] CanvasGroup m_optionsCanvasGroup;
    [SerializeField] CanvasGroup m_creditsCanvasGroup;

    [Header("Camera")]
    [SerializeField] Camera m_cameraObject; 
    [SerializeField] float m_zoomAmount = 1f;
    [SerializeField] float m_zoomSpeed = 4f;

    [Header("Buttons")]
    [SerializeField] GameObject m_startButton;
    [SerializeField] GameObject m_loadButton;

    private Vector3 m_defaultStartButtonPos = new Vector3(-592, -197, 0);
    private Vector3 m_startButtonPosIfSaveExists;
    private Transform m_camTransform;
    private Vector3 m_initialPosition;
    private FadeAnimator m_fadeAnimator;
    private MenuState m_currentState;

    enum MenuState
    {
        MainView, OptionsView, CreditsView
    }

    #region Unity Functions
    void Start()
    {
        m_loadButton.SetActive(false);
        m_startButton.transform.localPosition = m_defaultStartButtonPos;
        if (GameManager.Instance.saveExists)
        {
            m_loadButton.SetActive(true);
            m_startButtonPosIfSaveExists = m_defaultStartButtonPos + new Vector3(0, 90, 0);
            m_startButton.transform.localPosition = m_startButtonPosIfSaveExists;
        }
        m_camTransform = m_cameraObject.transform;
        m_initialPosition = m_cameraObject.transform.position;
        m_fadeAnimator = GetComponent<FadeAnimator>();
        m_optionsCanvasGroup.alpha = 0f;
        m_creditsCanvasGroup.alpha = 0f;
        m_optionsCanvasGroup.interactable = false;
        m_creditsCanvasGroup.interactable = false;
        m_optionsCanvasGroup.blocksRaycasts = false;
        m_creditsCanvasGroup.blocksRaycasts = false;
        m_fadeAnimator.FadeIn(m_mainCanvasGroup, 2f);
        Cursor.lockState = CursorLockMode.Confined;
    }
    #endregion

    public void ReturnToMenu()
    {
        m_optionsCanvasGroup.interactable = false;
        m_creditsCanvasGroup.interactable = false;
        m_optionsCanvasGroup.blocksRaycasts = false;
        m_creditsCanvasGroup.blocksRaycasts = false;

        switch (m_currentState)
        {
            case MenuState.OptionsView:
                m_fadeAnimator.FadeOut(m_optionsCanvasGroup, 0.5f);
                break;
            case MenuState.CreditsView:
                m_fadeAnimator.FadeOut(m_creditsCanvasGroup, 0.5f);
                break;
        }
        m_fadeAnimator.FadeIn(m_mainCanvasGroup, 0.5f);
        m_currentState = MenuState.MainView;
        m_mainCanvasGroup.interactable = true;
        m_mainCanvasGroup.blocksRaycasts = true;
    }

    public void StartGame()
    {
        StartCoroutine(ChangeToGameScene());
    }

    public void LoadGame()
    {
        GameManager.Instance.ShouldLoad(true);
        StartCoroutine(ChangeToGameScene());
    }

    public void ViewOptions()
    {
        m_mainCanvasGroup.interactable = false;
        m_creditsCanvasGroup.blocksRaycasts = false;
        m_currentState = MenuState.OptionsView;

        m_fadeAnimator.FadeOut(m_mainCanvasGroup, 0.5f);
        m_fadeAnimator.FadeIn(m_optionsCanvasGroup, 0.5f);
        m_optionsCanvasGroup.interactable = true;
        m_optionsCanvasGroup.blocksRaycasts = true;
    }

    public void ViewCredits()
    {
        m_mainCanvasGroup.interactable = false;
        m_optionsCanvasGroup.blocksRaycasts = false;
        m_currentState = MenuState.CreditsView;

        m_fadeAnimator.FadeOut(m_mainCanvasGroup, 0.5f);
        m_fadeAnimator.FadeIn(m_creditsCanvasGroup, 0.5f);
        m_creditsCanvasGroup.interactable = true;
        m_creditsCanvasGroup.blocksRaycasts = true;
    }

    public void QuitGame()
    {
        Application.Quit();

        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    IEnumerator ChangeToGameScene()
    {
        float timeElapsed = 0f;
        Vector3 targetPosition = m_initialPosition + m_camTransform.forward * m_zoomAmount;

        while (timeElapsed < m_zoomSpeed)
        {
            m_camTransform.position = Vector3.Lerp(m_initialPosition, targetPosition, timeElapsed / m_zoomSpeed);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        m_camTransform.position = targetPosition;
        SceneManager.LoadScene("Presentation_Scene");
    }
}
