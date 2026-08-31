using MeteGame.Core;
using UnityEngine;

namespace MeteGame.City
{
    /// <summary>
    /// Grid tabanlı prosedürel şehir: yollar, şerit çizgileri, kaldırımlı bloklar,
    /// pastel binalar, ağaçlı parklar ve şehri çevreleyen çit.
    /// Sabit tohum kullanır — şehir her oyunda aynıdır.
    /// </summary>
    public static class CityBuilder
    {
        public static CityLayout Build(Transform parent)
        {
            int blocks = GameConfig.CityBlocks;
            float road = GameConfig.RoadWidth;
            float block = GameConfig.BlockSize;
            float pitch = GameConfig.CityPitch;
            float extent = GameConfig.CityExtent;
            float half = extent / 2f;
            var rng = new System.Random(GameConfig.CitySeed);

            var cityRoot = new GameObject("City").transform;
            cityRoot.SetParent(parent, false);

            // Zemin: üst yüzeyi tam y=0'da duran geniş bir plaka.
            PartFactory.Create(PrimitiveType.Cube, "Ground", cityRoot,
                new Vector3(0f, -0.5f, 0f), new Vector3(extent + 60f, 1f, extent + 60f),
                GameConfig.Grass, withBoxCollider: true, castShadows: false);

            // Yol merkez koordinatları (blocks + 1 yol her yönde).
            var xs = new float[blocks + 1];
            var zs = new float[blocks + 1];
            for (int i = 0; i <= blocks; i++)
            {
                xs[i] = -half + road / 2f + i * pitch;
                zs[i] = -half + road / 2f + i * pitch;
            }

            BuildRoads(cityRoot, xs, zs, extent, road);
            BuildBlocks(cityRoot, xs, zs, blocks, block, road, rng);
            BuildBoundary(cityRoot, half);

            var layout = new CityLayout { RoadXs = xs, RoadZs = zs, HalfExtent = half };
            BuildCrosswalks(cityRoot, layout);
            BuildParkedCars(cityRoot, layout, rng);
            return layout;
        }

        static void BuildRoads(Transform cityRoot, float[] xs, float[] zs, float extent, float road)
        {
            var roadsRoot = new GameObject("Roads").transform;
            roadsRoot.SetParent(cityRoot, false);

            // Dikey ve yatay yol şeritleri. Küçük y farkları z-fighting'i önler,
            // kuş bakışı kamerada fark edilmez.
            foreach (float x in xs)
                PartFactory.Create(PrimitiveType.Cube, "RoadV", roadsRoot,
                    new Vector3(x, 0.02f, 0f), new Vector3(road, 0.04f, extent),
                    GameConfig.Road, castShadows: false);

            foreach (float z in zs)
                PartFactory.Create(PrimitiveType.Cube, "RoadH", roadsRoot,
                    new Vector3(0f, 0.05f, z), new Vector3(extent, 0.04f, road),
                    GameConfig.Road, castShadows: false);

            // Kesikli şerit çizgileri (kavşaklara taşmaz).
            foreach (float x in xs)
            {
                for (int j = 0; j < zs.Length - 1; j++)
                {
                    float start = zs[j] + road / 2f + 2f;
                    float end = zs[j + 1] - road / 2f - 2f;
                    for (float z = start; z + 2.5f <= end; z += 7f)
                        PartFactory.Create(PrimitiveType.Cube, "Dash", roadsRoot,
                            new Vector3(x, 0.08f, z + 1.25f), new Vector3(0.35f, 0.02f, 2.5f),
                            GameConfig.RoadMarking, castShadows: false);
                }
            }

            foreach (float z in zs)
            {
                for (int i = 0; i < xs.Length - 1; i++)
                {
                    float start = xs[i] + road / 2f + 2f;
                    float end = xs[i + 1] - road / 2f - 2f;
                    for (float x = start; x + 2.5f <= end; x += 7f)
                        PartFactory.Create(PrimitiveType.Cube, "Dash", roadsRoot,
                            new Vector3(x + 1.25f, 0.08f, z), new Vector3(2.5f, 0.02f, 0.35f),
                            GameConfig.RoadMarking, castShadows: false);
                }
            }
        }

        static void BuildBlocks(Transform cityRoot, float[] xs, float[] zs, int blocks, float block, float road, System.Random rng)
        {
            var blocksRoot = new GameObject("Blocks").transform;
            blocksRoot.SetParent(cityRoot, false);

            for (int i = 0; i < blocks; i++)
            {
                for (int j = 0; j < blocks; j++)
                {
                    float cx = xs[i] + road / 2f + block / 2f;
                    float cz = zs[j] + road / 2f + block / 2f;

                    // Kaldırım platformu (görsel, collider'sız — araç takılmasın).
                    PartFactory.Create(PrimitiveType.Cube, "Sidewalk", blocksRoot,
                        new Vector3(cx, 0.03f, cz), new Vector3(block, 0.06f, block),
                        GameConfig.Sidewalk, castShadows: false);

                    // Her blok 2x2 parsele bölünür: bina veya park.
                    float lot = block / 2f;
                    for (int u = 0; u < 2; u++)
                    {
                        for (int v = 0; v < 2; v++)
                        {
                            float px = cx - lot / 2f + u * lot;
                            float pz = cz - lot / 2f + v * lot;
                            if (rng.NextDouble() < 0.18)
                                BuildPark(blocksRoot, px, pz, lot, rng);
                            else
                                BuildBuilding(blocksRoot, px, pz, lot, rng);
                        }
                    }
                }
            }
        }

        static void BuildBuilding(Transform parent, float x, float z, float lot, System.Random rng)
        {
            float width = lot - 3.2f - (float)rng.NextDouble() * 2f;
            float depth = lot - 3.2f - (float)rng.NextDouble() * 2f;
            float height = Mathf.Lerp(4f, 16f, (float)rng.NextDouble());
            Color color = GameConfig.BuildingPalette[rng.Next(GameConfig.BuildingPalette.Length)];

            PartFactory.Create(PrimitiveType.Cube, "Building", parent,
                new Vector3(x, 0.06f + height / 2f, z), new Vector3(width, height, depth),
                color, withBoxCollider: true);

            PartFactory.Create(PrimitiveType.Cube, "Roof", parent,
                new Vector3(x, 0.06f + height + 0.25f, z), new Vector3(width * 0.85f, 0.5f, depth * 0.85f),
                Color.Lerp(color, Color.black, 0.25f));
        }

        static void BuildPark(Transform parent, float x, float z, float lot, System.Random rng)
        {
            PartFactory.Create(PrimitiveType.Cube, "ParkGrass", parent,
                new Vector3(x, 0.065f, z), new Vector3(lot - 2f, 0.05f, lot - 2f),
                GameConfig.Grass, castShadows: false);

            int treeCount = 2 + rng.Next(2);
            for (int t = 0; t < treeCount; t++)
            {
                float tx = x + ((float)rng.NextDouble() - 0.5f) * (lot - 6f);
                float tz = z + ((float)rng.NextDouble() - 0.5f) * (lot - 6f);
                BuildTree(parent, tx, tz, rng);
            }
        }

        static void BuildTree(Transform parent, float x, float z, System.Random rng)
        {
            float s = 0.8f + (float)rng.NextDouble() * 0.6f;
            PartFactory.Create(PrimitiveType.Cylinder, "Trunk", parent,
                new Vector3(x, 0.8f * s, z), new Vector3(0.4f * s, 0.8f * s, 0.4f * s),
                GameConfig.TreeTrunk);
            PartFactory.Create(PrimitiveType.Sphere, "Crown", parent,
                new Vector3(x, 2.1f * s, z), new Vector3(2.4f * s, 2.2f * s, 2.4f * s),
                GameConfig.TreeCrown);
        }

        static void BuildBoundary(Transform cityRoot, float half)
        {
            const float thickness = 2f;
            const float height = 1.8f;
            float length = half * 2f + thickness * 2f;

            var walls = new[]
            {
                new { name = "HedgeN", pos = new Vector3(0f, height / 2f, half + thickness / 2f), scale = new Vector3(length, height, thickness) },
                new { name = "HedgeS", pos = new Vector3(0f, height / 2f, -half - thickness / 2f), scale = new Vector3(length, height, thickness) },
                new { name = "HedgeE", pos = new Vector3(half + thickness / 2f, height / 2f, 0f), scale = new Vector3(thickness, height, length) },
                new { name = "HedgeW", pos = new Vector3(-half - thickness / 2f, height / 2f, 0f), scale = new Vector3(thickness, height, length) }
            };

            foreach (var wall in walls)
                PartFactory.Create(PrimitiveType.Cube, wall.name, cityRoot,
                    wall.pos, wall.scale, GameConfig.Hedge, withBoxCollider: true);
        }

        static void BuildCrosswalks(Transform cityRoot, CityLayout layout)
        {
            var root = new GameObject("Crosswalks").transform;
            root.SetParent(cityRoot, false);

            int n = layout.RoadCount;
            float road = GameConfig.RoadWidth;
            const int stripes = 5;
            const float stripeW = 0.55f;
            const float stripeLen = 2.2f;

            for (int ix = 0; ix < n; ix++)
            {
                for (int iz = 0; iz < n; iz++)
                {
                    Vector3 c = layout.IntersectionCenter(ix, iz);
                    PlaceZebra(root, c, northSouthWalk: true, +1, road, stripes, stripeW, stripeLen);
                    PlaceZebra(root, c, northSouthWalk: true, -1, road, stripes, stripeW, stripeLen);
                    PlaceZebra(root, c, northSouthWalk: false, +1, road, stripes, stripeW, stripeLen);
                    PlaceZebra(root, c, northSouthWalk: false, -1, road, stripes, stripeW, stripeLen);
                }
            }
        }

        static void PlaceZebra(Transform parent, Vector3 center, bool northSouthWalk, int sign,
            float road, int stripes, float stripeW, float stripeLen)
        {
            float offset = road / 2f + 1.15f;
            Vector3 origin = northSouthWalk
                ? center + new Vector3(0f, 0.09f, sign * offset)
                : center + new Vector3(sign * offset, 0.09f, 0f);

            float span = road - 1.6f;
            for (int s = 0; s < stripes; s++)
            {
                float t = (s + 0.5f) / stripes;
                float along = Mathf.Lerp(-span / 2f, span / 2f, t);
                Vector3 pos = northSouthWalk
                    ? origin + new Vector3(along, 0f, 0f)
                    : origin + new Vector3(0f, 0f, along);
                Vector3 scale = northSouthWalk
                    ? new Vector3(stripeW, 0.02f, stripeLen)
                    : new Vector3(stripeLen, 0.02f, stripeW);
                PartFactory.Create(PrimitiveType.Cube, "Zebra", parent, pos, scale,
                    GameConfig.RoadMarking, castShadows: false);
            }
        }

        static void BuildParkedCars(Transform cityRoot, CityLayout layout, System.Random rng)
        {
            var root = new GameObject("ParkedCars").transform;
            root.SetParent(cityRoot, false);

            int blocks = GameConfig.CityBlocks;
            float road = GameConfig.RoadWidth;
            float block = GameConfig.BlockSize;
            Color wheel = new Color(0.14f, 0.14f, 0.16f);

            for (int i = 0; i < blocks; i++)
            {
                for (int j = 0; j < blocks; j++)
                {
                    float cx = layout.RoadXs[i] + road / 2f + block / 2f;
                    float cz = layout.RoadZs[j] + road / 2f + block / 2f;

                    TryPark(root, rng, new Vector3(cx - block / 2f + 1.5f, 0f, cz + RandomAlong(rng, block)),
                        Compass.North, wheel);
                    TryPark(root, rng, new Vector3(cx + block / 2f - 1.5f, 0f, cz + RandomAlong(rng, block)),
                        Compass.South, wheel);
                    TryPark(root, rng, new Vector3(cx + RandomAlong(rng, block), 0f, cz - block / 2f + 1.5f),
                        Compass.East, wheel);
                    TryPark(root, rng, new Vector3(cx + RandomAlong(rng, block), 0f, cz + block / 2f - 1.5f),
                        Compass.West, wheel);
                }
            }
        }

        static float RandomAlong(System.Random rng, float block)
        {
            return Mathf.Lerp(-block / 2f + 5f, block / 2f - 5f, (float)rng.NextDouble());
        }

        static void TryPark(Transform parent, System.Random rng, Vector3 pos, Compass facing, Color wheel)
        {
            if (rng.NextDouble() > 0.42)
                return;

            Color body = GameConfig.CarPalette[rng.Next(GameConfig.CarPalette.Length)];
            var car = new GameObject("ParkedCar");
            car.transform.SetParent(parent, false);
            car.transform.SetPositionAndRotation(pos + Vector3.up * 0.36f, CompassUtil.Rotation(facing));

            var box = car.AddComponent<BoxCollider>();
            box.size = new Vector3(1.8f, 1.1f, 4.0f);
            box.center = new Vector3(0f, 0.2f, 0f);

            PartFactory.Create(PrimitiveType.Cube, "Body", car.transform,
                new Vector3(0f, 0.2f, 0f), new Vector3(1.7f, 0.55f, 3.8f), body);
            PartFactory.Create(PrimitiveType.Cube, "Cabin", car.transform,
                new Vector3(0f, 0.65f, -0.2f), new Vector3(1.5f, 0.45f, 1.8f),
                new Color(0.25f, 0.33f, 0.42f));
            CreateParkedWheel(car.transform, new Vector3(-0.85f, -0.05f, 1.2f), wheel);
            CreateParkedWheel(car.transform, new Vector3(0.85f, -0.05f, 1.2f), wheel);
            CreateParkedWheel(car.transform, new Vector3(-0.85f, -0.05f, -1.2f), wheel);
            CreateParkedWheel(car.transform, new Vector3(0.85f, -0.05f, -1.2f), wheel);
        }

        static void CreateParkedWheel(Transform parent, Vector3 localPosition, Color color)
        {
            var wheel = PartFactory.Create(PrimitiveType.Cylinder, "Wheel", parent,
                localPosition, new Vector3(0.6f, 0.14f, 0.6f), color);
            wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        }
    }
}
