using MeteGame.Controls;
using MeteGame.Core;
using UnityEngine;

namespace MeteGame.Vehicle
{
    /// <summary>
    /// Çocuk dostu arcade sürüş. GAZ basılı = hızlan, bırak = frenle dur.
    /// GERİ basılı = geri git, bırak = çabuk dur. Joystick aracı ekran yönüne çevirir.
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
                targetSpeed = 0f;

            bool changingDirection = Mathf.Abs(CurrentSpeed) > 0.2f
                                     && targetSpeed != 0f
                                     && !Mathf.Approximately(Mathf.Sign(targetSpeed), Mathf.Sign(CurrentSpeed));
            // Gaza / geriye basılı değilken veya yön değişirken hızlıca yavaşla.
            bool shouldBrake = DriveInput.Locked || changingDirection
                               || (!DriveInput.Throttle && !DriveInput.Reverse);
            float rate = shouldBrake ? brakeDeceleration : acceleration;
            if (DriveInput.Reverse && !shouldBrake)
                rate = acceleration * 1.45f; // geri vites çabuk tutsun
            CurrentSpeed = Mathf.MoveTowards(CurrentSpeed, targetSpeed, rate * dt);

            ApplySteering(dt);

            Vector3 velocity = transform.forward * CurrentSpeed;
            velocity.y = Body.linearVelocity.y;
            Body.linearVelocity = velocity;
        }

        void ApplySteering(float dt)
        {
            float maxStep = maxSteerDegPerSec * dt;

            if (DriveInput.TryGetAimYaw(out float desiredYaw))
            {
                // Joystick: dururken de dönebilir — önce yön, sonra gaz.
                float currentYaw = Body.rotation.eulerAngles.y;
                float delta = Mathf.DeltaAngle(currentYaw, desiredYaw);
                float step = Mathf.Clamp(delta, -maxStep, maxStep);
                Body.MoveRotation(Body.rotation * Quaternion.Euler(0f, step, 0f));
                return;
            }

            float steerScale = Mathf.Clamp01(Mathf.Abs(CurrentSpeed) / 6f + 0.15f);
            float direction = CurrentSpeed >= 0f ? 1f : -1f;
            float yawDelta = DriveInput.Steer * maxSteerDegPerSec * steerScale * direction * dt;
            Body.MoveRotation(Body.rotation * Quaternion.Euler(0f, yawDelta, 0f));
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
