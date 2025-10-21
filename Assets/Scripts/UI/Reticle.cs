using UnityEngine;
using UnityEngine.UI;

public enum ReticleState
{
    Default,
    InteractableItem
}

[RequireComponent(typeof(Image))]
public class Reticle : MonoBehaviour
{
    public static Reticle Instance;
    [SerializeField] private Sprite m_defaultSprite;
    [SerializeField] private Sprite m_interactableItemSprite;
    private Image m_cursorImage;

    #region Unity Methods
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        m_cursorImage = GetComponent<Image>();
        m_cursorImage.sprite = m_defaultSprite;
    }
    #endregion

    public void SetSprite(ReticleState state)
    {
        m_cursorImage.sprite = state switch
        {
            ReticleState.Default => m_defaultSprite,
            ReticleState.InteractableItem => m_interactableItemSprite,
            _ => m_defaultSprite,
        };
    }

    public void SetActivity(bool active)
    {
        if (active)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }

    }


}
