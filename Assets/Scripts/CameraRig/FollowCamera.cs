using MeteGame.Core;
using UnityEngine;

namespace MeteGame.CameraRig
{
    /// <summary>
    /// GTA 2 tarzı kuş bakışı takip kamerası: kuzeyi sabit (dönmez), eğimli bakar,
    /// araç hızlandıkça hafifçe ileriyi gösterir ve FOV biraz açılır.
    /// </summary>
    public class FollowCamera : MonoBehaviour
    {
        public float distance = 32f;
        public float pitchDegrees = 62f;
        public float lookAheadTime = 0.7f;
        public float maxLookAhead = 12f;
        public float smoothTime = 0.22f;
        public float baseFov = 50f;
        public float speedFovBoost = 8f;

        Transform _target;
        Rigidbody _targetBody;
        Camera _camera;
        Vector3 _offset;
        Vector3 _velocity;
        float _fovVelocity;

        public void SetTarget(Transform target, Rigidbody targetBody)
        {
            _target = target;
            _targetBody = targetBody;
            _camera = GetComponent<Camera>();
            if (_camera != null)
                _camera.fieldOfView = baseFov;

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
            float speed = 0f;
            if (_targetBody != null)
            {
                Vector3 vel = _targetBody.linearVelocity;
                lookAhead = vel * lookAheadTime;
                lookAhead.y = 0f;
                lookAhead = Vector3.ClampMagnitude(lookAhead, maxLookAhead);
                vel.y = 0f;
                speed = vel.magnitude;
            }

            Vector3 desired = _target.position + lookAhead + _offset;
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime);

            if (_camera != null)
            {
                float t = Mathf.Clamp01(speed / GameConfig.MaxForwardSpeed);
                float fov = Mathf.Lerp(baseFov, baseFov + speedFovBoost, t);
                _camera.fieldOfView = Mathf.SmoothDamp(_camera.fieldOfView, fov, ref _fovVelocity, 0.35f);
            }
        }
    }
}
