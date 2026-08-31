using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MeteGame.Core
{
    /// <summary>
    /// Unity'nin yerleşik primitive mesh'lerinden (küp, küre, silindir...)
    /// renkli oyun parçaları üretir. Prototipin tüm görselleri buradan çıkar.
    /// </summary>
    public static class PartFactory
    {
        static readonly Dictionary<PrimitiveType, Mesh> MeshCache = new Dictionary<PrimitiveType, Mesh>();

        static Mesh GetMesh(PrimitiveType type)
        {
            if (!MeshCache.TryGetValue(type, out var mesh) || mesh == null)
            {
                // Sürümden bağımsız güvenli yol: geçici primitive'den mesh'i al.
                var temp = GameObject.CreatePrimitive(type);
                temp.SetActive(false);
                mesh = temp.GetComponent<MeshFilter>().sharedMesh;
                Object.Destroy(temp);
                MeshCache[type] = mesh;
            }
            return mesh;
        }

        public static GameObject Create(
            PrimitiveType type,
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Color color,
            bool withBoxCollider = false,
            bool castShadows = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;

            var filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = GetMesh(type);

            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = MaterialLibrary.Get(color);
            if (!castShadows)
                renderer.shadowCastingMode = ShadowCastingMode.Off;

            if (withBoxCollider)
                go.AddComponent<BoxCollider>();

            return go;
        }
    }
}
