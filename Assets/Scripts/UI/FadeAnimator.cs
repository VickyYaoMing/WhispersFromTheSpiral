using System.Collections;
using UnityEngine;

public class FadeAnimator : MonoBehaviour
{
    public void FadeIn(CanvasGroup group, float duration)
    {
        StartCoroutine(FadeCanvasGroup(group, 0f, 1f, duration));
    }

    public void FadeOut(CanvasGroup group, float duration)
    {
        StartCoroutine(FadeCanvasGroup(group, 1f, 0f, duration));
    }

    IEnumerator FadeCanvasGroup(CanvasGroup group, float startAlpha, float finalAlpha, float duration)
    {
        float elapsedTime = 0f;
        group.alpha = startAlpha;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            group.alpha = Mathf.Lerp(startAlpha, finalAlpha, elapsedTime / duration);
            yield return null;
        }

        group.alpha = finalAlpha;
    }
}
