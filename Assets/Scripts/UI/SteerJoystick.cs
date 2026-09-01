using MeteGame.Controls;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MeteGame.UI
{
    /// <summary>
    /// Sağ alt şeffaf yön joystick'i. Kamera kuzeyi sabit olduğu için
    /// çubuk yukarı = kuzey, sağ = doğu: araç burnu o yöne döner.
    /// İleride aynı yere direksiyon konabilir.
    /// </summary>
    public class SteerJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        RectTransform _range;
        RectTransform _knob;
        float _travel;
        int _pointerId = int.MinValue;
        Canvas _canvas;

        public static SteerJoystick Build(Transform parent)
        {
            const float size = 360f;
            const float knobSize = 140f;

            var range = UIFactory.CreateIcon("SteerJoystick", parent,
                new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-214f, 214f), new Vector2(size, size),
                UIFactory.CircleSprite, new Color(1f, 1f, 1f, 0.18f));
            range.raycastTarget = true;

            // Dört yön işareti: ekran yukarı/aşağı/sağ/sol = dünya kuzey/güney/doğu/batı.
            PlacePip(range.transform, 0f, 122f, 0f);
            PlacePip(range.transform, 0f, -122f, 180f);
            PlacePip(range.transform, 122f, 0f, -90f);
            PlacePip(range.transform, -122f, 0f, 90f);

            var knob = UIFactory.CreateIcon("Knob", range.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(knobSize, knobSize),
                UIFactory.CircleSprite, new Color(1f, 1f, 1f, 0.42f));
            knob.raycastTarget = false;

            var stick = range.gameObject.AddComponent<SteerJoystick>();
            stick._range = range.rectTransform;
            stick._knob = knob.rectTransform;
            stick._travel = (size - knobSize) * 0.5f;
            stick._canvas = parent.GetComponentInParent<Canvas>();
            return stick;
        }

        static void PlacePip(Transform parent, float x, float y, float zRot)
        {
            var pip = UIFactory.CreateIcon("Pip", parent,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(x, y), new Vector2(36f, 36f),
                UIFactory.ChevronSprite, new Color(1f, 1f, 1f, 0.28f));
            pip.raycastTarget = false;
            pip.rectTransform.localEulerAngles = new Vector3(0f, 0f, zRot);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_pointerId != int.MinValue)
                return;
            _pointerId = eventData.pointerId;
            Apply(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != _pointerId)
                return;
            Apply(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != _pointerId)
                return;
            Release();
        }

        void OnDisable() => Release();

        void Apply(PointerEventData eventData)
        {
            Camera cam = null;
            if (_canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                cam = _canvas.worldCamera;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _range, eventData.position, cam, out Vector2 local))
                return;

            if (local.magnitude > _travel)
                local = local.normalized * _travel;

            _knob.anchoredPosition = local;
            DriveInput.TouchStick = local / _travel;
        }

        void Release()
        {
            _pointerId = int.MinValue;
            if (_knob != null)
                _knob.anchoredPosition = Vector2.zero;
            DriveInput.TouchStick = Vector2.zero;
        }
    }
}
