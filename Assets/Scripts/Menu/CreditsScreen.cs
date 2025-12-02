using Assets.Scripts.AudioSystem;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsScreen : MonoBehaviour
{
    [SerializeField] GameObject creditsObject;

    private CanvasGroup m_creditsGroup;
    private FadeAnimator m_fadeAnimator;
    private readonly float scrollSpeed = 100f;     
    private readonly float scrollDistance = 5000f;
    private Vector3 startPos;

    void Start()
    {
        m_creditsGroup = GetComponent<CanvasGroup>();
        m_fadeAnimator = GetComponent<FadeAnimator>();

        m_creditsGroup.alpha = 0.0f;
        startPos = creditsObject.transform.localPosition;

        SoundManager.PlayMusic(SoundType.UI_MenuTheme, 0f);
        StartCoroutine(ScrollCredits());
    }

    IEnumerator ScrollCredits()
    {
        m_fadeAnimator.FadeIn(m_creditsGroup, 1f);

        yield return new WaitForSeconds(2f);

        float totalDistanceMoved = 0f;

        while (totalDistanceMoved < scrollDistance)
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                ReturnToMainMenu();
            }
            float amountToMove = scrollSpeed * Time.deltaTime;
            totalDistanceMoved += amountToMove;

            creditsObject.transform.localPosition += new Vector3(0, amountToMove, 0);
            yield return null;
        }
        ReturnToMainMenu();
    }

    private void ReturnToMainMenu()
    {
        SceneManager.LoadScene("System_MainMenu");
    }
}
