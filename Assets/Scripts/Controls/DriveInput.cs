using UnityEngine;

namespace MeteGame.Controls
{
    /// <summary>
    /// Tek el sürüş: sağ joystick (veya WASD) hem yön hem gaz.
    /// Çubuğu ittiğin dünya yönüne araç yürür; bırakınca durur.
    /// Sol BİP korna. Editörde H korna, Esc duraklatma.
    /// </summary>
    public static class DriveInput
    {
        /// <summary>Joystick: x sağ (doğu), y yukarı (kuzey). 0 = ortada.</summary>
        public static Vector2 TouchStick;

        /// <summary>Korna butonuna basılı mı?</summary>
        public static bool HonkHeld;

        /// <summary>Görev teklifi açıkken oyuncu aracı durur; şehir yaşamaya devam eder.</summary>
        public static bool Locked;

        public const float StickDeadzone = 0.18f;

        public static void ResetTouch()
        {
            TouchStick = Vector2.zero;
            HonkHeld = false;
        }

        /// <summary>
        /// Dünya düzleminde gidiş. x = doğu, y = kuzey. magnitude 0–1 (itme miktarı).
        /// Yoksa false — araç frenler.
        /// </summary>
        public static bool TryGetMove(out Vector2 direction, out float magnitude)
        {
            direction = Vector2.zero;
            magnitude = 0f;
            if (Locked)
                return false;

            Vector2 stick = TouchStick;
            if (stick.sqrMagnitude < StickDeadzone * StickDeadzone)
                stick = KeyboardStick();

            float mag = stick.magnitude;
            if (mag < StickDeadzone)
                return false;

            magnitude = Mathf.Clamp01(mag);
            direction = stick / mag;
            return true;
        }

        static Vector2 KeyboardStick()
        {
            Vector2 key = Vector2.zero;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                key.y += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                key.y -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                key.x += 1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                key.x -= 1f;
            if (key.sqrMagnitude > 1f)
                key.Normalize();
            return key;
        }
    }
}
