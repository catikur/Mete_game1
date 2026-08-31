using System;
using System.IO;
using UnityEngine;

namespace MeteGame.Core
{
    /// <summary>Kayıt sistemi: cihazda tek JSON dosyası, internet/hesap yok.</summary>
    public static class SaveManager
    {
        static SaveData _data;

        static string FilePath => Path.Combine(Application.persistentDataPath, "save.json");

        public static SaveData Data
        {
            get
            {
                if (_data == null)
                    _data = Load();
                return _data;
            }
        }

        static SaveData Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath));
                    if (data != null)
                        return data;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Mete Oyunu] Kayıt okunamadı, yeni kayıt açılıyor: " + e.Message);
            }
            return new SaveData();
        }

        public static void Save()
        {
            try
            {
                File.WriteAllText(FilePath, JsonUtility.ToJson(Data, true));
            }
            catch (Exception e)
            {
                Debug.LogError("[Mete Oyunu] Kayıt yazılamadı: " + e.Message);
            }
        }

        /// <summary>Gün değiştiyse günlük görev sayacını sıfırlar. True = yeni gün.</summary>
        public static bool RefreshDaily()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            if (Data.dailyDate != today)
            {
                Data.dailyDate = today;
                Data.dailyCompleted = 0;
                Data.courtesyAwarded = false;
                Save();
                return true;
            }
            return false;
        }

        /// <summary>Günün görevlerini üretmek için tarihten türetilen tohum.</summary>
        public static int TodaySeed()
        {
            var now = DateTime.Now;
            return now.Year * 10000 + now.Month * 100 + now.Day;
        }
    }
}
