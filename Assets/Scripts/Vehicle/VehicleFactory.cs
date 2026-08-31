using MeteGame.Core;
using UnityEngine;

namespace MeteGame.Vehicle
{
    /// <summary>
    /// Primitive parçalardan oyuncu aracını kurar. M4'te Kenney/Meshy prefabları
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

            BuildTaxiVisual(root.transform);

            return root.AddComponent<VehicleController>();
        }

        static void BuildTaxiVisual(Transform parent)
        {
            Color bodyColor = GameConfig.TaxiYellow;
            Color windowColor = new Color(0.25f, 0.33f, 0.42f);
            Color wheelColor = new Color(0.14f, 0.14f, 0.16f);

            // Kasa, kabin ve taksi tabelası
            PartFactory.Create(PrimitiveType.Cube, "Body", parent,
                new Vector3(0f, 0.62f, 0f), new Vector3(1.9f, 0.65f, 4.2f), bodyColor);
            PartFactory.Create(PrimitiveType.Cube, "Cabin", parent,
                new Vector3(0f, 1.18f, -0.25f), new Vector3(1.7f, 0.55f, 2.1f), windowColor);
            PartFactory.Create(PrimitiveType.Cube, "TaxiSign", parent,
                new Vector3(0f, 1.56f, -0.25f), new Vector3(0.55f, 0.22f, 0.7f), Color.white);

            // Tekerlekler (silindir, X eksenine yatırılır)
            var wheelScale = new Vector3(0.72f, 0.16f, 0.72f);
            CreateWheel(parent, new Vector3(-0.95f, 0.36f, 1.35f), wheelScale, wheelColor);
            CreateWheel(parent, new Vector3(0.95f, 0.36f, 1.35f), wheelScale, wheelColor);
            CreateWheel(parent, new Vector3(-0.95f, 0.36f, -1.35f), wheelScale, wheelColor);
            CreateWheel(parent, new Vector3(0.95f, 0.36f, -1.35f), wheelScale, wheelColor);

            // Farlar (ön, sıcak sarı) ve stoplar (arka, kırmızı)
            Color headlight = new Color(1f, 0.95f, 0.6f);
            Color taillight = new Color(0.85f, 0.2f, 0.15f);
            PartFactory.Create(PrimitiveType.Cube, "HeadlightL", parent,
                new Vector3(-0.6f, 0.62f, 2.12f), new Vector3(0.4f, 0.2f, 0.1f), headlight);
            PartFactory.Create(PrimitiveType.Cube, "HeadlightR", parent,
                new Vector3(0.6f, 0.62f, 2.12f), new Vector3(0.4f, 0.2f, 0.1f), headlight);
            PartFactory.Create(PrimitiveType.Cube, "TaillightL", parent,
                new Vector3(-0.6f, 0.62f, -2.12f), new Vector3(0.4f, 0.2f, 0.1f), taillight);
            PartFactory.Create(PrimitiveType.Cube, "TaillightR", parent,
                new Vector3(0.6f, 0.62f, -2.12f), new Vector3(0.4f, 0.2f, 0.1f), taillight);
        }

        static void CreateWheel(Transform parent, Vector3 localPosition, Vector3 scale, Color color)
        {
            var wheel = PartFactory.Create(PrimitiveType.Cylinder, "Wheel", parent, localPosition, scale, color);
            wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        }
    }
}
