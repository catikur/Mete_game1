using System.Collections.Generic;
using MeteGame.Vehicle;
using UnityEngine;

namespace MeteGame.Core
{
    /// <summary>
    /// Kenney (CC0) modellerini Resources'tan yükler. Unity henüz FBX import
    /// etmediyse <c>null</c> döner — çağıran primitive'e düşer.
    /// Meshy özel aracı: <c>Resources/Vehicles/&lt;id&gt;</c> varsa o tercih edilir.
    /// </summary>
    public static class KenneyLibrary
    {
        const string VehiclesFolder = "Kenney/Vehicles/";
        const string CityFolder = "Kenney/City/";
        const string CustomFolder = "Vehicles/";

        static readonly Dictionary<string, GameObject> PrefabCache = new Dictionary<string, GameObject>();
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int ColorId = Shader.PropertyToID("_Color");

        static int _loadStatus; // 0 henüz, 1 Kenney var, 2 yok

        public static bool TryAttachCatalogVehicle(Transform parent, VehicleDef def, Color paint)
        {
            if (def == null)
                return false;

            var custom = LoadPrefab(CustomFolder + def.Id);
            if (custom != null)
                return Attach(parent, custom, def.Id, def.ColliderSize, paint);

            string model = CatalogModelName(def.Style);
            var kenney = LoadPrefab(VehiclesFolder + model);
            if (kenney == null)
            {
                NoteMissing();
                return false;
            }

            NoteLoaded();
            return Attach(parent, kenney, model, def.ColliderSize, paint);
        }

        public static bool TryAttachNamedVehicle(Transform parent, string modelName, Vector3 targetSize, Color paint)
        {
            var kenney = LoadPrefab(VehiclesFolder + modelName);
            if (kenney == null)
            {
                NoteMissing();
                return false;
            }

            NoteLoaded();
            return Attach(parent, kenney, modelName, targetSize, paint);
        }

        public static bool TryAttachBuilding(Transform parent, System.Random rng, float targetWidth)
        {
            int index = rng.Next(21);
            char letter = (char)('a' + index);
            var prefab = LoadPrefab(CityFolder + "building-type-" + letter);
            if (prefab == null)
            {
                NoteMissing();
                return false;
            }

            NoteLoaded();
            var go = Object.Instantiate(prefab, parent, false);
            go.name = "KenneyBuilding_" + letter;
            StripImportedColliders(go);
            FitByMaxFootprint(go, targetWidth);
            go.transform.localRotation = Quaternion.Euler(0f, 90f * rng.Next(4), 0f);
            return true;
        }

        public static bool TryAttachTree(Transform parent, Vector3 localPosition, bool large, float height)
        {
            string name = large ? "tree-large" : "tree-small";
            var prefab = LoadPrefab(CityFolder + name);
            if (prefab == null)
            {
                NoteMissing();
                return false;
            }

            NoteLoaded();
            var go = Object.Instantiate(prefab, parent, false);
            go.name = "KenneyTree";
            go.transform.localPosition = localPosition;
            StripImportedColliders(go);
            FitByHeight(go, height);
            return true;
        }

        public static string NpcModelName(int bodyType)
        {
            switch (bodyType % 3)
            {
                case 1: return "suv";
                case 2: return "hatchback-sports";
                default: return "sedan";
            }
        }

        public static string ParkedModelName(System.Random rng)
        {
            int roll = rng.Next(5);
            switch (roll)
            {
                case 0: return "taxi";
                case 1: return "suv";
                case 2: return "hatchback-sports";
                case 3: return "van";
                default: return "sedan";
            }
        }

        public static bool TryGetLocalBounds(Transform root, out Bounds bounds)
        {
            bounds = new Bounds();
            bool any = false;
            var filters = root.GetComponentsInChildren<MeshFilter>();
            for (int i = 0; i < filters.Length; i++)
            {
                var mesh = filters[i].sharedMesh;
                if (mesh == null)
                    continue;

                var meshBounds = mesh.bounds;
                Vector3 min = meshBounds.min;
                Vector3 max = meshBounds.max;
                for (int c = 0; c < 8; c++)
                {
                    var corner = new Vector3(
                        (c & 1) == 0 ? min.x : max.x,
                        (c & 2) == 0 ? min.y : max.y,
                        (c & 4) == 0 ? min.z : max.z);
                    Vector3 world = filters[i].transform.TransformPoint(corner);
                    Vector3 local = root.InverseTransformPoint(world);
                    if (!any)
                    {
                        bounds = new Bounds(local, Vector3.zero);
                        any = true;
                    }
                    else
                    {
                        bounds.Encapsulate(local);
                    }
                }
            }

            return any;
        }

        static string CatalogModelName(VehicleStyle style)
        {
            switch (style)
            {
                case VehicleStyle.Minibus: return "van";
                case VehicleStyle.Pickup: return "truck-flat";
                case VehicleStyle.Ambulance: return "ambulance";
                case VehicleStyle.Police: return "police";
                case VehicleStyle.FireTruck: return "firetruck";
                case VehicleStyle.IceCream: return "van";
                case VehicleStyle.Race: return "race";
                default: return "taxi";
            }
        }

        static GameObject LoadPrefab(string resourcesPath)
        {
            if (PrefabCache.TryGetValue(resourcesPath, out var cached))
                return cached;

            var loaded = Resources.Load<GameObject>(resourcesPath);
            PrefabCache[resourcesPath] = loaded;
            return loaded;
        }

        static bool Attach(Transform parent, GameObject prefab, string name, Vector3 targetSize, Color paint)
        {
            var go = Object.Instantiate(prefab, parent, false);
            go.name = "Kenney_" + name;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            StripImportedColliders(go);
            FitToFootprint(go, targetSize);
            ApplyPaint(go, paint);
            return true;
        }

        static void FitToFootprint(GameObject go, Vector3 targetSize)
        {
            go.transform.localScale = Vector3.one;
            if (!TryGetLocalBounds(go.transform, out var bounds))
                return;

            float sx = bounds.size.x > 0.05f ? targetSize.x / bounds.size.x : 1f;
            float sz = bounds.size.z > 0.05f ? targetSize.z / bounds.size.z : 1f;
            float s = Mathf.Min(sx, sz);
            if (s < 0.05f || float.IsNaN(s) || float.IsInfinity(s))
                s = 1f;
            go.transform.localScale = Vector3.one * s;
        }

        static void FitByMaxFootprint(GameObject go, float targetWidth)
        {
            go.transform.localScale = Vector3.one;
            if (!TryGetLocalBounds(go.transform, out var bounds))
                return;

            float span = Mathf.Max(bounds.size.x, bounds.size.z);
            if (span < 0.05f)
                return;
            float s = targetWidth / span;
            if (s < 0.05f || float.IsNaN(s) || float.IsInfinity(s))
                s = 1f;
            go.transform.localScale = Vector3.one * s;
        }

        static void FitByHeight(GameObject go, float targetHeight)
        {
            go.transform.localScale = Vector3.one;
            if (!TryGetLocalBounds(go.transform, out var bounds))
                return;

            if (bounds.size.y < 0.05f)
                return;
            float s = targetHeight / bounds.size.y;
            if (s < 0.05f || float.IsNaN(s) || float.IsInfinity(s))
                s = 1f;
            go.transform.localScale = Vector3.one * s;
        }

        static void ApplyPaint(GameObject go, Color paint)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return;

            Color tint = Color.Lerp(Color.white, paint, 0.5f);
            var block = new MaterialPropertyBlock();
            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                r.GetPropertyBlock(block);
                block.SetColor(BaseColorId, tint);
                block.SetColor(ColorId, tint);
                r.SetPropertyBlock(block);
            }
        }

        static void StripImportedColliders(GameObject go)
        {
            var colliders = go.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
                Object.DestroyImmediate(colliders[i]);
        }

        static void NoteLoaded()
        {
            if (_loadStatus == 1)
                return;
            _loadStatus = 1;
            Debug.Log("[Mete Oyunu] Kenney modelleri yüklendi.");
        }

        static void NoteMissing()
        {
            if (_loadStatus != 0)
                return;
            _loadStatus = 2;
            Debug.LogWarning("[Mete Oyunu] Kenney FBX henüz yok veya import edilmedi — primitive görsel kullanılıyor. Unity'de bir kez açıp Play'e bas; pembe olursa Mete Oyunu → Projeyi Kur.");
        }
    }
}
