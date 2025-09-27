using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Canvas m_playerUICanvas;
    [SerializeField] GameObject PauseMenu;
    [SerializeField] GameObject NotebookMenu;
    [SerializeField] GameObject CollectibleViewMenu;
    [SerializeField] GameObject DarkOverlay;
    private CanvasGroup m_pauseGroup;
    private CanvasGroup m_notebookGroup;
    private CanvasGroup m_collectibleGroup;
    private CanvasGroup m_darkOverlayGroup;
    private CanvasGroup[] m_canvasGroups = new CanvasGroup[4];
    private InteractionManager m_interactionManager;
    
    private FadeAnimator m_FadeAnimator;
    private bool m_isPaused;
    private bool m_isNotebookActive;
    private bool m_isViewingCollectible;
    private CollectibleData m_currentCollectibleData;


    #region Unity Methods
    void Start()
    {
        m_FadeAnimator = GetComponent<FadeAnimator>();
        m_pauseGroup = PauseMenu.GetComponent<CanvasGroup>();
        m_notebookGroup = NotebookMenu.GetComponent<CanvasGroup>();
        m_collectibleGroup = CollectibleViewMenu.GetComponent<CanvasGroup>();
        m_darkOverlayGroup = DarkOverlay.GetComponent<CanvasGroup>();

        m_canvasGroups[0] = m_pauseGroup;
        m_canvasGroups[1] = m_notebookGroup;
        m_canvasGroups[2] = m_collectibleGroup;
        m_canvasGroups[3] = m_darkOverlayGroup;

        for (int i = 0; i < m_canvasGroups.Length; i++)
        {
            m_canvasGroups[i].alpha = 0.0f;
            m_canvasGroups[i].interactable = false;
            m_canvasGroups[i].blocksRaycasts = false;
        }
    }
    void OnEnable()
    {
        m_interactionManager = GetComponent<InteractionManager>();
        if (m_interactionManager != null) { m_interactionManager.OnCollectibleFound += CollectibleFound;  }
    }
    void OnDisable()
    {
        if (m_interactionManager != null) { m_interactionManager.OnCollectibleFound -= CollectibleFound; }
    }
    #endregion 

    public void Exit()
    {
        if (m_isViewingCollectible)
        {
            m_FadeAnimator.FadeOut(m_collectibleGroup, 0.1f);
            m_FadeAnimator.FadeOut(m_darkOverlayGroup, 0.5f);
            m_isViewingCollectible = false;
            return;
        }

        m_isPaused = !m_isPaused;

        if (m_isPaused)
        {
            m_FadeAnimator.FadeIn(m_darkOverlayGroup, 0.5f);
            m_FadeAnimator.FadeIn(m_pauseGroup, 0.5f);
            m_pauseGroup.interactable = true;
        }
        else
        {
            m_pauseGroup.interactable = false;
            m_FadeAnimator.FadeOut(m_darkOverlayGroup, 0.5f);
            m_FadeAnimator.FadeOut(m_pauseGroup, 0.5f);        
        }
    }

    public void ToggleNotebook()
    {
        if (m_isViewingCollectible) { return; }

        m_isNotebookActive = !m_isNotebookActive;

        if (m_isNotebookActive)
        {
            m_FadeAnimator.FadeIn(m_darkOverlayGroup, 0.5f);
            m_FadeAnimator.FadeIn(m_notebookGroup, 0.5f);
            m_notebookGroup.interactable = true;
        }
        else
        {
            m_notebookGroup.interactable = false;
            m_FadeAnimator.FadeOut(m_notebookGroup, 0.5f);
            m_FadeAnimator.FadeOut(m_darkOverlayGroup, 0.5f);
        }
    }

    private void ViewCollectible(GameObject collectible)
    {
        m_isViewingCollectible = true;
        if (m_isViewingCollectible)
        {
            CollectibleViewMenu.GetComponent<Image>().sprite = m_currentCollectibleData.SpriteInWorld;
            TextMeshProUGUI descriptionText = CollectibleViewMenu.GetComponentInChildren<TextMeshProUGUI>();
            descriptionText.text = m_currentCollectibleData.DescriptionText;
            m_FadeAnimator.FadeIn(m_collectibleGroup, 0.5f);
            m_FadeAnimator.FadeIn(m_darkOverlayGroup, 0.5f);
        }
    }

    private void CollectibleFound(GameObject collectible)
    {
        if (gameObject == null) { return; }

        m_currentCollectibleData = new()
        {
            SpriteInWorld = collectible.GetComponent<CollectibleItem>().SpriteInWorld,
            SpriteInNotebook = collectible.GetComponent<CollectibleItem>().SpriteInNotebook,
            DescriptionText = collectible.GetComponent<CollectibleItem>().Description.text,
        };
        ViewCollectible(collectible);
        collectible.GetComponent<CollectibleItem>().OnCollect();
    }
}
