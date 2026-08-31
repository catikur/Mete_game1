using MeteGame.City;
using MeteGame.Core;
using UnityEngine;

namespace MeteGame.Traffic
{
    /// <summary>
    /// Şerit takip eden NPC araç. Işığa uyar, öndeki araca/oyuncuya mesafe bırakır,
    /// kavşakta rastgele döner. Çarpışmada kaza yok — kinematic, oyuncu ona çarpınca yavaşlar.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class TrafficCar : MonoBehaviour
    {
        enum Mode { Cruise, Crossing }

        TrafficSystem _system;
        CityLayout _layout;
        Rigidbody _body;

        int _destIx;
        int _destIz;
        CardinalDir _heading;
        Mode _mode;
        float _speed;

        Vector3 _curveA;
        Vector3 _curveB;
        Vector3 _curveC;
        float _curveT;
        float _curveLength = 1f;

        public void Init(TrafficSystem system, CityLayout layout, int destIx, int destIz, CardinalDir heading)
        {
            _system = system;
            _layout = layout;
            _destIx = destIx;
            _destIz = destIz;
            _heading = heading;
            _mode = Mode.Cruise;
            _body = GetComponent<Rigidbody>();
        }

        void FixedUpdate()
        {
            if (_system == null)
                return;

            float dt = Time.fixedDeltaTime;
            Vector3 pos = transform.position;
            Vector3 target;
            Vector3 tangent;

            if (_mode == Mode.Crossing)
            {
                float step = Mathf.Max(_speed, 2f) * dt / _curveLength;
                _curveT = Mathf.Min(1f, _curveT + step);
                target = Bezier(_curveA, _curveB, _curveC, _curveT);
                tangent = BezierTangent(_curveA, _curveB, _curveC, _curveT);
                if (_curveT >= 0.995f)
                    _mode = Mode.Cruise;
            }
            else
            {
                target = _layout.StopLine(_destIx, _destIz, _heading);
                tangent = target - pos;
            }

            tangent.y = 0f;
            Vector3 flatPos = new Vector3(pos.x, 0.5f, pos.z);
            Vector3 flatTarget = new Vector3(target.x, 0.5f, target.z);
            float dist = Vector3.Distance(flatPos, flatTarget);

            float desired = DesiredSpeed(dist);
            _speed = Mathf.MoveTowards(_speed, desired, 10f * dt);

            Vector3 look = tangent.sqrMagnitude > 0.05f ? tangent.normalized : CardinalUtil.Forward(_heading);
            Quaternion want = Quaternion.LookRotation(look, Vector3.up);
            Quaternion rot = Quaternion.Slerp(transform.rotation, want, 1f - Mathf.Exp(-8f * dt));

            Vector3 next = pos + rot * Vector3.forward * _speed * dt;
            next.y = 0.5f;
            _body.MovePosition(next);
            _body.MoveRotation(rot);

            if (_mode == Mode.Cruise && dist < 1.7f && desired > 0.4f && _system.IsGreen(_heading))
                TryEnterIntersection();
        }

        float DesiredSpeed(float distToStop)
        {
            float desired = GameConfig.NpcSpeed;
            float gap = _system.GapAhead(transform.position, transform.forward, 12f, transform);
            if (gap < 10f)
                desired = Mathf.Min(desired, Mathf.Lerp(0f, GameConfig.NpcSpeed, Mathf.InverseLerp(3.2f, 10f, gap)));

            if (_mode == Mode.Cruise && !_system.IsGreen(_heading))
            {
                float room = Mathf.Max(0.4f, distToStop - 0.6f);
                desired = Mathf.Min(desired, Mathf.Sqrt(2f * 8f * room));
                if (distToStop < 1.6f)
                    desired = 0f;
            }

            return desired;
        }

        void TryEnterIntersection()
        {
            if (_system.GapAhead(transform.position, transform.forward, 6f, transform) < 3.5f)
                return;

            CardinalDir next = PickTurn();
            if (!_layout.TryAdvance(_destIx, _destIz, next, out int nx, out int nz))
                return;

            _curveA = transform.position;
            _curveC = _layout.ExitPoint(_destIx, _destIz, next);
            Vector3 center = _layout.IntersectionCenter(_destIx, _destIz) + Vector3.up * 0.5f;
            _curveB = center
                      + CardinalUtil.Right(_heading) * GameConfig.LaneOffset * 0.45f
                      + CardinalUtil.Right(next) * GameConfig.LaneOffset * 0.45f;
            _curveLength = Mathf.Max(4f, Vector3.Distance(_curveA, _curveC) * (next == _heading ? 1.05f : 1.35f));
            _curveT = 0f;
            _heading = next;
            _destIx = nx;
            _destIz = nz;
            _mode = Mode.Crossing;
        }

        CardinalDir PickTurn()
        {
            // Düz 50, sağ 30, sol 20 — yoksa mevcut olanlar arasında.
            CardinalDir[] ranked = { _heading, CardinalUtil.RightOf(_heading), CardinalUtil.LeftOf(_heading) };
            int[] weights = { 5, 3, 2 };
            int total = 0;
            for (int i = 0; i < 3; i++)
            {
                if (!_layout.HasRoad(_destIx, _destIz, ranked[i]))
                    weights[i] = 0;
                total += weights[i];
            }

            if (total == 0)
            {
                if (_layout.HasRoad(_destIx, _destIz, CardinalUtil.Opposite(_heading)))
                    return CardinalUtil.Opposite(_heading);
                return _heading;
            }

            int roll = Random.Range(0, total);
            for (int i = 0; i < 3; i++)
            {
                roll -= weights[i];
                if (roll < 0)
                    return ranked[i];
            }
            return _heading;
        }

        static Vector3 Bezier(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            float u = 1f - t;
            return u * u * a + 2f * u * t * b + t * t * c;
        }

        static Vector3 BezierTangent(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            return 2f * (1f - t) * (b - a) + 2f * t * (c - b);
        }
    }
}
