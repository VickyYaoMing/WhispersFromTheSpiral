using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuNavigation : MonoBehaviour
{
    [SerializeField] CanvasGroup mainCanvasGroup;
    [SerializeField] CanvasGroup optionsCanvasGroup;
    [SerializeField] CanvasGroup creditsCanvasGroup;

    private FadeAnimator m_fadeAnimator;
    private MenuState m_currentState;

    enum MenuState
    {
        MainView, OptionsView, CreditsView
    }

    #region Unity Functions
    void Start()
    {
        m_fadeAnimator = GetComponent<FadeAnimator>();
        optionsCanvasGroup.alpha = 0f;
        creditsCanvasGroup.alpha = 0f;
        m_fadeAnimator.FadeIn(mainCanvasGroup, 2f);
    }
    #endregion

    public void ReturnToMenu()
    {
        optionsCanvasGroup.interactable = false;
        creditsCanvasGroup.interactable = false;

        switch (m_currentState)
        {
            case MenuState.OptionsView:
                m_fadeAnimator.FadeOut(optionsCanvasGroup, 0.5f);
                break;
            case MenuState.CreditsView:
                m_fadeAnimator.FadeOut(creditsCanvasGroup, 0.5f);
                break;
        }
        m_fadeAnimator.FadeIn(mainCanvasGroup, 0.5f);
        m_currentState = MenuState.MainView;
        mainCanvasGroup.interactable = true;
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Mansion_Main");
    }

    public void ViewOptions()
    {
        mainCanvasGroup.interactable = false;
        m_currentState = MenuState.OptionsView;

        m_fadeAnimator.FadeOut(mainCanvasGroup, 0.5f);
        m_fadeAnimator.FadeIn(optionsCanvasGroup, 0.5f);
        optionsCanvasGroup.interactable = true;
    }

    public void ViewCredits()
    {
        mainCanvasGroup.interactable = false;
        m_currentState = MenuState.CreditsView;

        m_fadeAnimator.FadeOut(mainCanvasGroup, 0.5f);
        m_fadeAnimator.FadeIn(creditsCanvasGroup, 0.5f);
        creditsCanvasGroup.interactable = true;
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
