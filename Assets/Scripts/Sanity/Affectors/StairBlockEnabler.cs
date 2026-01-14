using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StairBlockEnabler : MonoBehaviour
{
    [SerializeField] private GameObject gameObject;
    public Vector3 positionToMoveTo;
    private void Start()
    {
        positionToMoveTo.y = gameObject.transform.position.y - 2f;
    }
    public IEnumerator MoveBars(Vector3 targetPosition, float duration)
    {
        float time = 0;
        Vector3 startPosition = gameObject.transform.position;
        while (time < duration)
        {
            gameObject.transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        gameObject.transform.position = targetPosition;
    }
}
