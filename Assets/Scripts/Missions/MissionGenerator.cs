using MeteGame.City;
using MeteGame.Core;
using UnityEngine;

namespace MeteGame.Missions
{
    /// <summary>
    /// Prosedürel görev üretici. Tohum, günün tarihinden ve görev sırasından türetilir:
    /// görevler her gün değişir ama aynı gün içinde deterministiktir.
    /// </summary>
    public static class MissionGenerator
    {
        static readonly string[] Places =
        {
            "fırına", "markete", "okula", "parka", "muayenehaneye", "kütüphaneye", "pastaneye"
        };

        public static Mission Generate(CityLayout layout, Vector3 playerPosition, int missionIndex)
        {
            var rng = new System.Random(SaveManager.TodaySeed() * 100 + missionIndex);
            var type = (MissionType)rng.Next(System.Enum.GetValues(typeof(MissionType)).Length);

            Vector3 pickup = layout.RandomRoadPointAwayFrom(rng, playerPosition, 40f, GameConfig.MaxMissionDistance);
            Vector3 dropoff = layout.RandomRoadPointAwayFrom(rng, pickup, GameConfig.MinMissionDistance, GameConfig.MaxMissionDistance);

            var mission = new Mission
            {
                Type = type,
                PickupPoint = pickup,
                DropoffPoint = dropoff
            };

            string place = Places[rng.Next(Places.Length)];
            switch (type)
            {
                case MissionType.Delivery:
                    mission.Title = "Kurye Görevi";
                    mission.PickupText = "Paketi al!";
                    mission.DropoffText = "Paketi " + place + " teslim et!";
                    mission.CargoShape = PrimitiveType.Cube;
                    mission.CargoColor = new Color(0.72f, 0.52f, 0.30f);
                    break;

                case MissionType.Taxi:
                    mission.Title = "Taksi Görevi";
                    mission.PickupText = "Yolcuyu al!";
                    mission.DropoffText = "Yolcuyu evine bırak!";
                    mission.CargoShape = PrimitiveType.Capsule;
                    mission.CargoColor = new Color(0.30f, 0.55f, 0.90f);
                    break;

                case MissionType.AnimalRescue:
                    mission.Title = "Kayıp Hayvan!";
                    mission.PickupText = "Kayıp kediyi al!";
                    mission.DropoffText = "Kediyi sahibine götür!";
                    mission.CargoShape = PrimitiveType.Sphere;
                    mission.CargoColor = new Color(0.95f, 0.60f, 0.25f);
                    break;

                case MissionType.SchoolRun:
                    mission.Title = "Okul Servisi";
                    mission.PickupText = "Öğrenciyi al!";
                    mission.DropoffText = "Öğrenciyi okula yetiştir!";
                    mission.CargoShape = PrimitiveType.Capsule;
                    mission.CargoColor = new Color(0.95f, 0.80f, 0.30f);
                    break;

                default: // TimedDelivery
                    mission.Title = "Hızlı Teslimat!";
                    mission.PickupText = "Paketi al!";
                    mission.DropoffText = "Çabuk! Paketi " + place + " götür!";
                    mission.CargoShape = PrimitiveType.Cube;
                    mission.CargoColor = new Color(0.90f, 0.35f, 0.30f);
                    break;
            }

            // Ödül, toplam sürüş mesafesiyle orantılı ve 5'e yuvarlanır.
            float totalDistance = Vector3.Distance(playerPosition, pickup) + mission.Distance;
            mission.RewardCoins = Mathf.Max(15, Mathf.RoundToInt((20f + totalDistance / 10f) / 5f) * 5);

            if (type == MissionType.TimedDelivery)
                mission.BonusSeconds = totalDistance / 7f + 15f; // Rahat ama heyecanlı bir hedef.

            return mission;
        }
    }
}
