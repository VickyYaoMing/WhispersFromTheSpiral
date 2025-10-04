using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Notebook : MonoBehaviour
{
    [SerializeField] GameObject NotebookObject;
    [SerializeField] GameObject InspectorObject;
    [SerializeField] GameObject DescriptionObject;
    private GameObject[] NotebookSlots;
    private CollectibleData[] m_foundCollectibles;
    private readonly int m_maximumCollectibles = 24;
    private int m_currentIndex;
    private CollectibleData m_currentCollectibleData;
    private TextMeshProUGUI m_currentDescription;
     

    #region Unity Methods
    private void Start()
    {
        NotebookSlots = new GameObject[m_maximumCollectibles];
        m_foundCollectibles = new CollectibleData[m_maximumCollectibles];
        for (int i = 0; i < NotebookObject.transform.childCount; i++)
        {
            var slot = NotebookObject.transform.GetChild(i).GetComponent<NotebookSlot>();
            if (slot != null)
            {
                slot.Init(i);
                NotebookSlots[i] = NotebookObject.transform.GetChild(i).gameObject;
            }
        }
        m_currentDescription = DescriptionObject.GetComponent<TextMeshProUGUI>();
    }
    #endregion

    public void AddCollectibleToNotebook(GameObject collectible)
    {
        if (gameObject == null) { return; }

        m_currentCollectibleData = new()
        {
            SpriteInWorld = collectible.GetComponent<CollectibleItem>().SpriteInWorld,
            SpriteInNotebook = collectible.GetComponent<CollectibleItem>().SpriteInNotebook,
            DescriptionText = collectible.GetComponent<CollectibleItem>().Description.text,
        };
        m_foundCollectibles[m_currentIndex] = m_currentCollectibleData;
        NotebookSlots[m_currentIndex].GetComponent<NotebookSlot>().UpdateCollectible(m_currentCollectibleData);
        m_currentIndex++;
        collectible.GetComponent<CollectibleItem>().OnCollect();
    }

    public void DisplayCollectibleInfo(NotebookSlot slot)
    {
        int indexToView = slot.GetIndex();
        if (m_foundCollectibles[indexToView] == null) { return; }
        InspectorObject.GetComponent<Image>().sprite = m_foundCollectibles[indexToView].SpriteInWorld;
        m_currentDescription.text = m_foundCollectibles[indexToView].DescriptionText;
    }
}
