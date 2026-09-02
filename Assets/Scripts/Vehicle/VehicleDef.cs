using UnityEngine;

namespace MeteGame.Vehicle
{
    public enum VehicleStyle
    {
        Taxi,
        Minibus,
        Pickup,
        Ambulance,
        Police,
        FireTruck,
        IceCream,
        Race
    }

    /// <summary>Garaj kataloğundaki bir araç tanımı.</summary>
    public sealed class VehicleDef
    {
        public string Id;
        public string Name;
        public string Blurb;
        public int Price;
        public VehicleStyle Style;
        public int DefaultColorIndex;

        public float MaxForwardSpeed;
        public float MaxReverseSpeed;
        public float Acceleration;
        public float MaxSteerDegPerSec;
        public float Mass;

        public Vector3 ColliderSize;
        public Vector3 ColliderCenter;
        public Vector3 CargoLocalPosition;
    }
}
