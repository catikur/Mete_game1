using UnityEngine;

namespace MeteGame.Missions
{
    public enum MissionType
    {
        Delivery,      // Kurye
        Taxi,          // Taksi
        AnimalRescue,  // Kayıp hayvan
        SchoolRun,     // Okul servisi
        TimedDelivery  // Hızlı teslimat (daha sıkı süre)
    }

    public enum MissionDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    public enum MissionLeg
    {
        None,
        Pickup,
        Dropoff
    }

    /// <summary>Tek bir görevin tüm verisi. Görevler asla başarısız olmaz.</summary>
    public class Mission
    {
        public MissionType Type;
        public MissionDifficulty Difficulty;
        public string Title;
        public string PickupText;   // Ör: "Paketi al!"
        public string DropoffText;  // Ör: "Paketi fırına teslim et!"
        public Vector3 PickupPoint;
        public Vector3 DropoffPoint;
        public int RewardCoins;

        /// <summary>Görevi aldıktan sonra ilk adrese gitmek için süre (saniye).</summary>
        public int PickupSeconds;

        /// <summary>Alıştan teslimata kadar süre (saniye).</summary>
        public int DropoffSeconds;

        /// <summary>Üretim anında oyuncudan alış noktasına mesafe.</summary>
        public float PickupDistance;

        /// <summary>Araca binen kargo görselinin şekli ve rengi.</summary>
        public PrimitiveType CargoShape;
        public Color CargoColor;

        public float DropoffDistance => Vector3.Distance(PickupPoint, DropoffPoint);

        public string DifficultyLabel => Difficulty switch
        {
            MissionDifficulty.Easy => "Kolay",
            MissionDifficulty.Hard => "Zor",
            _ => "Orta"
        };
    }
}
