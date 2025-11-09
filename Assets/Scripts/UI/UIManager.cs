using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Canvas m_playerUICanvas;
    [SerializeField] GameObject PauseMenu;
    [SerializeField] GameObject NotebookMenu;
    [SerializeField] GameObject CollectibleViewMenu;
    
    private CanvasGroup m_pauseGroup;
    private CanvasGroup m_notebookGroup;
    private CanvasGroup m_collectibleGroup;
    private readonly CanvasGroup[] m_canvasGroups = new CanvasGroup[3];
    private InteractionManager m_interactionManager;

    private FadeAnimator m_fadeAnimator;
    private Notebook m_notebook;
    private GameObject m_currentCollectibleInView;
    private int m_currentCollectibleDescriptionIndex;
    private TextMeshProUGUI m_collectibleDescriptionText;
    
    private bool m_isPaused;
    private bool m_isNotebookActive;
    private bool m_isViewingCollectible;
    public bool IsPaused { get { return m_isPaused; } }
    public bool IsNotebookActive { get { return m_isNotebookActive; } }
    public bool IsViewingCollectible { get { return m_isViewingCollectible; } }

    #region Unity Methods
    private void Start()
    {
        m_fadeAnimator = GetComponent<FadeAnimator>();
        m_pauseGroup = PauseMenu.GetComponent<CanvasGroup>();
        m_notebookGroup = NotebookMenu.GetComponent<CanvasGroup>();
        m_collectibleGroup = CollectibleViewMenu.GetComponent<CanvasGroup>();
        m_collectibleDescriptionText = CollectibleViewMenu.GetComponentInChildren<TextMeshProUGUI>();

        m_canvasGroups[0] = m_pauseGroup;
        m_canvasGroups[1] = m_notebookGroup;
        m_canvasGroups[2] = m_collectibleGroup;

        for (int i = 0; i < m_canvasGroups.Length; i++)
        {
            m_canvasGroups[i].alpha = 0.0f;
            m_canvasGroups[i].interactable = false;
        }
    }
    private void OnEnable()
    {
        m_interactionManager = GetComponent<InteractionManager>();
        m_notebook = GetComponent<Notebook>();
        if (m_interactionManager != null) 
        { 
            m_interactionManager.OnCollectibleFound += m_notebook.AddCollectibleToNotebook;  
            m_interactionManager.OnCollectibleFound += ViewCollectible;  
        }
    }
    private void OnDisable()
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
            ExitCollectible();
        }
        else if (m_isNotebookActive)
        {
            m_fadeAnimator.FadeOut(m_notebookGroup, 0.1f);
            m_isNotebookActive = false;
            return;
        }
        else
        {
            TogglePause();
        }
    }

    public void ToggleNotebook()
    {
        if (m_isViewingCollectible) { return; }

        if (m_isPaused) {  return; }

        m_isNotebookActive = !m_isNotebookActive;

        if (m_isNotebookActive)
        {
            m_fadeAnimator.FadeIn(m_notebookGroup, 0.5f);
            m_notebookGroup.interactable = true;
        }
        else
        {
            m_notebookGroup.interactable = false;
            m_fadeAnimator.FadeOut(m_notebookGroup, 0.5f);
        }
    }

    private void ExitCollectible()
    {
        if (m_currentCollectibleDescriptionIndex < m_currentCollectibleInView.GetComponent<CollectibleItem>().DescriptionAsPages.Length - 1)
        {
            m_currentCollectibleDescriptionIndex++;
            m_collectibleDescriptionText.text = m_currentCollectibleInView.GetComponent<CollectibleItem>().DescriptionAsPages[m_currentCollectibleDescriptionIndex]; 
            return;
        }
        m_currentCollectibleDescriptionIndex = 0;
        m_currentCollectibleInView.GetComponent<CollectibleItem>().OnCollect();
        m_fadeAnimator.FadeOut(m_collectibleGroup, 0.1f);
        m_isViewingCollectible = false;
    }

    private void TogglePause()
    {
        m_isPaused = !m_isPaused;

        if (m_isPaused)
        {
            m_fadeAnimator.FadeIn(m_pauseGroup, 0.5f);
            m_pauseGroup.interactable = true;
        }
        else
        {
            m_pauseGroup.interactable = false;
            m_fadeAnimator.FadeOut(m_pauseGroup, 0.5f);
        }
    }

    private void ViewCollectible(GameObject collectible)
    {
        if (m_isViewingCollectible) { return; }
        m_isViewingCollectible = true;
        m_currentCollectibleInView = collectible;

        CollectibleViewMenu.transform.GetChild(0).GetComponent<Image>().sprite
            = collectible.GetComponent<CollectibleItem>().SpriteInWorld;
        CollectibleViewMenu.transform.GetChild(0).GetComponent<Image>().SetNativeSize();
        m_collectibleDescriptionText.text = collectible.GetComponent<CollectibleItem>().DescriptionAsPages[0];
        m_fadeAnimator.FadeIn(m_collectibleGroup, 0.5f);
    }
}