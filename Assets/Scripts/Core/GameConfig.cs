using UnityEngine;

namespace MeteGame.Core
{
    /// <summary>Oyunun ayar sabitleri ve renk paleti — tek yerden dengelenir.</summary>
    public static class GameConfig
    {
        // Şehir
        public const int CityBlocks = 6;
        public const float RoadWidth = 10f;
        public const float BlockSize = 26f;
        public const int CitySeed = 20260831; // Sabit: şehir her oyunda aynı kalır.

        // Araç
        public const float MaxForwardSpeed = 15f;
        public const float MaxReverseSpeed = 5f;
        public const float Acceleration = 9f;
        public const float BrakeDeceleration = 22f;
        public const float MaxSteerDegPerSec = 180f;

        // Görevler
        public const int DailyMissionTarget = 5;
        public const float MinMissionDistance = 60f;
        public const float MaxMissionDistance = 160f;
        public const float MarkerTriggerRadius = 5f;

        // İki aşamalı görev süresi (çocuklar için bol; bitince görev batmaz)
        public const float TimerCruiseSpeed = 6f;   // tahmini ortalama m/s
        public const float TimerLegBuffer = 18f;    // dönüş + ışık payı
        public const int MinLegSeconds = 25;
        public const float TimerSoonRatio = 0.40f;
        public const float TimerUrgentRatio = 0.18f;

        // Trafik / şehir hayatı
        public const int TrafficCarCount = 16;
        public const int PedestrianCount = 24;
        public const float LaneOffset = 2.4f;
        public const float NpcSpeed = 8f;
        public const float PedestrianSpeed = 1.6f;
        public const float LightGreenSeconds = 6.5f;
        public const float LightYellowSeconds = 1.6f;
        public const float StopLinePadding = 1.4f;

        // Renkler
        public static readonly Color Grass = FromBytes(112, 173, 92);
        public static readonly Color Road = FromBytes(62, 64, 70);
        public static readonly Color RoadMarking = FromBytes(240, 238, 228);
        public static readonly Color Sidewalk = FromBytes(182, 182, 188);
        public static readonly Color Hedge = FromBytes(52, 116, 62);
        public static readonly Color TreeTrunk = FromBytes(122, 84, 56);
        public static readonly Color TreeCrown = FromBytes(74, 154, 78);
        public static readonly Color PickupColor = FromBytes(0, 186, 255);
        public static readonly Color DropoffColor = FromBytes(88, 214, 104);
        public static readonly Color TaxiYellow = FromBytes(252, 196, 32);
        public static readonly Color Gold = FromBytes(255, 205, 60);
        public static readonly Color SignalRed = FromBytes(230, 55, 45);
        public static readonly Color SignalYellow = FromBytes(255, 200, 40);
        public static readonly Color SignalGreen = FromBytes(50, 210, 90);

        public const int PaintPrice = 50;

        public static readonly Color[] CarPalette =
        {
            FromBytes(232, 76, 76),   // kırmızı
            FromBytes(70, 130, 210),  // mavi
            FromBytes(90, 175, 95),   // yeşil
            FromBytes(252, 196, 32),  // sarı (taksi)
            FromBytes(245, 245, 245), // beyaz
            FromBytes(255, 140, 60),  // turuncu
            FromBytes(170, 110, 210), // mor
            FromBytes(55, 55, 60)     // siyah
        };

        public static readonly Color[] ShirtPalette =
        {
            FromBytes(230, 90, 90),
            FromBytes(80, 140, 220),
            FromBytes(255, 170, 60),
            FromBytes(90, 190, 130),
            FromBytes(200, 110, 190),
            FromBytes(255, 230, 90)
        };

        public static readonly Color[] BuildingPalette =
        {
            FromBytes(236, 160, 150), // somon
            FromBytes(244, 210, 130), // hardal
            FromBytes(150, 196, 232), // bebek mavisi
            FromBytes(184, 160, 214), // lila
            FromBytes(240, 178, 200), // pembe
            FromBytes(160, 214, 190), // mint
            FromBytes(226, 196, 166), // kum
            FromBytes(198, 208, 220)  // gri-mavi
        };

        /// <summary>Bir yol + bir blok geneşliği (grid adımı).</summary>
        public static float CityPitch => BlockSize + RoadWidth;

        /// <summary>Şehrin toplam kenar uzunluğu.</summary>
        public static float CityExtent => CityBlocks * CityPitch + RoadWidth;

        static Color FromBytes(byte r, byte g, byte b) => new Color32(r, g, b, 255);
    }
}
