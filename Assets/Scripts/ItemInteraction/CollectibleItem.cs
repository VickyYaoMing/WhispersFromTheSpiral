using UnityEngine;
using UnityEngine.UI;

public class CollectibleItem : InteractableBase
{
    [SerializeField] Sprite[] m_sprites;
    public Sprite SpriteInWorld { get { return m_sprites[0]; } }
    public Sprite SpriteInNotebook { get { return m_sprites[1]; } }
    public Text Description { get; private set; }

    #region Unity Methods
    private void Start()
    {
        Description = GetComponent<Text>();
        IsCollectible = true;
    }
    #endregion

    public void OnCollect()
    {
        Destroy(gameObject);
    }

}
