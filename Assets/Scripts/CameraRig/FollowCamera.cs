using UnityEngine;

namespace MeteGame.CameraRig
{
    /// <summary>
    /// GTA 2 tarzı kuş bakışı takip kamerası: kuzeyi sabit (dönmez), eğimli bakar,
    /// araç hızlandıkça hafifçe ileriyi gösterir.
    /// </summary>
    public class FollowCamera : MonoBehaviour
    {
        public float distance = 32f;
        public float pitchDegrees = 62f;
        public float lookAheadTime = 0.7f;
        public float maxLookAhead = 12f;
        public float smoothTime = 0.22f;

        Transform _target;
        Rigidbody _targetBody;
        Vector3 _offset;
        Vector3 _velocity;

        public void SetTarget(Transform target, Rigidbody targetBody)
        {
            _target = target;
            _targetBody = targetBody;

            var rotation = Quaternion.Euler(pitchDegrees, 0f, 0f);
            transform.rotation = rotation;
            _offset = -(rotation * Vector3.forward) * distance;
            transform.position = target.position + _offset;
        }

        void LateUpdate()
        {
            if (_target == null)
                return;

            Vector3 lookAhead = Vector3.zero;
            if (_targetBody != null)
            {
                lookAhead = _targetBody.linearVelocity * lookAheadTime;
                lookAhead.y = 0f;
                lookAhead = Vector3.ClampMagnitude(lookAhead, maxLookAhead);
            }

            Vector3 desired = _target.position + lookAhead + _offset;
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime);
        }
    }
}
