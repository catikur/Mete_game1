using System.Collections.Generic;
using UnityEngine;

namespace MeteGame.Core
{
    /// <summary>
    /// Renk başına tek paylaşılan materyal üretir; aynı renkteki tüm nesneler
    /// aynı materyali kullanır (batching ve bellek dostu).
    /// </summary>
    public static class MaterialLibrary
    {
        static readonly Dictionary<int, Material> Cache = new Dictionary<int, Material>();
        static Material _baseMaterial;

        static Material BaseMaterial
        {
            get
            {
                if (_baseMaterial == null)
                {
                    // Editor kurulumunun oluşturduğu asset; build'e shader'ın dahil olmasını da sağlar.
                    _baseMaterial = Resources.Load<Material>("Materials/MeteLit");
                    if (_baseMaterial == null)
                    {
                        var shader = Shader.Find("Universal Render Pipeline/Lit");
                        if (shader == null)
                            shader = Shader.Find("Standard");
                        _baseMaterial = new Material(shader);
                        if (_baseMaterial.HasProperty("_Smoothness"))
                            _baseMaterial.SetFloat("_Smoothness", 0.18f);
                    }
                }
                return _baseMaterial;
            }
        }

        public static Material Get(Color color)
        {
            Color32 c = color;
            int key = (c.r << 24) | (c.g << 16) | (c.b << 8) | c.a;
            if (!Cache.TryGetValue(key, out var material) || material == null)
            {
                material = new Material(BaseMaterial) { color = color };
                Cache[key] = material;
            }
            return material;
        }
    }
}
