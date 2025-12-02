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
    }
    #endregion
    public override void SecondaryInteraction()
    {
        if (m_playerLightComponent != null)
        {
            m_playerLightComponent.HasFoundLamp = true;
        }
        Destroy(gameObject);
    }
}
