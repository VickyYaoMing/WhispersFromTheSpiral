using UnityEngine;

public class PlayerLight : MonoBehaviour
{
    [Header("GameObjects")]
    [SerializeField] private GameObject m_playerLightObj;
    //[SerializeField] private GameObject m_lampPickupObj;

    [Header("Light Settings")]
    [SerializeField] private float m_minIntensity;
    [SerializeField] private float m_maxIntensity;
    [SerializeField] private float m_minFlickerCooldown;
    [SerializeField] private float m_maxFlickerCooldown;

    public bool HasFoundLamp;

    private Light m_playerLight;
    private bool m_isLampActive;
    private float m_timer;

    #region Unity Method
    private void Awake()
    {
        if(m_playerLightObj.GetComponentInChildren<Light>() != null)
        {
            m_playerLight = m_playerLightObj.GetComponentInChildren<Light>();
        }
        m_isLampActive = false;
        m_playerLightObj.SetActive(false);
    }
    private void Update()
    {
        if (!m_isLampActive) return;

        m_timer += Time.deltaTime;
        if (!(m_timer >= Random.Range(m_minFlickerCooldown, m_maxFlickerCooldown))) return;
        m_playerLight.intensity = Random.Range(m_minIntensity, m_maxIntensity);
        m_timer = 0;
    }
    #endregion

    public void ToggleLight()
    {
        if (!HasFoundLamp) return;
        m_isLampActive = !m_isLampActive;
        if (m_isLampActive)
        {
            m_playerLightObj.SetActive(true);
        }
        else
        {
            m_playerLightObj.SetActive(false);
        }
    }
}
