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

        // Garaj alanları (M3'te kullanılacak, şema şimdiden hazır)
        public List<string> unlockedVehicleIds = new List<string> { "taksi" };
        public string selectedVehicleId = "taksi";
    }
}
