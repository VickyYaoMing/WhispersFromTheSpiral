using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class CollectibleItem : SecondaryInteractionItem
{
    [SerializeField] Sprite[] m_sprites;
    public Sprite SpriteInWorld { get { return m_sprites[0]; } }
    public Sprite SpriteInNotebook { get { return m_sprites[1]; } }
    public Text Description { get; private set; }
    public string[] DescriptionAsPages { get; private set; }
    public bool hasBeenCollected { get; private set; }

    #region Unity Methods
    private void Start()
    {
        Description = GetComponent<Text>();
        IsCollectible = true;
        DescriptionAsPages = SplitDescription(Description.text);
    }
    #endregion

    public override void SecondaryInteraction()
    {
        OnCollect();
    }

    public void OnCollect()
    {
        hasBeenCollected = true;
        Destroy(gameObject);
    }

    private string[] SplitDescription(string description)
    {
        List<string> pages = new();
        string[] lines = description.Split(new[] { "\n\n" }, StringSplitOptions.None);
        foreach (string line in lines)
        {
            pages.Add(line);
        }
        return pages.ToArray();
    }

    public void Save(ref CollectibleSaveData saveData)
    {
        saveData.hasBeenCollected = hasBeenCollected;
    }

}