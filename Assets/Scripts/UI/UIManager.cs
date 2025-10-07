using TMPro;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Canvas m_playerUICanvas;
    [SerializeField] GameObject PauseMenu;
    [SerializeField] GameObject NotebookMenu;
    [SerializeField] GameObject CollectibleViewMenu;
    [SerializeField] GameObject DarkOverlay;
    public bool IsPaused { get { return m_isPaused; } }
    public bool IsNotebookActive { get { return m_isNotebookActive; } }
    public bool IsViewingCollectible { get { return m_isViewingCollectible; } }
    private CanvasGroup m_pauseGroup;
    private CanvasGroup m_notebookGroup;
    private CanvasGroup m_collectibleGroup;
    private CanvasGroup m_darkOverlayGroup;
    private readonly CanvasGroup[] m_canvasGroups = new CanvasGroup[4];
    private InteractionManager m_interactionManager;
    
    private FadeAnimator m_fadeAnimator;
    private Notebook m_notebook;
    private bool m_isPaused;
    private bool m_isNotebookActive;
    private bool m_isViewingCollectible;

    #region Unity Methods
    void Start()
    {
        m_fadeAnimator = GetComponent<FadeAnimator>();
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
        }
    }
    void OnEnable()
    {
        m_interactionManager = GetComponent<InteractionManager>();
        m_notebook = GetComponent<Notebook>();
        if (m_interactionManager != null) 
        { 
            m_interactionManager.OnCollectibleFound += m_notebook.AddCollectibleToNotebook;  
            m_interactionManager.OnCollectibleFound += ViewCollectible;  
        }
    }
    void OnDisable()
    {
        if (m_interactionManager != null) 
        { 
            m_interactionManager.OnCollectibleFound -= m_notebook.AddCollectibleToNotebook;
            m_interactionManager.OnCollectibleFound -= ViewCollectible;
        }
    }
    #endregion 

    public void Exit()
    {
        if (m_isViewingCollectible)
        {
            m_fadeAnimator.FadeOut(m_collectibleGroup, 0.1f);
            m_fadeAnimator.FadeOut(m_darkOverlayGroup, 0.5f);
            m_isViewingCollectible = false;
            return;
        }

        if (m_isNotebookActive)
        {
            m_fadeAnimator.FadeOut(m_notebookGroup, 0.1f);
            m_fadeAnimator.FadeOut(m_darkOverlayGroup, 0.5f);
            m_isNotebookActive = false;
            return;
        }

        m_isPaused = !m_isPaused;

        if (m_isPaused)
        {
            m_fadeAnimator.FadeIn(m_darkOverlayGroup, 0.5f);
            m_fadeAnimator.FadeIn(m_pauseGroup, 0.5f);
            m_pauseGroup.interactable = true;
        }
        else
        {
            m_pauseGroup.interactable = false;
            m_fadeAnimator.FadeOut(m_darkOverlayGroup, 0.5f);
            m_fadeAnimator.FadeOut(m_pauseGroup, 0.5f);        
        }
    }

    public void ToggleNotebook()
    {
        if (m_isViewingCollectible) { return; }

        if (m_isPaused) {  return; }

        m_isNotebookActive = !m_isNotebookActive;

        if (m_isNotebookActive)
        {
            m_fadeAnimator.FadeIn(m_darkOverlayGroup, 0.5f);
            m_fadeAnimator.FadeIn(m_notebookGroup, 0.5f);
            m_notebookGroup.interactable = true;
        }
        else
        {
            m_notebookGroup.interactable = false;
            m_fadeAnimator.FadeOut(m_notebookGroup, 0.5f);
            m_fadeAnimator.FadeOut(m_darkOverlayGroup, 0.5f);
        }
    }

    private void ViewCollectible(GameObject collectible)
    {
        m_isViewingCollectible = true;
        if (m_isViewingCollectible)
        {
            CollectibleViewMenu.GetComponent<Image>().sprite = collectible.GetComponent<CollectibleItem>().SpriteInWorld;
            TextMeshProUGUI descriptionText = CollectibleViewMenu.GetComponentInChildren<TextMeshProUGUI>();
            descriptionText.text = collectible.GetComponent<CollectibleItem>().Description.text;
            m_fadeAnimator.FadeIn(m_collectibleGroup, 0.5f);
            m_fadeAnimator.FadeIn(m_darkOverlayGroup, 0.5f);
        }
    }
}
