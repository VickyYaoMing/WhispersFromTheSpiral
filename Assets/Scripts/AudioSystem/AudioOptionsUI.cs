using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.AudioSystem;

public class AudioOptionsUI : MonoBehaviour
{
    [Header("Sliders (0-1)")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    private void Start()
    {
        if (masterSlider != null)
        {
            masterSlider.value = 1f;
            masterSlider.onValueChanged.AddListener(OnMasterChanged);
        }
        if (musicSlider != null)
        {
            musicSlider.value = 1f;
            musicSlider.onValueChanged.AddListener(OnMusicChanged);
        }
        if (sfxSlider != null)
        {
            sfxSlider.value = 1f;
            sfxSlider.onValueChanged.AddListener(OnSfxChanged);
        }
    }
    private void OnDestroy()
    {
        if (masterSlider != null)
        {
            masterSlider.onValueChanged.RemoveListener(OnMasterChanged);
        }
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
        }
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
        }
    }
    private void OnMasterChanged(float value)
    {
        SoundManager.SetMasterVolume(value);
    }
    private void OnMusicChanged(float value)
    {
        SoundManager.SetMusicVolume(value);
    }
    private void OnSfxChanged(float value)
    {
        SoundManager.SetSfxVolume(value);
    }
}
