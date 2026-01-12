using UnityEngine;

public class LampObject : SecondaryInteractionItem
{
    private GameObject m_playerObj;
    private PlayerLight m_playerLightComponent;

    #region Unity Methods
    private void Awake()
    {
        m_playerObj = GameManager.Instance.Player.gameObject;
        if (m_playerObj != null)
        {
            m_playerLightComponent = m_playerObj.GetComponent<PlayerLight>();
        }

        GameManager.Instance.Lantern = this;
    }
    #endregion
    public override void SecondaryInteraction()
    {
        if (m_playerLightComponent != null)
        {
            m_playerLightComponent.HasFoundLamp = true;
        }
        gameObject.transform.position = new Vector3(0, -100, 0);
    }

    public void Save(ref LanternSaveData data)
    {
        data.lampHasBeenFound = m_playerLightComponent.HasFoundLamp;
        if(data.lampHasBeenFound) Debug.Log("lamp found");
        Debug.Log("lamp not found");
    }
    
    public void Load(LanternSaveData data)
    {
        if (!data.lampHasBeenFound) return;

        gameObject.transform.position = new Vector3(0, -100, 0);
    }
}

public struct LanternSaveData
{
    public bool lampHasBeenFound;
}
