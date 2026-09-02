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
    }
}
