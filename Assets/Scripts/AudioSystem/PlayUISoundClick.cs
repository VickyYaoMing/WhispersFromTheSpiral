using UnityEngine;
namespace Assets.Scripts.AudioSystem
{
    public class PlayUISoundClick : MonoBehaviour
    {
        public SoundType soundType = SoundType.UI_ClickButton;
        public void Play()
        {
            SoundManager.PlayUI(soundType);
        }
    }
}

