using UnityEngine;

public class Notebook : MonoBehaviour
{
    public delegate void CollectibleHandler(GameObject collectible);
    public event CollectibleHandler OnCollectibleFound;
    [SerializeField] GameObject NotebookObject;
    private GameObject[] NotebookSlots;
    private CollectibleData[] m_foundCollectibles;
    private readonly int m_maximumCollectibles = 24;
    private int m_currentIndex;
    private CollectibleData m_currentCollectibleData;

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

    public CollectibleData[] Get()
    {
        return m_foundCollectibles;
    }

    public int CurrentSize()
    {
        return m_currentIndex;
    }

}
