using MeteGame.City;
using MeteGame.Core;
using UnityEngine;

namespace MeteGame.Traffic
{
    /// <summary>
    /// Kaldırımda dolaşan, yeşil yanınca karşıya geçen yaya.
    /// Oyuncu yaklaşınca kenara zıplar; çarpınca oyun bitmez.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Pedestrian : MonoBehaviour
    {
        TrafficSystem _system;
        CityLayout _layout;
        Transform _player;
        Rigidbody _body;

        int _blockI;
        int _blockJ;
        int _corner; // 0..3, hedef köşe
        bool _crossing;
        Vector3 _crossFrom;
        Vector3 _crossTo;
        float _crossT;
        float _hop;

        readonly Vector3[] _corners = new Vector3[4];

        public void Init(TrafficSystem system, CityLayout layout, Transform player, int blockI, int blockJ, int corner)
        {
            _system = system;
            _layout = layout;
            _player = player;
            _blockI = blockI;
            _blockJ = blockJ;
            _corner = corner;
            _body = GetComponent<Rigidbody>();
            _layout.BlockWalkCorners(_blockI, _blockJ, _corners);
        }

        public void Startle()
        {
            _hop = 1f;
        }

        void FixedUpdate()
        {
            if (_system == null)
                return;

            float dt = Time.fixedDeltaTime;
            Vector3 target = _crossing ? Vector3.Lerp(_crossFrom, _crossTo, _crossT) : _corners[_corner];
            Vector3 pos = transform.position;
            Vector3 to = target - pos;
            to.y = 0f;

            Vector3 dodge = Vector3.zero;
            if (_player != null)
            {
                Vector3 away = pos - _player.position;
                away.y = 0f;
                float d = away.magnitude;
                if (d < 6f && d > 0.01f)
                {
                    dodge = away.normalized * Mathf.Lerp(3.2f, 0f, d / 6f);
                    if (d < 4.5f)
                        _hop = Mathf.Max(_hop, 0.7f);
                }
            }

            Vector3 dir = to.sqrMagnitude > 0.04f ? to.normalized : transform.forward;
            dir += dodge;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f)
                dir = transform.forward;

            float speed = GameConfig.PedestrianSpeed + dodge.magnitude * 0.5f;
            Quaternion rot = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(dir.normalized, Vector3.up), 1f - Mathf.Exp(-10f * dt));

            _hop = Mathf.MoveTowards(_hop, 0f, dt * 2.4f);
            float y = 0.55f + Mathf.Sin(Mathf.Clamp01(_hop) * Mathf.PI) * 0.55f;

            Vector3 next = pos + dir.normalized * speed * dt;
            next.y = y;
            _body.MovePosition(next);
            _body.MoveRotation(rot);

            if (_crossing)
            {
                _crossT += speed * dt / Mathf.Max(1f, Vector3.Distance(_crossFrom, _crossTo));
                if (_crossT >= 1f)
                    _crossing = false;
                return;
            }

            if (to.magnitude < 0.45f)
                ArriveAtCorner();
        }

        void ArriveAtCorner()
        {
            // Köşeden komşu bloğa geçmeyi dene; olmazsa döngüde ilerle.
            if (Random.value < 0.35f && TryStartCrossing())
                return;

            _corner = (_corner + 1) % 4;
        }

        bool TryStartCrossing()
        {
            // Köşe 0 GB, 1 GD, 2 KD, 3 KB.
            // 0 ve 1 güney kenar → güneye (j-1) veya 0/3 batı kenar → i-1, vs.
            Compass walk;
            int ni = _blockI;
            int nj = _blockJ;
            int arriveCorner = _corner;

            switch (_corner)
            {
                case 0: // GB: güney veya batı
                    if (Random.value < 0.5f) { walk = Compass.South; nj--; arriveCorner = 3; }
                    else { walk = Compass.West; ni--; arriveCorner = 1; }
                    break;
                case 1: // GD
                    if (Random.value < 0.5f) { walk = Compass.South; nj--; arriveCorner = 2; }
                    else { walk = Compass.East; ni++; arriveCorner = 0; }
                    break;
                case 2: // KD
                    if (Random.value < 0.5f) { walk = Compass.North; nj++; arriveCorner = 1; }
                    else { walk = Compass.East; ni++; arriveCorner = 3; }
                    break;
                default: // KB
                    if (Random.value < 0.5f) { walk = Compass.North; nj++; arriveCorner = 0; }
                    else { walk = Compass.West; ni--; arriveCorner = 2; }
                    break;
            }

            if (ni < 0 || nj < 0 || ni >= GameConfig.CityBlocks || nj >= GameConfig.CityBlocks)
                return false;
            if (!_system.PedestriansMayWalk(walk))
                return false;

            _crossFrom = _corners[_corner];
            _blockI = ni;
            _blockJ = nj;
            _layout.BlockWalkCorners(_blockI, _blockJ, _corners);
            _corner = arriveCorner;
            _crossTo = _corners[_corner];
            _crossT = 0f;
            _crossing = true;
            return true;
        }

        public static Pedestrian Spawn(Transform parent, Color shirt)
        {
            var go = new GameObject("Pedestrian");
            go.transform.SetParent(parent, false);

            var rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            var capsule = go.AddComponent<CapsuleCollider>();
            capsule.center = new Vector3(0f, 0.15f, 0f);
            capsule.radius = 0.28f;
            capsule.height = 1.2f;

            Color skin = new Color(0.96f, 0.80f, 0.64f);
            Color pants = new Color(0.28f, 0.32f, 0.45f);
            PartFactory.Create(PrimitiveType.Capsule, "Body", go.transform,
                new Vector3(0f, 0.05f, 0f), new Vector3(0.45f, 0.45f, 0.45f), shirt);
            PartFactory.Create(PrimitiveType.Sphere, "Head", go.transform,
                new Vector3(0f, 0.72f, 0f), new Vector3(0.42f, 0.42f, 0.42f), skin);
            PartFactory.Create(PrimitiveType.Cylinder, "Legs", go.transform,
                new Vector3(0f, -0.28f, 0f), new Vector3(0.32f, 0.22f, 0.32f), pants);

            return go.AddComponent<Pedestrian>();
        }
    }
}
