using System.Collections.Generic;
using MeteGame.City;
using MeteGame.Controls;
using MeteGame.Core;
using MeteGame.UI;
using MeteGame.Vehicle;
using UnityEngine;

namespace MeteGame.Traffic
{
    /// <summary>
    /// Trafik ışıkları, NPC araçlar ve yayalar. Tüm kavşaklar aynı fazı paylaşır
    /// (kuzey-güney yeşil, sonra doğu-batı) — çocuk için okunması kolay.
    /// </summary>
    public class TrafficSystem : MonoBehaviour
    {
        enum Phase { NsGreen, NsYellow, EwGreen, EwYellow }

        CityLayout _layout;
        Transform _player;
        VehicleController _playerVehicle;
        HudController _hud;

        Phase _phase = Phase.NsGreen;
        float _phaseTime;
        readonly List<TrafficCar> _cars = new List<TrafficCar>();
        readonly List<Pedestrian> _peds = new List<Pedestrian>();
        readonly List<MeshRenderer> _nsLamps = new List<MeshRenderer>();
        readonly List<MeshRenderer> _ewLamps = new List<MeshRenderer>();

        float _honkCooldown;
        AudioClip _honkClip;
        bool _courtesyGiven;

        public static TrafficSystem Spawn(Transform parent, CityLayout layout, VehicleController player, HudController hud)
        {
            var go = new GameObject("TrafficSystem");
            go.transform.SetParent(parent, false);
            var system = go.AddComponent<TrafficSystem>();
            system._layout = layout;
            system._player = player.transform;
            system._playerVehicle = player;
            system._hud = hud;
            system.BuildSignals();
            system.SpawnCars();
            system.SpawnPeds();
            system.ApplyLampColors();
            return system;
        }

        public bool IsGreen(CardinalDir heading)
        {
            bool ns = CardinalUtil.IsNorthSouth(heading);
            return ns ? _phase == Phase.NsGreen : _phase == Phase.EwGreen;
        }

        public bool IsRed(CardinalDir heading)
        {
            bool ns = CardinalUtil.IsNorthSouth(heading);
            if (ns) return _phase == Phase.EwGreen || _phase == Phase.EwYellow;
            return _phase == Phase.NsGreen || _phase == Phase.NsYellow;
        }

        public bool PedestriansMayWalk(CardinalDir walkDir) => IsGreen(walkDir);

        public float GapAhead(Vector3 pos, Vector3 forward, float maxDist, Transform ignore)
        {
            Vector3 fwd = forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.01f)
                return maxDist;
            fwd.Normalize();

            float best = maxDist;
            Consider(_player.position, _player);
            for (int i = 0; i < _cars.Count; i++)
            {
                if (_cars[i] == null)
                    continue;
                Consider(_cars[i].transform.position, _cars[i].transform);
            }

            return Mathf.Max(0f, best);

            void Consider(Vector3 other, Transform t)
            {
                if (t == ignore)
                    return;
                Vector3 d = other - pos;
                d.y = 0f;
                float along = Vector3.Dot(d, fwd);
                if (along < 2f || along > maxDist)
                    return;
                float lateral = (d - fwd * along).magnitude;
                if (lateral < 2.3f)
                    best = Mathf.Min(best, along - 2.4f);
            }
        }

        void Update()
        {
            TickLights();
            TickHonk();
            TickCourtesy();
        }

        void TickLights()
        {
            _phaseTime += Time.deltaTime;
            float limit = (_phase == Phase.NsGreen || _phase == Phase.EwGreen)
                ? GameConfig.LightGreenSeconds
                : GameConfig.LightYellowSeconds;

            if (_phaseTime < limit)
                return;

            _phaseTime = 0f;
            _phase = (Phase)(((int)_phase + 1) % 4);
            ApplyLampColors();
        }

        void ApplyLampColors()
        {
            Color ns = ColorForNs();
            Color ew = ColorForEw();
            for (int i = 0; i < _nsLamps.Count; i++)
                _nsLamps[i].sharedMaterial = MaterialLibrary.Get(ns);
            for (int i = 0; i < _ewLamps.Count; i++)
                _ewLamps[i].sharedMaterial = MaterialLibrary.Get(ew);
        }

        Color ColorForNs()
        {
            if (_phase == Phase.NsGreen) return GameConfig.SignalGreen;
            if (_phase == Phase.NsYellow) return GameConfig.SignalYellow;
            return GameConfig.SignalRed;
        }

        Color ColorForEw()
        {
            if (_phase == Phase.EwGreen) return GameConfig.SignalGreen;
            if (_phase == Phase.EwYellow) return GameConfig.SignalYellow;
            return GameConfig.SignalRed;
        }

        void TickHonk()
        {
            _honkCooldown -= Time.deltaTime;
            bool want = DriveInput.HonkHeld || Input.GetKeyDown(KeyCode.H);
            if (!want || _honkCooldown > 0f)
                return;

            _honkCooldown = 0.35f;
            PlayHonk();
            Vector3 from = _player.position;
            for (int i = 0; i < _peds.Count; i++)
            {
                if (_peds[i] == null)
                    continue;
                if ((Flat(_peds[i].transform.position) - Flat(from)).sqrMagnitude < 14f * 14f)
                    _peds[i].Startle();
            }
        }

        void PlayHonk()
        {
            if (_honkClip == null)
                _honkClip = BuildHonkClip();
            AudioSource.PlayClipAtPoint(_honkClip, _player.position, 0.85f);
        }

        static AudioClip BuildHonkClip()
        {
            const int hz = 22050;
            int n = (int)(hz * 0.22f);
            var clip = AudioClip.Create("honk", n, 1, hz, false);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)hz;
                float freq = t < 0.11f ? 420f : 330f;
                float env = 1f - i / (float)n;
                data[i] = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * t)) * 0.28f * env;
            }
            clip.SetData(data, 0);
            return clip;
        }

        void TickCourtesy()
        {
            if (_courtesyGiven || SaveManager.Data.courtesyAwarded)
                return;
            if (_playerVehicle == null || Mathf.Abs(_playerVehicle.CurrentSpeed) > 1.4f)
                return;

            Vector3 p = _player.position;
            int n = _layout.RoadCount;
            for (int ix = 0; ix < n; ix++)
            {
                for (int iz = 0; iz < n; iz++)
                {
                    foreach (CardinalDir heading in new[] { CardinalDir.North, CardinalDir.East, CardinalDir.South, CardinalDir.West })
                    {
                        if (!_layout.HasRoad(ix, iz, CardinalUtil.Opposite(heading)))
                            continue;
                        if (!IsRed(heading))
                            continue;
                        Vector3 line = _layout.StopLine(ix, iz, heading);
                        if ((Flat(p) - Flat(line)).sqrMagnitude < 4.5f * 4.5f)
                        {
                            AwardCourtesy();
                            return;
                        }
                    }
                }
            }
        }

        void AwardCourtesy()
        {
            _courtesyGiven = true;
            SaveManager.Data.courtesyAwarded = true;
            SaveManager.Data.stars += 1;
            SaveManager.Save();
            _hud.SetCurrency(SaveManager.Data.coins, SaveManager.Data.stars);
            _hud.ShowToast("Dikkatli sürüş!  +1 YILDIZ");
        }

        void BuildSignals()
        {
            var root = new GameObject("Signals").transform;
            root.SetParent(transform, false);
            int n = _layout.RoadCount;
            float road = GameConfig.RoadWidth;

            for (int ix = 0; ix < n; ix++)
            {
                for (int iz = 0; iz < n; iz++)
                {
                    Vector3 c = _layout.IntersectionCenter(ix, iz);
                    PlaceSignal(root, c, CardinalDir.North, road, _nsLamps);
                    PlaceSignal(root, c, CardinalDir.South, road, _nsLamps);
                    PlaceSignal(root, c, CardinalDir.East, road, _ewLamps);
                    PlaceSignal(root, c, CardinalDir.West, road, _ewLamps);
                }
            }
        }

        static void PlaceSignal(Transform parent, Vector3 center, CardinalDir heading, float road, List<MeshRenderer> lamps)
        {
            // Direk, yaklaşan şeridin sağında, dur çizgisinin yanında.
            Vector3 pos = center
                          - CardinalUtil.Forward(heading) * (road / 2f + 0.9f)
                          + CardinalUtil.Right(heading) * (GameConfig.LaneOffset + 1.7f);

            PartFactory.Create(PrimitiveType.Cylinder, "Pole", parent,
                pos + Vector3.up * 1.4f, new Vector3(0.18f, 1.4f, 0.18f),
                new Color(0.2f, 0.2f, 0.22f));

            var lamp = PartFactory.Create(PrimitiveType.Sphere, "Lamp", parent,
                pos + Vector3.up * 2.95f, Vector3.one * 0.55f, GameConfig.SignalRed);
            lamps.Add(lamp.GetComponent<MeshRenderer>());
        }

        void SpawnCars()
        {
            var root = new GameObject("NpcCars").transform;
            root.SetParent(transform, false);
            var rng = new System.Random(GameConfig.CitySeed + 17);
            int n = _layout.RoadCount;
            int spawned = 0;
            int guard = 0;

            while (spawned < GameConfig.TrafficCarCount && guard++ < 400)
            {
                int fromIx = rng.Next(n);
                int fromIz = rng.Next(n);
                CardinalDir heading = (CardinalDir)rng.Next(4);
                if (!_layout.TryAdvance(fromIx, fromIz, heading, out int toIx, out int toIz))
                    continue;

                float t = 0.15f + (float)rng.NextDouble() * 0.7f;
                Vector3 pos = _layout.LanePoint(fromIx, fromIz, heading, t);
                if ((Flat(pos) - Flat(_player.position)).sqrMagnitude < 18f * 18f)
                    continue;

                bool tooClose = false;
                for (int i = 0; i < _cars.Count; i++)
                {
                    if ((Flat(_cars[i].transform.position) - Flat(pos)).sqrMagnitude < 12f * 12f)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose)
                    continue;

                Color color = GameConfig.CarPalette[rng.Next(GameConfig.CarPalette.Length)];
                int body = rng.Next(3);
                var go = VehicleFactory.CreateNpcCar(pos, CardinalUtil.Rotation(heading), color, body);
                go.transform.SetParent(root, true);
                var car = go.AddComponent<TrafficCar>();
                car.Init(this, _layout, toIx, toIz, heading);
                _cars.Add(car);
                spawned++;
            }
        }

        void SpawnPeds()
        {
            var root = new GameObject("Pedestrians").transform;
            root.SetParent(transform, false);
            var rng = new System.Random(GameConfig.CitySeed + 99);
            var corners = new Vector3[4];

            for (int i = 0; i < GameConfig.PedestrianCount; i++)
            {
                int bi = rng.Next(GameConfig.CityBlocks);
                int bj = rng.Next(GameConfig.CityBlocks);
                int corner = rng.Next(4);
                _layout.BlockWalkCorners(bi, bj, corners);
                Vector3 a = corners[corner];
                Vector3 b = corners[(corner + 1) % 4];
                Vector3 pos = Vector3.Lerp(a, b, (float)rng.NextDouble()) + Vector3.up * 0.55f;

                Color shirt = GameConfig.ShirtPalette[rng.Next(GameConfig.ShirtPalette.Length)];
                var ped = Pedestrian.Spawn(root, shirt);
                ped.transform.SetPositionAndRotation(pos, Quaternion.identity);
                ped.Init(this, _layout, _player, bi, bj, (corner + 1) % 4);
                _peds.Add(ped);
            }
        }

        static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);
    }
}
