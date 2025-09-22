using UnityEngine;

public class UIManager : MonoBehaviour
{
    //add inventory
    [SerializeField] Canvas m_playerUICanvas;
    [SerializeField] CanvasGroup m_pauseMenu;
    [SerializeField] CanvasGroup m_viewInventory;
    [SerializeField] CanvasGroup m_viewCollectible;
    FadeAnimator m_FadeAnimator;
    bool m_isPaused;
    bool m_isInventoryActive;

    #region Unity Methods
    void Awake()
    {
        m_FadeAnimator = GetComponent<FadeAnimator>();
    }

    private void Start()
    {
        m_pauseMenu.alpha = 0.0f;
        m_viewInventory.alpha = 0.0f;
        m_viewCollectible.alpha = 0.0f;
        m_isPaused = false;
    }
    #endregion 

    public void Pause()
    {
        m_isPaused = !m_isPaused;

        if (m_isPaused)
        {
            m_FadeAnimator.FadeIn(m_pauseMenu, 1.0f);
            m_pauseMenu.interactable = true;
        }
        else
        {
            m_pauseMenu.interactable = false;
            m_FadeAnimator.FadeOut(m_pauseMenu, 1.0f);        
        }
    }

    public void ToggleInventory()
    {
        m_isInventoryActive = !m_isInventoryActive;

        if (m_isInventoryActive)
        {
            m_FadeAnimator.FadeIn(m_viewInventory, 1.0f);
            m_viewInventory.interactable = true;
        }
        else
        {
            m_viewInventory.interactable = false;
            m_FadeAnimator.FadeOut(m_viewInventory, 1.0f);
        }
    }


}
