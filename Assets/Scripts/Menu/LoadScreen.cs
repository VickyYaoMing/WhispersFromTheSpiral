using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadScreen : MonoBehaviour
{
    private CanvasGroup m_logoGroup;
    private Image m_logoImage; 
    private FadeAnimator m_FadeAnimator;
    void Start()
    {
        m_FadeAnimator = GetComponent<FadeAnimator>();
        m_logoGroup = GetComponentInChildren<CanvasGroup>();
        m_logoImage = GetComponentInChildren<Image>();
        m_logoGroup.alpha = 0;
        StartCoroutine(ChangeToMenuScene());
    }

    IEnumerator ChangeToMenuScene()
    {
        m_FadeAnimator.FadeIn(m_logoGroup, 1f);
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("System_MainMenu");
    }
}
