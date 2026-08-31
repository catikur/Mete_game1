using UnityEngine;

namespace MeteGame.Missions
{
    /// <summary>Teslimata kadar çatıdaki paketin hafif zıplaması.</summary>
    public class CargoBob : MonoBehaviour
    {
        Vector3 _baseLocal;
        Vector3 _baseScale;

        void Awake()
        {
            _baseLocal = transform.localPosition;
            _baseScale = transform.localScale;
        }

        void Update()
        {
            float bounce = Mathf.Abs(Mathf.Sin(Time.time * 7f));
            var pos = _baseLocal;
            pos.y += bounce * 0.12f;
            transform.localPosition = pos;
            transform.localScale = _baseScale * (1f + bounce * 0.06f);
            transform.localRotation = Quaternion.Euler(0f, Time.time * 70f, Mathf.Sin(Time.time * 5f) * 8f);
        }
    }
}
