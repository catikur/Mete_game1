using UnityEngine;

namespace MeteGame.Controls
{
    /// <summary>
    /// Sürüş girdisini tek noktada toplar: dokunmatik butonlar (UI'dan yazılır)
    /// ve editörde test için klavye. Gaz otomatiktir — girdi sadece yön ve geri vites.
    /// </summary>
    public static class DriveInput
    {
        /// <summary>UI butonlarının yazdığı direksiyon değeri: -1 sol, +1 sağ.</summary>
        public static float TouchSteer;

        /// <summary>GERİ butonuna basılı mı?</summary>
        public static bool TouchReverse;

        public static float Steer
        {
            get
            {
                float keyboard = 0f;
                if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                    keyboard -= 1f;
                if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                    keyboard += 1f;
                return Mathf.Clamp(keyboard + TouchSteer, -1f, 1f);
            }
        }

        public static bool Reverse =>
            TouchReverse
            || Input.GetKey(KeyCode.DownArrow)
            || Input.GetKey(KeyCode.S)
            || Input.GetKey(KeyCode.Space);
    }
}
