using System;
using System.Collections.Generic;

namespace MeteGame.Core
{
    /// <summary>Cihaza JSON olarak yazılan oyuncu ilerlemesi.</summary>
    [Serializable]
    public class SaveData
    {
        public int coins;
        public int stars;
        public int totalMissionsCompleted;

        // Günlük görev takibi
        public string dailyDate = "";
        public int dailyCompleted;

        public bool courtesyAwarded;

        // Üst üste iki bacağı da zamanında bitirme serisi
        public int currentStreak;
        public int bestStreak;

        // Garaj
        public List<string> unlockedVehicleIds = new List<string> { "taksi" };
        public string selectedVehicleId = "taksi";
        public List<VehiclePaint> paints = new List<VehiclePaint>();

        // Şehir oturumu: menüye dönünce / uygulama kapanınca kaldığın yer
        public CitySession city = new CitySession();
    }

    /// <summary>Şehirde kaldığın yer: araç konumu + varsa yarım kalan görev.</summary>
    [Serializable]
    public class CitySession
    {
        public bool active;
        public float x;
        public float y = 0.5f;
        public float z;
        public float yaw;

        public bool offerOpen;
        public bool hasMission;
        public int leg;

        public int type;
        public int difficulty;
        public string title = "";
        public string pickupText = "";
        public string dropoffText = "";
        public float pickX, pickY, pickZ;
        public float dropX, dropY, dropZ;
        public int reward;
        public int pickupSeconds;
        public int dropoffSeconds;
        public float pickupDistance;
        public int cargoShape;
        public float cr = 1f, cg = 1f, cb = 1f;
        public bool pickupOnTime;
        public float legRemaining;
        public float legDuration;

        public bool thiefAlive;
        public float thiefX, thiefY = 0.5f, thiefZ, thiefYaw;
    }
}
