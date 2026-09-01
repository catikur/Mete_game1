using UnityEngine;

namespace MeteGame.Controls
{
    /// <summary>
    /// Sürüş girdisi: sol başparmak gaz/geri/bip, sağ başparmak yön joystick'i.
    /// Editörde klavye yedek (W gaz, A/D dönüş, S geri, H korna).
    /// </summary>
    public static class DriveInput
    {
        /// <summary>Joystick: x sağ, y yukarı (ekran / dünya kuzeyi). 0 = ortada.</summary>
        public static Vector2 TouchStick;

        /// <summary>GAZ butonuna basılı mı?</summary>
        public static bool TouchThrottle;

        /// <summary>GERİ butonuna basılı mı?</summary>
        public static bool TouchReverse;

        /// <summary>Korna butonuna basılı mı?</summary>
        public static bool HonkHeld;

        /// <summary>Görev teklifi açıkken oyuncu aracı durur; şehir yaşamaya devam eder.</summary>
        public static bool Locked;

        public const float StickDeadzone = 0.22f;

        public static void ResetTouch()
        {
            TouchStick = Vector2.zero;
            TouchThrottle = false;
            TouchReverse = false;
            HonkHeld = false;
        }

        public static bool Throttle
        {
            get
            {
                if (Locked)
                    return false;
                return TouchThrottle
                       || Input.GetKey(KeyCode.UpArrow)
                       || Input.GetKey(KeyCode.W);
            }
        }

        /// <summary>
        /// Joystick yönü varsa aracı o dünyaya (ekran yukarı = kuzey) çevir.
        /// Yoksa false — klavye A/D ile göreli dönüş kullanılır.
        /// </summary>
        public static bool TryGetAimYaw(out float yawDegrees)
        {
            yawDegrees = 0f;
            if (Locked)
                return false;
            if (TouchStick.sqrMagnitude < StickDeadzone * StickDeadzone)
                return false;
            yawDegrees = Mathf.Atan2(TouchStick.x, TouchStick.y) * Mathf.Rad2Deg;
            return true;
        }

        public static float Steer
        {
            get
            {
                if (Locked)
                    return 0f;
                float keyboard = 0f;
                if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                    keyboard -= 1f;
                if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                    keyboard += 1f;
                return Mathf.Clamp(keyboard, -1f, 1f);
            }
        }

        public static bool Reverse =>
            !Locked && (
                TouchReverse
                || Input.GetKey(KeyCode.DownArrow)
                || Input.GetKey(KeyCode.S));
    }
}
