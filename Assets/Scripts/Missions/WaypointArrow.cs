using MeteGame.Core;
using UnityEngine;

namespace MeteGame.Missions
{
    /// <summary>
    /// Aracın üstünde duran büyük 3D ok — kuş bakışı kamerada hedef yönünü net gösterir.
    /// </summary>
    public class WaypointArrow : MonoBehaviour
    {
        Transform _vehicle;
        Vector3? _target;

        public static WaypointArrow Attach(Transform vehicle)
        {
            var go = new GameObject("WaypointArrow");
            var arrow = go.AddComponent<WaypointArrow>();
            arrow._vehicle = vehicle;
            arrow.BuildVisual();
            go.SetActive(false);
            return arrow;
        }

        public void SetTarget(Vector3? target)
        {
            _target = target;
            gameObject.SetActive(target.HasValue);
        }

        void BuildVisual()
        {
            Color fill = new Color(1f, 0.9f, 0.15f);
            Color edge = new Color(0.12f, 0.08f, 0f);

            // Üstten bakınca V şeklinde şerit — GTA 2 kamerasında ok gibi okunur.
            BuildArm("EdgeL", new Vector3(-0.85f, 0f, 0.35f), 38f, new Vector3(0.85f, 0.28f, 3.4f), edge);
            BuildArm("EdgeR", new Vector3(0.85f, 0f, 0.35f), -38f, new Vector3(0.85f, 0.28f, 3.4f), edge);
            BuildArm("ArmL", new Vector3(-0.72f, 0.12f, 0.35f), 38f, new Vector3(0.55f, 0.32f, 3.0f), fill);
            BuildArm("ArmR", new Vector3(0.72f, 0.12f, 0.35f), -38f, new Vector3(0.55f, 0.32f, 3.0f), fill);
        }

        void BuildArm(string name, Vector3 pos, float yaw, Vector3 scale, Color color)
        {
            var arm = PartFactory.Create(PrimitiveType.Cube, name, transform, pos, scale, color);
            arm.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        }

        void LateUpdate()
        {
            if (_vehicle == null || !_target.HasValue)
                return;

            Vector3 dir = _target.Value - _vehicle.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.05f)
                return;

            float bob = 4.6f + Mathf.Sin(Time.time * 4f) * 0.35f;
            transform.position = _vehicle.position + Vector3.up * bob;
            transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
            transform.localScale = Vector3.one * (1.15f + Mathf.Sin(Time.time * 5f) * 0.08f);
        }
    }
}
