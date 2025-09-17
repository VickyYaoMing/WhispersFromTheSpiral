using UnityEngine;

public class MainMenuNavigation : MonoBehaviour
{
    [SerializeField] CanvasGroup mainCanvasGroup;
    [SerializeField] CanvasGroup optionsCanvasGroup;
    [SerializeField] CanvasGroup creditsCanvasGroup;
    private MainMenuAnimator m_menuAnimator;
    private MenuState m_currentState;

    enum MenuState
    {
        MainView, OptionsView, CreditsView
    }

    #region Unity Functions
    private void Awake()
    {
        m_menuAnimator = GetComponent<MainMenuAnimator>();
    }

    void Start()
    {
        optionsCanvasGroup.alpha = 0f;
        creditsCanvasGroup.alpha = 0f;
        m_menuAnimator.FadeIn(mainCanvasGroup, 2f);
    }
    #endregion

    public void ReturnToMenu()
    {
        optionsCanvasGroup.interactable = false;
        creditsCanvasGroup.interactable = false;

        switch (m_currentState)
        {
            case MenuState.OptionsView:
                m_menuAnimator.FadeOut(optionsCanvasGroup, 0.5f);
                break;
            case MenuState.CreditsView:
                m_menuAnimator.FadeOut(creditsCanvasGroup, 0.5f);
                break;
        }
        m_menuAnimator.FadeIn(mainCanvasGroup, 0.5f);
        m_currentState = MenuState.MainView;
        mainCanvasGroup.interactable = true;
    }

    public void StartGame()
    {
        Debug.Log("Start button pressed, would load the game but there isn't one :(");
    }

    public void ViewOptions()
    {
        mainCanvasGroup.interactable = false;
        m_currentState = MenuState.OptionsView;

        m_menuAnimator.FadeOut(mainCanvasGroup, 0.5f);
        m_menuAnimator.FadeIn(optionsCanvasGroup, 0.5f);
        optionsCanvasGroup.interactable = true;
    }

    public void ViewCredits()
    {
        mainCanvasGroup.interactable = false;
        m_currentState = MenuState.CreditsView;

        m_menuAnimator.FadeOut(mainCanvasGroup, 0.5f);
        m_menuAnimator.FadeIn(creditsCanvasGroup, 0.5f);
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
