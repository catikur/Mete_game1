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
        public const float MaxSteerDegPerSec = 120f;

        // Görevler
        public const int DailyMissionTarget = 5;
        public const float MinMissionDistance = 60f;
        public const float MaxMissionDistance = 160f;
        public const float MarkerTriggerRadius = 5f;

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
