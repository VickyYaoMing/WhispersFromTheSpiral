using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotebookSlot : MonoBehaviour
{
    [SerializeField] Sprite DefaultSlotSprite;
    public CollectibleData GetData { get { return m_dataInNotebookSlot; } }

    private string DefaultDescription = "";
    private CollectibleData m_dataInNotebookSlot;
    private int m_indexInNotebook;
    private Image m_notebookSlotSprite;

    #region Unity Methods
    public void Start()
    {
        m_notebookSlotSprite = GetComponent<Image>();
    }
    #endregion

    public void Init(int index)
    {
       m_notebookSlotSprite.sprite = DefaultSlotSprite;
       m_indexInNotebook = index;
    }

    public void UpdateCollectible(CollectibleData data)
    {
        m_dataInNotebookSlot = data;
        m_notebookSlotSprite.sprite = data.SpriteInNotebook;
    }

}
