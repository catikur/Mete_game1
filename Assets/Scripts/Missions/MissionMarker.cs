using MeteGame.Core;
using MeteGame.Vehicle;
using UnityEngine;

namespace MeteGame.Missions
{
    /// <summary>
    /// Hedef noktası işareti: yerde renkli halka + havada zıplayıp dönen ikon.
    /// Oyuncunun aracı halkaya girince Reached tetiklenir.
    /// </summary>
    public class MissionMarker : MonoBehaviour
    {
        public System.Action Reached;

        Transform _icon;
        float _iconBaseY;
        bool _consumed;

        public static MissionMarker Spawn(string name, Vector3 position, Color color, PrimitiveType iconShape)
        {
            var go = new GameObject(name);
            go.transform.position = position;

            PartFactory.Create(PrimitiveType.Cylinder, "Ring", go.transform,
                new Vector3(0f, 0.1f, 0f), new Vector3(7f, 0.06f, 7f), color, castShadows: false);

            var icon = PartFactory.Create(iconShape, "Icon", go.transform,
                new Vector3(0f, 3.2f, 0f), Vector3.one * 1.4f, color);
            icon.transform.localRotation = Quaternion.Euler(35f, 0f, 35f);

            var trigger = go.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 1.5f, 0f);
            trigger.radius = GameConfig.MarkerTriggerRadius;

            var marker = go.AddComponent<MissionMarker>();
            marker._icon = icon.transform;
            marker._iconBaseY = 3.2f;
            return marker;
        }

        void Update()
        {
            if (_icon == null)
                return;

            var position = _icon.localPosition;
            position.y = _iconBaseY + Mathf.Sin(Time.time * 3f) * 0.5f;
            _icon.localPosition = position;
            _icon.Rotate(0f, 90f * Time.deltaTime, 0f, Space.World);
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
