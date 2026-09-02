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
            _hud.ShowMissionOffer(_current, StartMission);
        }

        void StartMission()
        {
            _pickupOnTime = false;
            _dropoffOnTime = false;

            if (_current.IsChase)
                StartChase();
            else
                StartPickupMarker();

            Sfx.Go(_vehicle.transform.position);
            _hud.ShowToast("HADİ!");
        }

        void StartPickupMarker()
        {
            _activeMarker = MissionMarker.Spawn("PickupMarker", _current.PickupPoint,
                GameConfig.PickupColor, _current.CargoShape);
            _activeMarker.Reached = OnPickupReached;

            _hud.SetMissionText(_current.PickupText);
            _hud.SetTarget(_current.PickupPoint);
            BeginLeg(MissionLeg.Pickup, _current.PickupSeconds);
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

            Transform cargoParent = _vehicle.CargoAnchor != null ? _vehicle.CargoAnchor : _vehicle.transform;
            Vector3 cargoLocal = _vehicle.CargoAnchor != null
                ? Vector3.zero
                : new Vector3(0f, 1.95f, -0.25f);
            _cargo = PartFactory.Create(_current.CargoShape, "Cargo", cargoParent,
                cargoLocal, Vector3.one * 0.8f, _current.CargoColor);
            _cargo.AddComponent<CargoBob>();

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
            if (_leg == MissionLeg.None)
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

        void ClearThief()
        {
            if (_thief == null)
                return;
            Destroy(_thief.gameObject);
            _thief = null;
        }
    }
}
