using UnityEngine;

namespace MeteGame.Controls
{
    /// <summary>
    /// Sürüş girdisi: dokunmatik (bas = gaz, kaydır = dön, bırak = fren)
    /// ve editörde klavye.
    /// </summary>
    public static class DriveInput
    {
        /// <summary>Kaydırma ile gelen direksiyon: -1 sol, +1 sağ.</summary>
        public static float TouchSteer;

        /// <summary>Ekrana basılı mı? (gaz)</summary>
        public static bool TouchThrottle;

        /// <summary>GERİ butonuna basılı mı?</summary>
        public static bool TouchReverse;

        /// <summary>Korna butonuna basılı mı?</summary>
        public static bool HonkHeld;

        /// <summary>Görev teklifi açıkken oyuncu aracı durur; şehir yaşamaya devam eder.</summary>
        public static bool Locked;

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
                return Mathf.Clamp(keyboard + TouchSteer, -1f, 1f);
            }
        }

        public static bool Reverse =>
            !Locked && (
                TouchReverse
                || Input.GetKey(KeyCode.DownArrow)
                || Input.GetKey(KeyCode.S));
    }
}
