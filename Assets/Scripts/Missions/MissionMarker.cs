using MeteGame.Core;
using MeteGame.Vehicle;
using UnityEngine;

namespace MeteGame.Missions
{
    /// <summary>
    /// Hedef işareti: yerde geniş halka, ışık sütunu ve zıplayan büyük ikon.
    /// Oyuncunun aracı halkaya girince Reached tetiklenir.
    /// </summary>
    public class MissionMarker : MonoBehaviour
    {
        public System.Action Reached;

        Transform _icon;
        Transform _beam;
        float _iconBaseY;
        bool _consumed;

        public static MissionMarker Spawn(string name, Vector3 position, Color color, PrimitiveType iconShape)
        {
            var go = new GameObject(name);
            go.transform.position = position;

            PartFactory.Create(PrimitiveType.Cylinder, "RingOuter", go.transform,
                new Vector3(0f, 0.12f, 0f), new Vector3(12f, 0.08f, 12f), color, castShadows: false);
            PartFactory.Create(PrimitiveType.Cylinder, "RingInner", go.transform,
                new Vector3(0f, 0.16f, 0f), new Vector3(7.5f, 0.08f, 7.5f),
                Color.Lerp(color, Color.white, 0.45f), castShadows: false);

            var beam = PartFactory.Create(PrimitiveType.Cylinder, "Beam", go.transform,
                new Vector3(0f, 8f, 0f), new Vector3(1.4f, 8f, 1.4f),
                Color.Lerp(color, Color.white, 0.2f), castShadows: false);

            var icon = PartFactory.Create(iconShape, "Icon", go.transform,
                new Vector3(0f, 5.2f, 0f), Vector3.one * 2.4f, color);
            icon.transform.localRotation = Quaternion.Euler(35f, 0f, 35f);

            var trigger = go.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 1.5f, 0f);
            trigger.radius = GameConfig.MarkerTriggerRadius;

            var marker = go.AddComponent<MissionMarker>();
            marker._icon = icon.transform;
            marker._beam = beam.transform;
            marker._iconBaseY = 5.2f;
            return marker;
        }

        void Update()
        {
            if (_icon != null)
            {
                var position = _icon.localPosition;
                position.y = _iconBaseY + Mathf.Sin(Time.time * 3.2f) * 0.7f;
                _icon.localPosition = position;
                _icon.Rotate(0f, 110f * Time.deltaTime, 0f, Space.World);
            }

            if (_beam != null)
            {
                float pulse = 1f + Mathf.Sin(Time.time * 4f) * 0.12f;
                _beam.localScale = new Vector3(1.4f * pulse, 8f, 1.4f * pulse);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (_consumed)
                return;

            var rigidbody = other.attachedRigidbody;
            if (rigidbody != null && rigidbody.GetComponent<VehicleController>() != null)
            {
                _consumed = true;
                Reached?.Invoke();
            }
        }
    }
}
