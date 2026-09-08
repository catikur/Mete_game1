using MeteGame.Controls;
using MeteGame.Core;
using UnityEngine;

namespace MeteGame.Vehicle
{
    /// <summary>
    /// Çocuk dostu arcade sürüş. Joystick / WASD: ittiğin dünya yönüne yürü,
    /// bırakınca frenle dur. Geri vites yok — aşağı itmek güneye gitmektir.
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
        public Transform CargoAnchor { get; private set; }

        public void Warp(Vector3 position, Quaternion rotation)
        {
            CurrentSpeed = 0f;
            transform.SetPositionAndRotation(position, rotation);
            if (Body == null)
                return;
            Body.position = position;
            Body.rotation = rotation;
            Body.linearVelocity = Vector3.zero;
            Body.angularVelocity = Vector3.zero;
        }

        public void ApplySpec(VehicleDef def, Transform cargoAnchor)
        {
            CargoAnchor = cargoAnchor;
            maxForwardSpeed = def.MaxForwardSpeed;
            maxReverseSpeed = def.MaxReverseSpeed;
            acceleration = def.Acceleration;
            maxSteerDegPerSec = def.MaxSteerDegPerSec;
            if (Body != null)
                Body.mass = def.Mass;
        }

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
            Vector3 vel = Body.linearVelocity;
            Vector3 horizontal = new Vector3(vel.x, 0f, vel.z);

            if (DriveInput.Locked || !DriveInput.TryGetMove(out Vector2 dir, out float mag))
            {
                horizontal = Vector3.MoveTowards(horizontal, Vector3.zero, brakeDeceleration * dt);
                CurrentSpeed = horizontal.magnitude;
                Body.linearVelocity = new Vector3(horizontal.x, vel.y, horizontal.z);
                return;
            }

            float desiredYaw = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
            float currentYaw = Body.rotation.eulerAngles.y;
            float delta = Mathf.DeltaAngle(currentYaw, desiredYaw);
            float maxStep = maxSteerDegPerSec * 1.45f * dt;
            float step = Mathf.Clamp(delta, -maxStep, maxStep);
            Body.MoveRotation(Body.rotation * Quaternion.Euler(0f, step, 0f));

            Vector3 want = new Vector3(dir.x, 0f, dir.y) * (maxForwardSpeed * mag);
            horizontal = Vector3.MoveTowards(horizontal, want, acceleration * 1.35f * dt);
            CurrentSpeed = horizontal.magnitude;
            Body.linearVelocity = new Vector3(horizontal.x, vel.y, horizontal.z);
        }

        void OnCollisionEnter(Collision collision)
        {
            if (Time.time < _hitCooldown)
                return;
            _hitCooldown = Time.time + 0.28f;

            Vector3 vel = Body.linearVelocity;
            Vector3 horizontal = new Vector3(vel.x, 0f, vel.z) * 0.45f;
            CurrentSpeed = horizontal.magnitude;
            Body.linearVelocity = new Vector3(horizontal.x, vel.y, horizontal.z);
        }
    }
}
