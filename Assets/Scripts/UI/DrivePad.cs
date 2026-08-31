using MeteGame.Controls;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MeteGame.UI
{
    /// <summary>
    /// Tam ekran sürüş yüzeyi: basılı tut = gaz, bırak = fren,
    /// sağa/sola kaydır = direksiyon. iPhone/iPad için tek parmak.
    /// </summary>
    public class DrivePad : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        int _pointerId = int.MinValue;
        float _originX;

        public static DrivePad Build(Transform parent)
        {
            var rect = UIFactory.CreateRect("DrivePad", parent,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.SetAsFirstSibling();

            var image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0f);
            image.raycastTarget = true;

            return rect.gameObject.AddComponent<DrivePad>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_pointerId != int.MinValue)
                return;

            _pointerId = eventData.pointerId;
            _originX = eventData.position.x;
            DriveInput.TouchThrottle = true;
            DriveInput.TouchSteer = 0f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != _pointerId)
                return;

            float span = Mathf.Max(180f, Screen.width * 0.16f);
            DriveInput.TouchSteer = Mathf.Clamp((eventData.position.x - _originX) / span, -1f, 1f);
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
            DriveInput.TouchThrottle = false;
            DriveInput.TouchSteer = 0f;
        }
    }
}
