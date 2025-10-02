using UnityEngine;

public class Notebook : MonoBehaviour
{
    [SerializeField] GameObject NotebookObject;
    private GameObject[] NotebookSlots;
    private CollectibleData[] m_foundCollectibles;
    private readonly int m_maximumCollectibles = 24;
    private int m_currentIndex;

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

    public void Add(CollectibleData data)
    {
        m_foundCollectibles[m_currentIndex] = data;
        NotebookSlots[m_currentIndex].GetComponent<NotebookSlot>().UpdateCollectible(data);
        m_currentIndex++;
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
