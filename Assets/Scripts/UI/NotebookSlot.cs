using UnityEngine;
using UnityEngine.UI;

public class NotebookSlot : MonoBehaviour
{
    [SerializeField] Notebook Owner;
    public string Description = "";

    private int m_indexInNotebook;
    private Image m_notebookSlotSprite;
    private Button m_slotButton;

    #region Unity Methods
    public void Start()
    {
        m_notebookSlotSprite = GetComponent<Image>();
        m_slotButton = GetComponent<Button>();
        m_slotButton.onClick.AddListener(() => { Owner.DisplayCollectibleInfo(this); });
    }
    #endregion

    public void Init(int index)
    {
       m_indexInNotebook = index;
    }

    public void UpdateCollectible(CollectibleData data)
    {
        m_notebookSlotSprite.sprite = data.SpriteInNotebook;
        Description = data.DescriptionText;
    }

    public int GetIndex()
    {
        return m_indexInNotebook;
    }

}
