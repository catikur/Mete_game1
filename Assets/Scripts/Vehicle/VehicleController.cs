using MeteGame.Controls;
using MeteGame.Core;
using UnityEngine;

namespace MeteGame.Vehicle
{
    /// <summary>
    /// Çocuk dostu arcade sürüş. Basılı tut = gaz, bırak = fren, kaydır = dön.
    /// Çarpışmada ceza yok — araç yavaşlar ve devam eder.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class VehicleController : MonoBehaviour
    {
        public float maxForwardSpeed = GameConfig.MaxForwardSpeed;
        public float maxReverseSpeed = GameConfig.MaxReverseSpeed;
        public float acceleration = GameConfig.Acceleration;
        public float brakeDeceleration = GameConfig.BrakeDeceleration;
        public float maxSteerDegPerSec = GameConfig.MaxSteerDegPerSec;

        public Rigidbody Body { get; private set; }
        public float CurrentSpeed { get; private set; }

        void Awake()
        {
            Body = GetComponent<Rigidbody>();
            Body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            Body.interpolation = RigidbodyInterpolation.Interpolate;
            Body.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        float _hitCooldown;

        void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;

            float targetSpeed;
            if (DriveInput.Locked)
                targetSpeed = 0f;
            else if (DriveInput.Reverse)
                targetSpeed = -maxReverseSpeed;
            else if (DriveInput.Throttle)
                targetSpeed = maxForwardSpeed;
            else
                targetSpeed = 0f; // parmak kalkınca fren

            bool changingDirection = Mathf.Abs(CurrentSpeed) > 0.2f
                                     && targetSpeed != 0f
                                     && !Mathf.Approximately(Mathf.Sign(targetSpeed), Mathf.Sign(CurrentSpeed));
            bool shouldBrake = DriveInput.Locked || changingDirection
                               || (!DriveInput.Throttle && !DriveInput.Reverse);
            float rate = shouldBrake ? brakeDeceleration : acceleration;
            CurrentSpeed = Mathf.MoveTowards(CurrentSpeed, targetSpeed, rate * dt);

            // Düşük hızda az döner; geri giderken direksiyon doğal olarak ters çalışır.
            float steerScale = Mathf.Clamp01(Mathf.Abs(CurrentSpeed) / 6f + 0.15f);
            float direction = CurrentSpeed >= 0f ? 1f : -1f;
            float yawDelta = DriveInput.Steer * maxSteerDegPerSec * steerScale * direction * dt;
            Body.MoveRotation(Body.rotation * Quaternion.Euler(0f, yawDelta, 0f));

            Vector3 velocity = transform.forward * CurrentSpeed;
            velocity.y = Body.linearVelocity.y;
            Body.linearVelocity = velocity;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (Time.time < _hitCooldown)
                return;
            _hitCooldown = Time.time + 0.28f;
            CurrentSpeed *= 0.45f;
        }
    }
}
