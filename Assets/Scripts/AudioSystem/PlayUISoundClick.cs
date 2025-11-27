using UnityEngine;
using UnityEngine.EventSystems;
namespace Assets.Scripts.AudioSystem
{
    public class PlayUISoundClick : MonoBehaviour, IPointerDownHandler
    {
        public SoundType soundType = SoundType.UI_ClickButton;
        public void OnPointerDown(PointerEventData eventData)
        {
            SoundManager.PlayUI(soundType);
        }
    }
}