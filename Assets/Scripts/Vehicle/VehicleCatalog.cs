using MeteGame.Core;
using UnityEngine;

namespace MeteGame.Vehicle
{
    /// <summary>Yedi araçlık katalog — fiyat, hız ve siluet burada dengelenir.</summary>
    public static class VehicleCatalog
    {
        public static readonly VehicleDef[] All =
        {
            new VehicleDef
            {
                Id = "taksi",
                Name = "Taksi",
                Blurb = "Dengeli şehir taksisi. Başlangıç aracı.",
                Price = 0,
                Style = VehicleStyle.Taxi,
                DefaultColorIndex = 3,
                MaxForwardSpeed = 15f,
                MaxReverseSpeed = 5f,
                Acceleration = 9f,
                MaxSteerDegPerSec = 180f,
                Mass = 1200f,
                ColliderSize = new Vector3(2.0f, 1.2f, 4.4f),
                ColliderCenter = new Vector3(0f, 0.7f, 0f),
                CargoLocalPosition = new Vector3(0f, 1.95f, -0.25f)
            },
            new VehicleDef
            {
                Id = "minibus",
                Name = "Minibüs",
                Blurb = "Geniş ve yüksek, biraz yavaş.",
                Price = 300,
                Style = VehicleStyle.Minibus,
                DefaultColorIndex = 2,
                MaxForwardSpeed = 12f,
                MaxReverseSpeed = 4.5f,
                Acceleration = 7f,
                MaxSteerDegPerSec = 150f,
                Mass = 1600f,
                ColliderSize = new Vector3(2.15f, 1.7f, 5.0f),
                ColliderCenter = new Vector3(0f, 0.95f, 0f),
                CargoLocalPosition = new Vector3(0f, 2.45f, -0.4f)
            },
            new VehicleDef
            {
                Id = "kamyonet",
                Name = "Kamyonet",
                Blurb = "Sağlam kasa, açık kasa yatağı.",
                Price = 600,
                Style = VehicleStyle.Pickup,
                DefaultColorIndex = 5,
                MaxForwardSpeed = 14f,
                MaxReverseSpeed = 5f,
                Acceleration = 8f,
                MaxSteerDegPerSec = 160f,
                Mass = 1500f,
                ColliderSize = new Vector3(2.05f, 1.35f, 4.7f),
                ColliderCenter = new Vector3(0f, 0.75f, 0f),
                CargoLocalPosition = new Vector3(0f, 1.85f, 0.35f)
            },
            new VehicleDef
            {
                Id = "ambulans",
                Name = "Ambulans",
                Blurb = "Hızlı yardım aracı.",
                Price = 900,
                Style = VehicleStyle.Ambulance,
                DefaultColorIndex = 4,
                MaxForwardSpeed = 17f,
                MaxReverseSpeed = 5.5f,
                Acceleration = 11f,
                MaxSteerDegPerSec = 190f,
                Mass = 1300f,
                ColliderSize = new Vector3(2.1f, 1.8f, 4.9f),
                ColliderCenter = new Vector3(0f, 1.0f, 0f),
                CargoLocalPosition = new Vector3(0f, 2.55f, -0.3f)
            },
            new VehicleDef
            {
                Id = "itfaiye",
                Name = "İtfaiye",
                Blurb = "Büyük, güçlü ve uzun.",
                Price = 1200,
                Style = VehicleStyle.FireTruck,
                DefaultColorIndex = 0,
                MaxForwardSpeed = 13f,
                MaxReverseSpeed = 4.5f,
                Acceleration = 7.5f,
                MaxSteerDegPerSec = 140f,
                Mass = 2000f,
                ColliderSize = new Vector3(2.2f, 1.85f, 5.7f),
                ColliderCenter = new Vector3(0f, 1.0f, 0f),
                CargoLocalPosition = new Vector3(0f, 2.55f, -0.8f)
            },
            new VehicleDef
            {
                Id = "dondurma",
                Name = "Dondurma Kamyonu",
                Blurb = "Eğlenceli ve sevimli.",
                Price = 1500,
                Style = VehicleStyle.IceCream,
                DefaultColorIndex = 6,
                MaxForwardSpeed = 13.5f,
                MaxReverseSpeed = 5f,
                Acceleration = 8f,
                MaxSteerDegPerSec = 160f,
                Mass = 1400f,
                ColliderSize = new Vector3(2.1f, 1.75f, 4.7f),
                ColliderCenter = new Vector3(0f, 0.95f, 0f),
                CargoLocalPosition = new Vector3(0f, 2.15f, 0.9f)
            },
            new VehicleDef
            {
                Id = "yaris",
                Name = "Yarış Arabası",
                Blurb = "En hızlısı! Alçak ve çevik.",
                Price = 2000,
                Style = VehicleStyle.Race,
                DefaultColorIndex = 1,
                MaxForwardSpeed = 18f,
                MaxReverseSpeed = 6f,
                Acceleration = 13f,
                MaxSteerDegPerSec = 220f,
                Mass = 900f,
                ColliderSize = new Vector3(2.05f, 0.9f, 3.9f),
                ColliderCenter = new Vector3(0f, 0.5f, 0f),
                CargoLocalPosition = new Vector3(0f, 1.35f, -0.2f)
            }
        };

        public static VehicleDef Get(string id)
        {
            for (int i = 0; i < All.Length; i++)
            {
                if (All[i].Id == id)
                    return All[i];
            }
            return All[0];
        }

        public static int IndexOf(string id)
        {
            for (int i = 0; i < All.Length; i++)
            {
                if (All[i].Id == id)
                    return i;
            }
            return 0;
        }

        public static VehicleDef Selected => Get(SaveManager.Data.selectedVehicleId);
    }
}
