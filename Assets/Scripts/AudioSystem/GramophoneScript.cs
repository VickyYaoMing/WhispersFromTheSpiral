using System;
using Assets.Scripts.AudioSystem;
using Unity.VisualScripting;
using UnityEngine;

public class GramophoneScript : MonoBehaviour
{
    [SerializeField] private SoundType soundType = SoundType.None;
    [SerializeField] private KeyCode interactKey = KeyCode.Mouse0;

    private bool _playerInRange = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;
        }
    }

    private void Update()
    {
        if (!_playerInRange) return;

        if (Input.GetKeyDown(interactKey))
        {
            if (soundType != SoundType.None)
            {
                SoundManager.PlayAt(soundType, transform.position, 1f);
                Debug.Log("Gramophone playing: " + soundType);
            }
        }
    }
}
