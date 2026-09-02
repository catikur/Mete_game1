using MeteGame.City;
using MeteGame.Core;
using MeteGame.Vehicle;
using UnityEngine;

namespace MeteGame.Missions
{
    /// <summary>
    /// Kaçan hırsız arabası: yolları takip eder, kırmızı ışığa uymaz.
    /// Yakalanınca durur. Şiddet / kaza yok — yakalama mesafeyledir.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class ThiefCar : MonoBehaviour
    {
        enum Mode { Cruise, Crossing }

        CityLayout _layout;
        Transform _player;
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

        public bool Caught { get; private set; }

        public static ThiefCar Spawn(CityLayout layout, Transform player, Vector3 preferred)
        {
            layout.SnapToLane(preferred, out Vector3 pos, out int destIx, out int destIz, out CardinalDir heading);
            var go = VehicleFactory.CreateThiefCar(pos, CardinalUtil.Rotation(heading));
            var thief = go.AddComponent<ThiefCar>();
            thief._layout = layout;
            thief._player = player;
            thief._destIx = destIx;
            thief._destIz = destIz;
            thief._heading = heading;
            thief._mode = Mode.Cruise;
            thief._body = go.GetComponent<Rigidbody>();
            thief._speed = GameConfig.ThiefSpeed;
            return thief;
        }

        public void StopFleeing()
        {
            Caught = true;
            _speed = 0f;
        }

        void FixedUpdate()
        {
            if (Caught || _layout == null)
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

            float desired = GameConfig.ThiefSpeed;
            _speed = Mathf.MoveTowards(_speed, desired, 12f * dt);

            Vector3 look = tangent.sqrMagnitude > 0.05f ? tangent.normalized : CardinalUtil.Forward(_heading);
            Quaternion want = Quaternion.LookRotation(look, Vector3.up);
            Quaternion rot = Quaternion.Slerp(transform.rotation, want, 1f - Mathf.Exp(-8f * dt));

            Vector3 next = pos + rot * Vector3.forward * _speed * dt;
            next.y = 0.5f;
            _body.MovePosition(next);
            _body.MoveRotation(rot);

            if (_mode == Mode.Cruise && dist < 1.8f)
                TryEnterIntersection();
        }

        void TryEnterIntersection()
        {
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
            CardinalDir[] ranked =
            {
                _heading,
                CardinalUtil.RightOf(_heading),
                CardinalUtil.LeftOf(_heading),
                CardinalUtil.Opposite(_heading)
            };

            int optionCount = 0;
            var options = new CardinalDir[4];
            for (int i = 0; i < 4; i++)
            {
                if (!_layout.HasRoad(_destIx, _destIz, ranked[i]))
                    continue;
                if (ranked[i] == CardinalUtil.Opposite(_heading) && optionCount > 0)
                    continue;
                options[optionCount++] = ranked[i];
            }

            if (optionCount == 0)
                return _heading;

            if (_player != null && Random.value < 0.6f)
            {
                Vector3 away = transform.position - _player.position;
                away.y = 0f;
                if (away.sqrMagnitude > 0.01f)
                {
                    away.Normalize();
                    CardinalDir best = options[0];
                    float bestDot = float.MinValue;
                    for (int i = 0; i < optionCount; i++)
                    {
                        float dot = Vector3.Dot(CardinalUtil.Forward(options[i]), away);
                        if (dot > bestDot)
                        {
                            bestDot = dot;
                            best = options[i];
                        }
                    }
                    return best;
                }
            }

            return options[Random.Range(0, optionCount)];
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
