using MeteGame.Core;
using UnityEngine;

namespace MeteGame.Vehicle
{
    /// <summary>Polis tavan lambası: kırmızı / mavi nöbetleşe yanar.</summary>
    public class LightBarBlink : MonoBehaviour
    {
        MeshRenderer _left;
        MeshRenderer _right;

        static readonly Color RedOn = new Color(1f, 0.18f, 0.18f);
        static readonly Color BlueOn = new Color(0.22f, 0.48f, 1f);
        static readonly Color Off = new Color(0.22f, 0.22f, 0.26f);

        public void Bind(MeshRenderer left, MeshRenderer right)
        {
            _left = left;
            _right = right;
        }

        void Update()
        {
            bool leftOn = Mathf.Repeat(Time.time, 0.36f) < 0.18f;
            if (_left != null)
                _left.sharedMaterial = MaterialLibrary.Get(leftOn ? RedOn : Off);
            if (_right != null)
                _right.sharedMaterial = MaterialLibrary.Get(leftOn ? Off : BlueOn);
        }
    }
}
