using UnityEngine;

namespace MeteGame.Missions
{
    public enum MissionType
    {
        Delivery,      // Kurye
        Taxi,          // Taksi
        AnimalRescue,  // Kayıp hayvan
        SchoolRun,     // Okul servisi
        TimedDelivery  // Hızlı teslimat (süre bonusu)
    }

    /// <summary>Tek bir görevin tüm verisi. Görevler asla başarısız olmaz.</summary>
    public class Mission
    {
        public MissionType Type;
        public string Title;
        public string PickupText;   // Ör: "Paketi al!"
        public string DropoffText;  // Ör: "Paketi fırına teslim et!"
        public Vector3 PickupPoint;
        public Vector3 DropoffPoint;
        public int RewardCoins;

        /// <summary>0'dan büyükse: bu süre içinde bitirilirse +1 bonus yıldız.</summary>
        public float BonusSeconds;

        /// <summary>Araca binen kargo görselinin şekli ve rengi.</summary>
        public PrimitiveType CargoShape;
        public Color CargoColor;

        public float Distance => Vector3.Distance(PickupPoint, DropoffPoint);
    }
}
