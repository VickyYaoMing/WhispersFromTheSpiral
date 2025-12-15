using System;
using Assets.Scripts.AudioSystem;
using Unity.VisualScripting;
using UnityEngine;

public class GramophoneScript : MonoBehaviour
{
    [SerializeField] private SoundType soundType;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                SoundManager.PlayAt(soundType, transform.position, 1f);
            }
        }
    }
}
