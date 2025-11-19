using System.Collections;
using UnityEngine;

public class CutsceneTrigger : MonoBehaviour
{
    public Player player;
    public GameObject cutsceneCamera;

    private void OnTriggerEnter(Collider other)
    {
        cutsceneCamera.SetActive(true);
        player.gameObject.SetActive(false);
    }

    IEnumerator FinishCutscene()
    {
        //Cutscene length. Surely there's a less hardcoded way to do this?
        yield return new WaitForSeconds(10);
        player.gameObject.SetActive(true);
        cutsceneCamera.SetActive(false);
    }

}
