using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
    [SerializeField] private float m_minIntensity;
    [SerializeField] private float m_maxIntensity;
    [SerializeField] private float m_minFlickerCooldown;
    [SerializeField] private float m_maxFlickerCooldown;

    private Light m_light;
    private float m_timer;

    #region Unity Methods
    private void Start()
    {
        m_light = GetComponent<Light>();
    }
    private void Update()
    {
        m_timer += Time.deltaTime;
        if(!(m_timer >= Random.Range(m_minFlickerCooldown, m_maxFlickerCooldown))) { return; }
        m_light.intensity = Random.Range(m_minIntensity, m_maxIntensity);
        m_timer = 0;
    }
    #endregion


}
