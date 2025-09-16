using System.Collections;
using UnityEngine;

public class MainMenuNavigation : MonoBehaviour
{
    [SerializeField] GameObject mainView;
    [SerializeField] GameObject optionsView;
    [SerializeField] GameObject creditsView;
    [SerializeField] CanvasGroup mainCanvasGroup;
    [SerializeField] CanvasGroup optionsCanvasGroup;
    [SerializeField] CanvasGroup creditsCanvasGroup;
    private float fadeDuration = 2.0f;

    enum MenuState
    {
        MainView, OptionsView, CreditsView
    }

    
    void Start()
    {
        ReturnToMenu();
    }

    public void ReturnToMenu()
    {
        mainView.SetActive(true);
        optionsView.SetActive(false);
        creditsView.SetActive(false);
        FadeIn(mainCanvasGroup);
    }

    public void StartGame()
    {
        Debug.Log("Start button pressed, would load the game but there isn't one :(");
    }

    public void ViewOptions()
    {
        FadeOut(mainCanvasGroup);
        mainView.SetActive(false);
   
        optionsView.SetActive(true);
        FadeIn(optionsCanvasGroup);
    }

    public void ViewCredits()
    {
        FadeOut(mainCanvasGroup);
        mainView.SetActive(false);

        creditsView.SetActive(true);
        FadeIn(creditsCanvasGroup);
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void FadeIn(CanvasGroup group)
    {
        StartCoroutine(FadeCanvasGroup(group, 0f, 1f));
    }

    private void FadeOut(CanvasGroup group)
    {
        StartCoroutine(FadeCanvasGroup(group, 1f, 0f));
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float startAlpha, float finalAlpha)
    {
        float elapsedTime = 0f;
        group.alpha = startAlpha;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            group.alpha = Mathf.Lerp(startAlpha, finalAlpha, elapsedTime / fadeDuration);
            yield return null;
        }

        group.alpha = finalAlpha;
    }


}
