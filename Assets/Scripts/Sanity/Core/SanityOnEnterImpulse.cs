using SanitySystem;
using UnityEngine;

public class SanityOnEnterImpulse : MonoBehaviour
{
    [SerializeField] private string _playerTag = "Player";
    [SerializeField] private GamePhaseManager gamePhaseManager;
    void Awake()
    {
        if (!gamePhaseManager)
        {
            gamePhaseManager = FindAnyObjectByType<GamePhaseManager>();
        }
    }
    void OnTriggerEnter(Collider _other)
    {
        if (_other.CompareTag(_playerTag))
        {
            return;
        }
        if (gamePhaseManager)
        {
            return;
        }
        gamePhaseManager.NextPhase();
    }
}
