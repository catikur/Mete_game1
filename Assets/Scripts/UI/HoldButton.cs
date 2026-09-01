using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MeteGame.UI
{
    /// <summary>
    /// Basılı tutulduğu sürece aktif olan dokunmatik buton (gaz, geri, korna).
    /// Basılıyken hafifçe parlar.
    /// </summary>
    public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public System.Action<bool> StateChanged;

        Image _image;
        float _normalAlpha;
        int _pointerId = int.MinValue;
        bool _pressed;

        void Awake()
        {
            _image = GetComponent<Image>();
            if (_image != null)
                _normalAlpha = _image.color.a;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_pointerId != int.MinValue)
                return;
            _pointerId = eventData.pointerId;
            SetPressed(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != _pointerId)
                return;
            Release();
        }

        void OnDisable() => Release();

        void Release()
        {
            _pointerId = int.MinValue;
            SetPressed(false);
        }

        void SetPressed(bool pressed)
        {
            if (_pressed == pressed)
                return;
            _pressed = pressed;
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
