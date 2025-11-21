using System;
using UnityEngine;

public class GrandFatherTrigger : MonoBehaviour
{
    [SerializeField] private LayerMask itemMask;
    [SerializeField] private float rayHitDistance;
    [SerializeField] private GameObject returnGameObject;
    [SerializeField] private AudioClip[] m_audioClips;

    private AudioSource m_source;
    private AudioClip m_ticking;
    private AudioClip m_dong;
    private Animator animator;
    private InteractionManager interactionManager;
    private bool hasClockBeenOpened = false;
    private bool hasGunBeenTaken = false;

    private void OnEnable()
    {
        ChessInteraction.ChessPuzzleCompleted += PuzzleTrigger;   
    }

    private void OnDisable()
    {
        ChessInteraction.ChessPuzzleCompleted -= PuzzleTrigger;

    }
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        interactionManager = GameManager.Instance.InteractionManager;
        m_source = GetComponent<AudioSource>();
        if(m_audioClips != null && m_audioClips.Length == 2)
        {
            m_ticking = m_audioClips[0];
            m_dong = m_audioClips[1];
        }
        m_source.resource = m_ticking;
        if (m_source != null ) { m_source.Play(); }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && hasClockBeenOpened && !hasGunBeenTaken)
        {
            GetComponent<BoxCollider>().enabled = false;
            Ray rayItem = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitItem;
           
            if (Physics.Raycast(rayItem, out hitItem, rayHitDistance, itemMask))
            {
                if (hitItem.collider.gameObject.CompareTag("ItemInsideSafe"))
                {
                    m_source.resource = m_dong;
                    m_source.volume = 1f;
                    m_source.Play();
                    m_source.loop = false;
                    hasGunBeenTaken = true;
                }
            }
        }
    }

    private void PuzzleTrigger(object e, EventArgs args)
    {
        Debug.Log("Grandfather clock triggered");
        animator.SetBool("winningCondition", true);
        hasClockBeenOpened = true;
        m_source.Stop();
    }
}
