using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MeteGame.UI
{
    /// <summary>
    /// Basılı tutulduğu sürece aktif olan dokunmatik buton (direksiyon ve geri vites için).
    /// Basılıyken hafifçe parlar.
    /// </summary>
    public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public System.Action<bool> StateChanged;

        Image _image;
        float _normalAlpha;

        void Awake()
        {
            _image = GetComponent<Image>();
            if (_image != null)
                _normalAlpha = _image.color.a;
        }

        public void OnPointerDown(PointerEventData eventData) => SetPressed(true);

        public void OnPointerUp(PointerEventData eventData) => SetPressed(false);

        void OnDisable() => SetPressed(false);

        void SetPressed(bool pressed)
        {
            StateChanged?.Invoke(pressed);
            if (_image != null)
            {
                var color = _image.color;
                color.a = pressed ? Mathf.Min(1f, _normalAlpha + 0.3f) : _normalAlpha;
                _image.color = color;
            }
        }
    }
}
