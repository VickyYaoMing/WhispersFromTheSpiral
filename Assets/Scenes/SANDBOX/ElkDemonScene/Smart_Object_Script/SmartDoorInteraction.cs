using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class SmartDoorInteraction : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private bool _locked;

    // IF: wanna add sound effects 
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip playerOpenSFX;
    [SerializeField] private AudioClip enemyOpenSFX;

    private bool isOpening = false;

    public void TryOpen(Transform opener, bool isEnemy = false)
    {
        if (_locked || isOpening)
            return;

        StartCoroutine(OpenDoorRoutine(opener, isEnemy));
    }

    private IEnumerator OpenDoorRoutine(Transform opener, bool isEnemy)
    {
        isOpening = true;

        Vector3 toOpener = opener.position - transform.position;
        float dot = Vector3.Dot(transform.forward, toOpener);
        bool fromFront = dot > 0;

        // Optional delay for enemy to "build weight"
        float delay = isEnemy ? 0.6f : 0f;
        yield return new WaitForSeconds(delay);

        // Adjust door animation speed for heavier effect
        _animator.speed = isEnemy ? 0.75f : 1f;

        //if (fromFront)
        //    _animator.SetTrigger("OpenDoorPos");
        //else
        //    _animator.SetTrigger("OpenDoorNeg");

        // Play sound effect 
        if (audioSource != null)
        {
            AudioClip clip = isEnemy ? enemyOpenSFX : playerOpenSFX;
            if (clip != null)
                audioSource.PlayOneShot(clip);
        }

        yield return new WaitForSeconds(2f);
        isOpening = false;
    }

    public void SetLocked(bool value)
    {
        _locked = value;
    }
}
