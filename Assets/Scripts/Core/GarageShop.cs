using System;
using System.Collections.Generic;
using MeteGame.Vehicle;
using UnityEngine;

namespace MeteGame.Core
{
    /// <summary>Garaj satın alma, seçim ve boya — kayıtla konuşur.</summary>
    public static class GarageShop
    {
        public static void Normalize(SaveData data)
        {
            if (data == null)
                return;
            if (data.unlockedVehicleIds == null)
                data.unlockedVehicleIds = new List<string>();
            if (data.paints == null)
                data.paints = new List<VehiclePaint>();

            if (data.unlockedVehicleIds.Count == 0)
                data.unlockedVehicleIds.Add("taksi");
            if (!data.unlockedVehicleIds.Contains("taksi"))
                data.unlockedVehicleIds.Insert(0, "taksi");

            EnsurePaint(data, "taksi", VehicleCatalog.Get("taksi").DefaultColorIndex);

            if (string.IsNullOrEmpty(data.selectedVehicleId)
                || !data.unlockedVehicleIds.Contains(data.selectedVehicleId))
            {
                data.selectedVehicleId = "taksi";
            }

            if (data.city == null)
                data.city = new CitySession();
        }

        public static bool IsUnlocked(string id) =>
            SaveManager.Data.unlockedVehicleIds.Contains(id);

        public static VehiclePaint GetPaint(string id)
        {
            var data = SaveManager.Data;
            int defaultColor = VehicleCatalog.Get(id).DefaultColorIndex;
            EnsurePaint(data, id, defaultColor);
            for (int i = 0; i < data.paints.Count; i++)
            {
                if (data.paints[i].id == id)
                    return data.paints[i];
            }
            return data.paints[data.paints.Count - 1];
        }

        public static int ColorIndex(string id) => GetPaint(id).colorIndex;

        public static Color BodyColor(string id, bool lockedPreview)
        {
            if (lockedPreview)
                return new Color(0.14f, 0.15f, 0.17f);
            int index = Mathf.Clamp(ColorIndex(id), 0, GameConfig.CarPalette.Length - 1);
            return GameConfig.CarPalette[index];
        }

        public static bool IsPaintUnlocked(string vehicleId, int colorIndex)
        {
            var paint = GetPaint(vehicleId);
            return HasColor(paint, colorIndex);
        }

        public static bool TryBuyVehicle(string id, out string message)
        {
            var def = VehicleCatalog.Get(id);
            var data = SaveManager.Data;
            if (IsUnlocked(id))
            {
                message = "Zaten sende!";
                return false;
            }
            if (data.coins < def.Price)
            {
                message = "Yeterli altın yok";
                return false;
            }

            data.coins -= def.Price;
            data.unlockedVehicleIds.Add(id);
            EnsurePaint(data, id, def.DefaultColorIndex);
            data.selectedVehicleId = id;
            SaveManager.Save();
            message = def.Name + " senin!";
            return true;
        }

        public static bool TrySelect(string id, out string message)
        {
            if (!IsUnlocked(id))
            {
                message = "Önce satın al";
                return false;
            }
            SaveManager.Data.selectedVehicleId = id;
            SaveManager.Save();
            message = "Seçildi!";
            return true;
        }

        public static bool TryPaint(string id, int colorIndex, out string message)
        {
            if (!IsUnlocked(id))
            {
                message = "Önce aracı al";
                return false;
            }

            colorIndex = Mathf.Clamp(colorIndex, 0, GameConfig.CarPalette.Length - 1);
            var paint = GetPaint(id);
            if (HasColor(paint, colorIndex))
            {
                paint.colorIndex = colorIndex;
                SaveManager.Save();
                message = "";
                return true;
            }

            if (SaveManager.Data.coins < GameConfig.PaintPrice)
            {
                message = "Boya için " + GameConfig.PaintPrice + " altın lazım";
                return false;
            }

            SaveManager.Data.coins -= GameConfig.PaintPrice;
            paint.unlockedColors = AppendColor(paint.unlockedColors, colorIndex);
            paint.colorIndex = colorIndex;
            SaveManager.Save();
            message = "Yeni renk!";
            return true;
        }

        static void EnsurePaint(SaveData data, string id, int defaultColor)
        {
            for (int i = 0; i < data.paints.Count; i++)
            {
                if (data.paints[i].id == id)
                {
                    if (string.IsNullOrEmpty(data.paints[i].unlockedColors))
                        data.paints[i].unlockedColors = defaultColor.ToString();
                    return;
                }
            }

            data.paints.Add(new VehiclePaint
            {
                id = id,
                colorIndex = defaultColor,
                unlockedColors = defaultColor.ToString()
            });
        }

        static bool HasColor(VehiclePaint paint, int colorIndex)
        {
            if (paint == null || string.IsNullOrEmpty(paint.unlockedColors))
                return false;
            string[] parts = paint.unlockedColors.Split(',');
            string key = colorIndex.ToString();
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Trim() == key)
                    return true;
            }
            return false;
        }

        static string AppendColor(string unlocked, int colorIndex)
        {
            if (string.IsNullOrEmpty(unlocked))
                return colorIndex.ToString();
            return unlocked + "," + colorIndex;
        }
    }

    [Serializable]
    public class VehiclePaint
    {
        public string id;
        public int colorIndex;
        public string unlockedColors;
    }
}
