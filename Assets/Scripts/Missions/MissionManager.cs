using System.Collections;
using MeteGame.City;
using MeteGame.Core;
using MeteGame.UI;
using MeteGame.Vehicle;
using UnityEngine;

namespace MeteGame.Missions
{
    /// <summary>
    /// Görev akışı: teklif → alış noktası → bırakış noktası → ödül + kutlama → yeni teklif.
    /// Görevler asla başarısız olmaz; süreli görevlerde hız sadece bonus kazandırır.
    /// </summary>
    public class MissionManager : MonoBehaviour
    {
        CityLayout _layout;
        VehicleController _vehicle;
        HudController _hud;

        Mission _current;
        MissionMarker _activeMarker;
        GameObject _cargo;
        float _missionStartTime;
        int _missionCounter;

        public void Init(CityLayout layout, VehicleController vehicle, HudController hud)
        {
            _layout = layout;
            _vehicle = vehicle;
            _hud = hud;
            _missionCounter = SaveManager.Data.totalMissionsCompleted;

            _hud.SetCurrency(SaveManager.Data.coins, SaveManager.Data.stars);
            _hud.SetDailyProgress(SaveManager.Data.dailyCompleted, GameConfig.DailyMissionTarget);

            StartCoroutine(OfferAfterDelay(1.2f));
        }

        IEnumerator OfferAfterDelay(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            OfferNext();
        }

        void OfferNext()
        {
            _current = MissionGenerator.Generate(_layout, _vehicle.transform.position, _missionCounter);
            _hud.ShowMissionOffer(_current, StartMission);
        }

        void StartMission()
        {
            _missionStartTime = Time.time;

            _activeMarker = MissionMarker.Spawn("PickupMarker", _current.PickupPoint,
                GameConfig.PickupColor, _current.CargoShape);
            _activeMarker.Reached = OnPickupReached;

            _hud.SetMissionText(_current.PickupText);
            _hud.SetTarget(_current.PickupPoint);
        }

        void OnPickupReached()
        {
            Destroy(_activeMarker.gameObject);

            // Kargo (paket/yolcu/kedi) aracın üstüne biner — teslim edilene kadar orada.
            _cargo = PartFactory.Create(_current.CargoShape, "Cargo", _vehicle.transform,
                new Vector3(0f, 1.95f, -0.25f), Vector3.one * 0.8f, _current.CargoColor);

            _activeMarker = MissionMarker.Spawn("DropoffMarker", _current.DropoffPoint,
                GameConfig.DropoffColor, _current.CargoShape);
            _activeMarker.Reached = OnDropoffReached;

            _hud.SetMissionText(_current.DropoffText);
            _hud.SetTarget(_current.DropoffPoint);
        }

        void OnDropoffReached()
        {
            Destroy(_activeMarker.gameObject);
            _activeMarker = null;
            if (_cargo != null)
                Destroy(_cargo);

            bool bonus = _current.BonusSeconds > 0f
                         && Time.time - _missionStartTime <= _current.BonusSeconds;
            int stars = bonus ? 2 : 1;

            var data = SaveManager.Data;
            data.coins += _current.RewardCoins;
            data.stars += stars;
            data.totalMissionsCompleted += 1;
            data.dailyCompleted += 1;
            SaveManager.Save();
            _missionCounter += 1;

            _hud.SetCurrency(data.coins, data.stars);
            _hud.SetDailyProgress(data.dailyCompleted, GameConfig.DailyMissionTarget);
            _hud.SetTarget(null);
            _hud.SetMissionText("");
            _hud.ShowCelebration(_current.RewardCoins, stars);

            StartCoroutine(OfferAfterDelay(2.6f));
        }
    }
}
