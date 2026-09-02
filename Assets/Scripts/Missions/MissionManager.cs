using System.Collections;
using MeteGame.City;
using MeteGame.Core;
using MeteGame.UI;
using MeteGame.Vehicle;
using UnityEngine;

namespace MeteGame.Missions
{
    /// <summary>
    /// Görev akışı: teklif → alış (süre 1) → bırakış (süre 2) → ödül + kutlama → yeni teklif.
    /// Süre bitince görev BATMAZ; o bacak için zamanında yıldızı kaçar.
    /// </summary>
    public class MissionManager : MonoBehaviour
    {
        CityLayout _layout;
        VehicleController _vehicle;
        HudController _hud;

        Mission _current;
        MissionMarker _activeMarker;
        GameObject _cargo;
        ThiefCar _thief;
        int _missionCounter;

        MissionLeg _leg = MissionLeg.None;
        public bool IsOnDuty => _leg != MissionLeg.None;
        bool _offerOpen;
        float _legStartTime;
        float _legDuration;
        bool _pickupOnTime;
        bool _dropoffOnTime;

        public void Init(CityLayout layout, VehicleController vehicle, HudController hud)
        {
            _layout = layout;
            _vehicle = vehicle;
            _hud = hud;
            _missionCounter = SaveManager.Data.totalMissionsCompleted;

            _hud.SetCurrency(SaveManager.Data.coins, SaveManager.Data.stars);
            _hud.SetDailyProgress(SaveManager.Data.dailyCompleted, GameConfig.DailyMissionTarget);
            _hud.SetCombo(SaveManager.Data.currentStreak);

            if (!TryRestoreSession())
                StartCoroutine(OfferAfterDelay(1.2f));
        }

        IEnumerator OfferAfterDelay(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            OfferNext();
        }

        void SyncDailyHud()
        {
            if (SaveManager.RefreshDaily())
                _hud.SetDailyProgress(SaveManager.Data.dailyCompleted, GameConfig.DailyMissionTarget);
        }

        void OfferNext()
        {
            SyncDailyHud();
            StopClock();
            ClearThief();
            _current = MissionGenerator.Generate(_layout, _vehicle.transform.position, _missionCounter);
            _offerOpen = true;
            _hud.ShowMissionOffer(_current, StartMission);
        }

        void StartMission()
        {
            _offerOpen = false;
            _pickupOnTime = false;
            _dropoffOnTime = false;

            if (_current.IsChase)
                StartChase();
            else
                StartPickupMarker();

            Sfx.Go(_vehicle.transform.position);
            _hud.ShowToast("HADİ!");
        }

        void StartChase()
        {
            _thief = ThiefCar.Spawn(_layout, _vehicle.transform, _current.PickupPoint);
            _hud.SetMissionText(_current.PickupText);
            _hud.SetTarget(_thief.transform.position);
            BeginLeg(MissionLeg.Pickup, _current.PickupSeconds);
        }

        void TryCatchThief()
        {
            if (_thief == null || _thief.Caught)
                return;

            Vector3 a = _vehicle.transform.position;
            Vector3 b = _thief.transform.position;
            a.y = 0f;
            b.y = 0f;
            if (Vector3.Distance(a, b) > GameConfig.ThiefCatchRadius)
                return;

            OnPickupReached();
        }

        void OnPickupReached()
        {
            if (_leg != MissionLeg.Pickup)
                return;

            _pickupOnTime = Remaining() >= 0f;
            if (_thief != null)
                _thief.StopFleeing();
            if (_activeMarker != null)
                Destroy(_activeMarker.gameObject);

            SpawnCargo();

            _activeMarker = MissionMarker.Spawn("DropoffMarker", _current.DropoffPoint,
                GameConfig.DropoffColor, _current.CargoShape);
            _activeMarker.Reached = OnDropoffReached;

            _hud.SetMissionText(_current.DropoffText);
            _hud.SetTarget(_current.DropoffPoint);
            BeginLeg(MissionLeg.Dropoff, _current.DropoffSeconds);

            ClearThief();

            Sfx.Ding(_vehicle.transform.position);
            string caught = _current.IsChase
                ? (_pickupOnTime ? "ZAMANINDA! YAKALADIN!" : "YAKALADIN!")
                : (_pickupOnTime ? "ZAMANINDA! ALDIN!" : "ALDIN!");
            _hud.ShowToast(caught);
        }

        void OnDropoffReached()
        {
            _dropoffOnTime = Remaining() >= 0f;
            Destroy(_activeMarker.gameObject);
            _activeMarker = null;
            if (_cargo != null)
                Destroy(_cargo);

            StopClock();

            int stars = 1;
            int bonusCoins = 0;
            if (_pickupOnTime)
            {
                stars += 1;
                bonusCoins += 5;
            }
            if (_dropoffOnTime)
            {
                stars += 1;
                bonusCoins += 5;
            }

            var data = SaveManager.Data;
            bool perfect = _pickupOnTime && _dropoffOnTime;
            if (perfect)
            {
                data.currentStreak += 1;
                if (data.currentStreak > data.bestStreak)
                    data.bestStreak = data.currentStreak;
                bonusCoins += 5 * data.currentStreak;
            }
            else
            {
                data.currentStreak = 0;
            }

            int coins = _current.RewardCoins + bonusCoins;

            SyncDailyHud();
            data.coins += coins;
            data.stars += stars;
            data.totalMissionsCompleted += 1;
            data.dailyCompleted += 1;
            SaveManager.Save();
            _missionCounter += 1;

            _hud.SetCurrency(data.coins, data.stars);
            _hud.SetDailyProgress(data.dailyCompleted, GameConfig.DailyMissionTarget);
            _hud.SetCombo(data.currentStreak);
            _hud.SetTarget(null);
            _hud.SetMissionText("");
            ClearThief();

            Sfx.Success(_vehicle.transform.position);
            _hud.ShowCelebration(coins, stars, perfect, data.currentStreak);

            StartCoroutine(OfferAfterDelay(2.6f));
        }

        void BeginLeg(MissionLeg leg, int seconds)
        {
            _leg = leg;
            _legStartTime = Time.time;
            _legDuration = seconds;
            PushTimer();
        }

        void StopClock()
        {
            _leg = MissionLeg.None;
            _hud.HideTimer();
        }

        float Remaining() => _legDuration - (Time.time - _legStartTime);

        void Update()
        {
            if (_leg == MissionLeg.None || Time.timeScale <= 0.01f)
                return;

            if (_leg == MissionLeg.Pickup && _current != null && _current.IsChase && _thief != null)
            {
                _hud.SetTarget(_thief.transform.position);
                TryCatchThief();
            }

            PushTimer();
        }

        void PushTimer()
        {
            string label = _leg == MissionLeg.Pickup
                ? _current.PickupPhaseLabel
                : _current.DropoffPhaseLabel;
            int step = _leg == MissionLeg.Pickup ? 1 : 2;
            _hud.SetTimer(label, step, Remaining(), _legDuration);
        }

        public void CaptureSession()
        {
            if (_vehicle == null)
                return;

            var data = SaveManager.Data;
            if (data.city == null)
                data.city = new CitySession();

            var session = data.city;
            Vector3 pos = _vehicle.transform.position;
            session.active = true;
            session.x = pos.x;
            session.y = 0.5f;
            session.z = pos.z;
            session.yaw = _vehicle.transform.eulerAngles.y;
            session.offerOpen = _offerOpen;
            session.leg = (int)_leg;
            session.pickupOnTime = _pickupOnTime;
            session.legRemaining = _leg != MissionLeg.None ? Remaining() : 0f;
            session.legDuration = _leg != MissionLeg.None ? _legDuration : 0f;

            bool keepMission = _current != null && (_offerOpen || _leg != MissionLeg.None);
            session.hasMission = keepMission;
            if (keepMission)
                WriteMission(session, _current);
            else
                ClearMissionFields(session);

            session.thiefAlive = _thief != null && !_thief.Caught && _leg == MissionLeg.Pickup;
            if (session.thiefAlive)
            {
                Vector3 tp = _thief.transform.position;
                session.thiefX = tp.x;
                session.thiefY = 0.5f;
                session.thiefZ = tp.z;
                session.thiefYaw = _thief.transform.eulerAngles.y;
            }

            SaveManager.Save();
        }

        bool TryRestoreSession()
        {
            var session = SaveManager.Data.city;
            if (session == null || !session.active || !session.hasMission)
                return false;

            _current = ReadMission(session);
            if (_current == null)
                return false;

            _pickupOnTime = session.pickupOnTime;
            _offerOpen = false;

            if (session.offerOpen)
            {
                _offerOpen = true;
                _hud.ShowMissionOffer(_current, StartMission);
                return true;
            }

            if (session.leg == (int)MissionLeg.Pickup)
            {
                if (_current.IsChase)
                    RestoreChase(session);
                else
                    StartPickupMarkerWithoutClock();
                RestoreClock(MissionLeg.Pickup, session);
                return true;
            }

            if (session.leg == (int)MissionLeg.Dropoff)
            {
                SpawnCargo();
                _activeMarker = MissionMarker.Spawn("DropoffMarker", _current.DropoffPoint,
                    GameConfig.DropoffColor, _current.CargoShape);
                _activeMarker.Reached = OnDropoffReached;
                _hud.SetMissionText(_current.DropoffText);
                _hud.SetTarget(_current.DropoffPoint);
                RestoreClock(MissionLeg.Dropoff, session);
                return true;
            }

            return false;
        }

        void StartPickupMarkerWithoutClock()
        {
            _activeMarker = MissionMarker.Spawn("PickupMarker", _current.PickupPoint,
                GameConfig.PickupColor, _current.CargoShape);
            _activeMarker.Reached = OnPickupReached;
            _hud.SetMissionText(_current.PickupText);
            _hud.SetTarget(_current.PickupPoint);
        }

        void StartPickupMarker()
        {
            StartPickupMarkerWithoutClock();
            BeginLeg(MissionLeg.Pickup, _current.PickupSeconds);
        }

        void RestoreChase(CitySession session)
        {
            Vector3 preferred = session.thiefAlive
                ? new Vector3(session.thiefX, 0.5f, session.thiefZ)
                : _current.PickupPoint;
            if (session.thiefAlive)
                _thief = ThiefCar.SpawnAt(_layout, _vehicle.transform, preferred, session.thiefYaw);
            else
                _thief = ThiefCar.Spawn(_layout, _vehicle.transform, preferred);

            _hud.SetMissionText(_current.PickupText);
            _hud.SetTarget(_thief.transform.position);
        }

        void RestoreClock(MissionLeg leg, CitySession session)
        {
            float duration = session.legDuration > 1f ? session.legDuration : Mathf.Max(1f, session.legRemaining);
            float remaining = session.legRemaining;
            _leg = leg;
            _legDuration = duration;
            _legStartTime = Time.time - (duration - remaining);
            PushTimer();
        }

        void SpawnCargo()
        {
            Transform cargoParent = _vehicle.CargoAnchor != null ? _vehicle.CargoAnchor : _vehicle.transform;
            Vector3 cargoLocal = _vehicle.CargoAnchor != null
                ? Vector3.zero
                : new Vector3(0f, 1.95f, -0.25f);
            _cargo = PartFactory.Create(_current.CargoShape, "Cargo", cargoParent,
                cargoLocal, Vector3.one * 0.8f, _current.CargoColor);
            _cargo.AddComponent<CargoBob>();
        }

        static void WriteMission(CitySession session, Mission mission)
        {
            session.type = (int)mission.Type;
            session.difficulty = (int)mission.Difficulty;
            session.title = mission.Title;
            session.pickupText = mission.PickupText;
            session.dropoffText = mission.DropoffText;
            session.pickX = mission.PickupPoint.x;
            session.pickY = mission.PickupPoint.y;
            session.pickZ = mission.PickupPoint.z;
            session.dropX = mission.DropoffPoint.x;
            session.dropY = mission.DropoffPoint.y;
            session.dropZ = mission.DropoffPoint.z;
            session.reward = mission.RewardCoins;
            session.pickupSeconds = mission.PickupSeconds;
            session.dropoffSeconds = mission.DropoffSeconds;
            session.pickupDistance = mission.PickupDistance;
            session.cargoShape = (int)mission.CargoShape;
            session.cr = mission.CargoColor.r;
            session.cg = mission.CargoColor.g;
            session.cb = mission.CargoColor.b;
        }

        static void ClearMissionFields(CitySession session)
        {
            session.hasMission = false;
            session.offerOpen = false;
            session.leg = 0;
            session.thiefAlive = false;
            session.title = "";
        }

        static Mission ReadMission(CitySession session)
        {
            if (session == null || !session.hasMission)
                return null;
            return new Mission
            {
                Type = (MissionType)session.type,
                Difficulty = (MissionDifficulty)session.difficulty,
                Title = session.title,
                PickupText = session.pickupText,
                DropoffText = session.dropoffText,
                PickupPoint = new Vector3(session.pickX, session.pickY, session.pickZ),
                DropoffPoint = new Vector3(session.dropX, session.dropY, session.dropZ),
                RewardCoins = session.reward,
                PickupSeconds = session.pickupSeconds,
                DropoffSeconds = session.dropoffSeconds,
                PickupDistance = session.pickupDistance,
                CargoShape = (PrimitiveType)session.cargoShape,
                CargoColor = new Color(session.cr, session.cg, session.cb)
            };
        }

        void ClearThief()
        {
            if (_thief == null)
                return;
            Destroy(_thief.gameObject);
            _thief = null;
        }
    }
}
