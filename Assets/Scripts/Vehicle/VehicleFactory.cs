using MeteGame.Core;
using UnityEngine;

namespace MeteGame.Vehicle
{
    /// <summary>
    /// Primitive parçalardan oyuncu ve NPC araçlarını kurar. M4'te Kenney/Meshy prefabları
    /// aynı arayüzden yüklenecek; primitive gövde fallback olarak kalacak.
    /// </summary>
    public static class VehicleFactory
    {
        public static VehicleController CreatePlayerVehicle(Vector3 position, Quaternion rotation)
        {
            var root = new GameObject("PlayerVehicle");
            root.transform.SetPositionAndRotation(position, rotation);

            var body = root.AddComponent<Rigidbody>();
            body.mass = 1200f;
            body.linearDamping = 0f;
            body.angularDamping = 8f;

            var collider = root.AddComponent<BoxCollider>();
            collider.size = new Vector3(2.0f, 1.2f, 4.4f);
            collider.center = new Vector3(0f, 0.7f, 0f);

            BuildCarVisual(root.transform, GameConfig.TaxiYellow, bodyType: 0, taxiSign: true);

            return root.AddComponent<VehicleController>();
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

            BuildCarVisual(root.transform, color, bodyType, taxiSign: false);
            return root;
        }

        static void BuildCarVisual(Transform parent, Color bodyColor, int bodyType, bool taxiSign)
        {
            float length = bodyType == 2 ? 3.5f : 4.2f;
            float height = bodyType == 1 ? 0.82f : 0.65f;
            float cabinH = bodyType == 1 ? 0.72f : 0.55f;
            Color windowColor = new Color(0.25f, 0.33f, 0.42f);
            Color wheelColor = new Color(0.14f, 0.14f, 0.16f);

            PartFactory.Create(PrimitiveType.Cube, "Body", parent,
                new Vector3(0f, 0.62f, 0f), new Vector3(1.9f, height, length), bodyColor);
            PartFactory.Create(PrimitiveType.Cube, "Cabin", parent,
                new Vector3(0f, 0.62f + height / 2f + cabinH / 2f, -0.2f),
                new Vector3(1.7f, cabinH, length * 0.5f), windowColor);

            if (taxiSign)
                PartFactory.Create(PrimitiveType.Cube, "TaxiSign", parent,
                    new Vector3(0f, 1.56f, -0.25f), new Vector3(0.55f, 0.22f, 0.7f), Color.white);

            float wheelZ = length * 0.32f;
            var wheelScale = new Vector3(0.72f, 0.16f, 0.72f);
            CreateWheel(parent, new Vector3(-0.95f, 0.36f, wheelZ), wheelScale, wheelColor);
            CreateWheel(parent, new Vector3(0.95f, 0.36f, wheelZ), wheelScale, wheelColor);
            CreateWheel(parent, new Vector3(-0.95f, 0.36f, -wheelZ), wheelScale, wheelColor);
            CreateWheel(parent, new Vector3(0.95f, 0.36f, -wheelZ), wheelScale, wheelColor);

            Color headlight = new Color(1f, 0.95f, 0.6f);
            Color taillight = new Color(0.85f, 0.2f, 0.15f);
            float bumper = length / 2f + 0.02f;
            PartFactory.Create(PrimitiveType.Cube, "HeadlightL", parent,
                new Vector3(-0.6f, 0.62f, bumper), new Vector3(0.4f, 0.2f, 0.1f), headlight);
            PartFactory.Create(PrimitiveType.Cube, "HeadlightR", parent,
                new Vector3(0.6f, 0.62f, bumper), new Vector3(0.4f, 0.2f, 0.1f), headlight);
            PartFactory.Create(PrimitiveType.Cube, "TaillightL", parent,
                new Vector3(-0.6f, 0.62f, -bumper), new Vector3(0.4f, 0.2f, 0.1f), taillight);
            PartFactory.Create(PrimitiveType.Cube, "TaillightR", parent,
                new Vector3(0.6f, 0.62f, -bumper), new Vector3(0.4f, 0.2f, 0.1f), taillight);
        }

        static void CreateWheel(Transform parent, Vector3 localPosition, Vector3 scale, Color color)
        {
            var wheel = PartFactory.Create(PrimitiveType.Cylinder, "Wheel", parent, localPosition, scale, color);
            wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        }
    }
}
