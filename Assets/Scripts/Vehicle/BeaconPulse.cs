using MeteGame.Core;
using UnityEngine;

namespace MeteGame.Vehicle
{
    /// <summary>Hırsız arabasının tavan ışığı — çocukların takip etmesi için nabız.</summary>
    public class BeaconPulse : MonoBehaviour
    {
        static readonly Color Hot = new Color(1f, 0.62f, 0.12f);
        static readonly Color Dim = new Color(0.55f, 0.22f, 0.08f);
        MeshRenderer _renderer;

        void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
        }

        void Update()
        {
            bool on = Mathf.Sin(Time.time * 10f) > 0f;
            if (_renderer != null)
                _renderer.sharedMaterial = MaterialLibrary.Get(on ? Hot : Dim);
        }
    }
}
