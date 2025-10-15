using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuNavigation : MonoBehaviour
{
    [SerializeField] CanvasGroup m_mainCanvasGroup;
    [SerializeField] CanvasGroup m_optionsCanvasGroup;
    [SerializeField] CanvasGroup m_creditsCanvasGroup;
    [SerializeField] Camera m_cameraObject; 
    [SerializeField] float m_zoomAmount = 1f;
    [SerializeField] float m_zoomSpeed = 4f;

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
        m_camTransform = m_cameraObject.transform;
        m_initialPosition = m_cameraObject.transform.position;
        m_fadeAnimator = GetComponent<FadeAnimator>();
        m_optionsCanvasGroup.alpha = 0f;
        m_creditsCanvasGroup.alpha = 0f;
        m_fadeAnimator.FadeIn(m_mainCanvasGroup, 2f);
    }
    #endregion

    public void ReturnToMenu()
    {
        m_optionsCanvasGroup.interactable = false;
        m_creditsCanvasGroup.interactable = false;

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
    }

    public void StartGame()
    {
        StartCoroutine(ChangeToGameScene());
    }

    public void ViewOptions()
    {
        m_mainCanvasGroup.interactable = false;
        m_currentState = MenuState.OptionsView;

        m_fadeAnimator.FadeOut(m_mainCanvasGroup, 0.5f);
        m_fadeAnimator.FadeIn(m_optionsCanvasGroup, 0.5f);
        m_optionsCanvasGroup.interactable = true;
    }

    public void ViewCredits()
    {
        m_mainCanvasGroup.interactable = false;
        m_currentState = MenuState.CreditsView;

        m_fadeAnimator.FadeOut(m_mainCanvasGroup, 0.5f);
        m_fadeAnimator.FadeIn(m_creditsCanvasGroup, 0.5f);
        m_creditsCanvasGroup.interactable = true;
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
        SceneManager.LoadScene("Mansion_Main");
    }
}
