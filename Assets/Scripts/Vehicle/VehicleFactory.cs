using MeteGame.Core;
using UnityEngine;

namespace MeteGame.Vehicle
{
    /// <summary>
    /// Primitive parçalardan oyuncu, önizleme ve NPC araçlarını kurar.
    /// Garaj kataloğu oyuncu siluetini seçer; NPC'ler basit gövde çeşitleri kullanır.
    /// </summary>
    public static class VehicleFactory
    {
        public static VehicleController CreatePlayerVehicle(Vector3 position, Quaternion rotation)
        {
            var def = VehicleCatalog.Selected;
            Color color = GarageShop.BodyColor(def.Id, false);
            return CreatePlayerVehicle(def, color, position, rotation);
        }

        public static VehicleController CreatePlayerVehicle(
            VehicleDef def, Color color, Vector3 position, Quaternion rotation)
        {
            var root = new GameObject("PlayerVehicle");
            root.transform.SetPositionAndRotation(position, rotation);

            var body = root.AddComponent<Rigidbody>();
            body.mass = def.Mass;
            body.linearDamping = 0f;
            body.angularDamping = 8f;

            var collider = root.AddComponent<BoxCollider>();
            collider.size = def.ColliderSize;
            collider.center = def.ColliderCenter;

            BuildCatalogVisual(root.transform, def, color);
            var cargo = new GameObject("CargoAnchor").transform;
            cargo.SetParent(root.transform, false);
            cargo.localPosition = def.CargoLocalPosition;

            var controller = root.AddComponent<VehicleController>();
            controller.ApplySpec(def, cargo);
            return controller;
        }

        public static GameObject CreatePreview(VehicleDef def, Color color, Transform parent)
        {
            var root = new GameObject("Preview_" + def.Id);
            root.transform.SetParent(parent, false);
            BuildCatalogVisual(root.transform, def, color);
            return root;
        }

        public static GameObject CreateNpcCar(Vector3 position, Quaternion rotation, Color color, int bodyType)
        {
            var root = new GameObject("NpcCar");
            root.transform.SetPositionAndRotation(position, rotation);

            var body = root.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;

            var collider = root.AddComponent<BoxCollider>();
            collider.size = new Vector3(1.9f, 1.15f, bodyType == 2 ? 3.6f : 4.2f);
            collider.center = new Vector3(0f, 0.65f, 0f);

            BuildNpcVisual(root.transform, color, bodyType);
            return root;
        }

        static void BuildCatalogVisual(Transform parent, VehicleDef def, Color bodyColor)
        {
            switch (def.Style)
            {
                case VehicleStyle.Minibus:
                    BuildMinibus(parent, bodyColor);
                    break;
                case VehicleStyle.Pickup:
                    BuildPickup(parent, bodyColor);
                    break;
                case VehicleStyle.Ambulance:
                    BuildAmbulance(parent, bodyColor);
                    break;
                case VehicleStyle.FireTruck:
                    BuildFireTruck(parent, bodyColor);
                    break;
                case VehicleStyle.IceCream:
                    BuildIceCream(parent, bodyColor);
                    break;
                case VehicleStyle.Race:
                    BuildRace(parent, bodyColor);
                    break;
                default:
                    BuildTaxi(parent, bodyColor);
                    break;
            }
        }

        static readonly Color Window = new Color(0.25f, 0.33f, 0.42f);
        static readonly Color WheelCol = new Color(0.14f, 0.14f, 0.16f);
        static readonly Color Headlight = new Color(1f, 0.95f, 0.6f);
        static readonly Color Taillight = new Color(0.85f, 0.2f, 0.15f);

        static void BuildTaxi(Transform parent, Color bodyColor)
        {
            BuildBasicCar(parent, bodyColor, 4.2f, 0.65f, 0.55f, taxiSign: true);
        }

        static void BuildMinibus(Transform parent, Color bodyColor)
        {
            const float length = 4.8f;
            PartFactory.Create(PrimitiveType.Cube, "Body", parent,
                new Vector3(0f, 0.85f, 0f), new Vector3(2.05f, 1.15f, length), bodyColor);
            PartFactory.Create(PrimitiveType.Cube, "Windows", parent,
                new Vector3(0f, 1.35f, 0.1f), new Vector3(1.95f, 0.55f, length * 0.72f), Window);
            AddWheels(parent, length * 0.32f, 0.95f);
            AddLights(parent, length / 2f, 0.7f);
        }

        static void BuildPickup(Transform parent, Color bodyColor)
        {
            PartFactory.Create(PrimitiveType.Cube, "Cabin", parent,
                new Vector3(0f, 0.85f, 0.85f), new Vector3(1.95f, 1.05f, 1.9f), bodyColor);
            PartFactory.Create(PrimitiveType.Cube, "CabinGlass", parent,
                new Vector3(0f, 1.15f, 0.95f), new Vector3(1.75f, 0.5f, 1.2f), Window);
            PartFactory.Create(PrimitiveType.Cube, "Bed", parent,
                new Vector3(0f, 0.55f, -1.15f), new Vector3(1.9f, 0.45f, 2.2f), bodyColor);
            PartFactory.Create(PrimitiveType.Cube, "RailL", parent,
                new Vector3(-0.88f, 0.95f, -1.15f), new Vector3(0.12f, 0.45f, 2.15f),
                Color.Lerp(bodyColor, Color.black, 0.25f));
            PartFactory.Create(PrimitiveType.Cube, "RailR", parent,
                new Vector3(0.88f, 0.95f, -1.15f), new Vector3(0.12f, 0.45f, 2.15f),
                Color.Lerp(bodyColor, Color.black, 0.25f));
            AddWheels(parent, 1.35f, 0.95f);
            AddLights(parent, 1.85f, 0.62f);
        }

        static void BuildAmbulance(Transform parent, Color bodyColor)
        {
            const float length = 4.7f;
            PartFactory.Create(PrimitiveType.Cube, "Body", parent,
                new Vector3(0f, 0.95f, 0f), new Vector3(2.05f, 1.35f, length), bodyColor);
            PartFactory.Create(PrimitiveType.Cube, "Stripe", parent,
                new Vector3(0f, 0.95f, 0f), new Vector3(2.12f, 0.28f, length * 0.92f),
                new Color(0.85f, 0.18f, 0.18f));
            PartFactory.Create(PrimitiveType.Cube, "CrossV", parent,
                new Vector3(0f, 1.35f, -0.2f), new Vector3(0.22f, 0.7f, 0.22f),
                new Color(0.85f, 0.18f, 0.18f));
            PartFactory.Create(PrimitiveType.Cube, "CrossH", parent,
                new Vector3(0f, 1.35f, -0.2f), new Vector3(0.7f, 0.22f, 0.22f),
                new Color(0.85f, 0.18f, 0.18f));
            PartFactory.Create(PrimitiveType.Cube, "LightBar", parent,
                new Vector3(0f, 1.75f, 0.9f), new Vector3(1.1f, 0.18f, 0.4f),
                new Color(0.2f, 0.45f, 0.95f));
            AddWheels(parent, length * 0.32f, 0.95f);
            AddLights(parent, length / 2f, 0.7f);
        }

        static void BuildFireTruck(Transform parent, Color bodyColor)
        {
            const float length = 5.5f;
            PartFactory.Create(PrimitiveType.Cube, "Cabin", parent,
                new Vector3(0f, 1.0f, 1.55f), new Vector3(2.1f, 1.4f, 1.8f), bodyColor);
            PartFactory.Create(PrimitiveType.Cube, "CabinGlass", parent,
                new Vector3(0f, 1.25f, 1.7f), new Vector3(1.85f, 0.55f, 1.1f), Window);
            PartFactory.Create(PrimitiveType.Cube, "Rear", parent,
                new Vector3(0f, 0.85f, -0.85f), new Vector3(2.05f, 1.1f, 3.4f), bodyColor);
            PartFactory.Create(PrimitiveType.Cube, "Ladder", parent,
                new Vector3(0f, 1.65f, -0.7f), new Vector3(0.45f, 0.18f, 3.6f),
                new Color(0.75f, 0.55f, 0.2f));
            AddWheels(parent, 1.7f, 1.0f);
            AddWheels(parent, 0.15f, 1.0f);
            AddLights(parent, length / 2f, 0.75f);
        }

        static void BuildIceCream(Transform parent, Color bodyColor)
        {
            const float length = 4.5f;
            PartFactory.Create(PrimitiveType.Cube, "Body", parent,
                new Vector3(0f, 0.9f, -0.15f), new Vector3(2.05f, 1.25f, length), bodyColor);
            PartFactory.Create(PrimitiveType.Cube, "Window", parent,
                new Vector3(0f, 1.15f, 1.15f), new Vector3(1.85f, 0.7f, 1.4f), Window);
            PartFactory.Create(PrimitiveType.Cube, "Awning", parent,
                new Vector3(0f, 1.65f, 0.2f), new Vector3(2.15f, 0.12f, 2.2f),
                new Color(1f, 0.45f, 0.55f));
            PartFactory.Create(PrimitiveType.Sphere, "Scoop", parent,
                new Vector3(0f, 2.15f, -0.9f), Vector3.one * 0.85f,
                new Color(1f, 0.85f, 0.9f));
            PartFactory.Create(PrimitiveType.Cylinder, "Cone", parent,
                new Vector3(0f, 1.7f, -0.9f), new Vector3(0.45f, 0.35f, 0.45f),
                new Color(0.92f, 0.7f, 0.35f));
            AddWheels(parent, length * 0.32f, 0.95f);
            AddLights(parent, length / 2f, 0.7f);
        }

        static void BuildRace(Transform parent, Color bodyColor)
        {
            const float length = 3.8f;
            PartFactory.Create(PrimitiveType.Cube, "Body", parent,
                new Vector3(0f, 0.42f, 0f), new Vector3(1.95f, 0.42f, length), bodyColor);
            PartFactory.Create(PrimitiveType.Cube, "Cabin", parent,
                new Vector3(0f, 0.72f, -0.15f), new Vector3(1.5f, 0.35f, 1.3f), Window);
            PartFactory.Create(PrimitiveType.Cube, "Spoiler", parent,
                new Vector3(0f, 0.78f, -1.7f), new Vector3(1.7f, 0.1f, 0.35f), bodyColor);
            PartFactory.Create(PrimitiveType.Cube, "SpoilerPostL", parent,
                new Vector3(-0.55f, 0.62f, -1.55f), new Vector3(0.1f, 0.28f, 0.1f), bodyColor);
            PartFactory.Create(PrimitiveType.Cube, "SpoilerPostR", parent,
                new Vector3(0.55f, 0.62f, -1.55f), new Vector3(0.1f, 0.28f, 0.1f), bodyColor);
            AddWheels(parent, length * 0.32f, 1.0f, 0.32f);
            AddLights(parent, length / 2f, 0.45f);
        }

        static void BuildNpcVisual(Transform parent, Color bodyColor, int bodyType)
        {
            float length = bodyType == 2 ? 3.5f : 4.2f;
            float height = bodyType == 1 ? 0.82f : 0.65f;
            float cabinH = bodyType == 1 ? 0.72f : 0.55f;
            BuildBasicCar(parent, bodyColor, length, height, cabinH, taxiSign: false);
        }

        static void BuildBasicCar(Transform parent, Color bodyColor, float length, float height, float cabinH, bool taxiSign)
        {
            PartFactory.Create(PrimitiveType.Cube, "Body", parent,
                new Vector3(0f, 0.62f, 0f), new Vector3(1.9f, height, length), bodyColor);
            PartFactory.Create(PrimitiveType.Cube, "Cabin", parent,
                new Vector3(0f, 0.62f + height / 2f + cabinH / 2f, -0.2f),
                new Vector3(1.7f, cabinH, length * 0.5f), Window);

            if (taxiSign)
                PartFactory.Create(PrimitiveType.Cube, "TaxiSign", parent,
                    new Vector3(0f, 1.56f, -0.25f), new Vector3(0.55f, 0.22f, 0.7f), Color.white);

            AddWheels(parent, length * 0.32f, 0.95f);
            AddLights(parent, length / 2f + 0.02f, 0.62f);
        }

        static void AddWheels(Transform parent, float wheelZ, float x, float y = 0.36f)
        {
            var scale = new Vector3(0.72f, 0.16f, 0.72f);
            CreateWheel(parent, new Vector3(-x, y, wheelZ), scale);
            CreateWheel(parent, new Vector3(x, y, wheelZ), scale);
            CreateWheel(parent, new Vector3(-x, y, -wheelZ), scale);
            CreateWheel(parent, new Vector3(x, y, -wheelZ), scale);
        }

        static void AddLights(Transform parent, float bumper, float y)
        {
            PartFactory.Create(PrimitiveType.Cube, "HeadlightL", parent,
                new Vector3(-0.6f, y, bumper), new Vector3(0.4f, 0.2f, 0.1f), Headlight);
            PartFactory.Create(PrimitiveType.Cube, "HeadlightR", parent,
                new Vector3(0.6f, y, bumper), new Vector3(0.4f, 0.2f, 0.1f), Headlight);
            PartFactory.Create(PrimitiveType.Cube, "TaillightL", parent,
                new Vector3(-0.6f, y, -bumper), new Vector3(0.4f, 0.2f, 0.1f), Taillight);
            PartFactory.Create(PrimitiveType.Cube, "TaillightR", parent,
                new Vector3(0.6f, y, -bumper), new Vector3(0.4f, 0.2f, 0.1f), Taillight);
        }

        static void CreateWheel(Transform parent, Vector3 localPosition, Vector3 scale)
        {
            var wheel = PartFactory.Create(PrimitiveType.Cylinder, "Wheel", parent, localPosition, scale, WheelCol);
            wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        }
    }
}
