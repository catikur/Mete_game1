using UnityEngine;

namespace MeteGame.City
{
    /// <summary>
    /// Kuzey/doğu/güney/batı. UnityEngine.Compass (cihaz pusulası) ile karışmasın diye
    /// CardinalDir adını kullanıyoruz.
    /// </summary>
    public enum CardinalDir
    {
        North = 0, // +Z
        East = 1,  // +X
        South = 2, // -Z
        West = 3   // -X
    }

    public static class CardinalUtil
    {
        public static readonly Vector3[] Vectors =
        {
            new Vector3(0f, 0f, 1f),
            new Vector3(1f, 0f, 0f),
            new Vector3(0f, 0f, -1f),
            new Vector3(-1f, 0f, 0f)
        };

        public static Vector3 Forward(CardinalDir dir) => Vectors[(int)dir];

        public static Vector3 Right(CardinalDir dir) => Vector3.Cross(Vector3.up, Forward(dir));

        public static CardinalDir Opposite(CardinalDir dir) => (CardinalDir)(((int)dir + 2) % 4);

        public static CardinalDir LeftOf(CardinalDir dir) => (CardinalDir)(((int)dir + 3) % 4);

        public static CardinalDir RightOf(CardinalDir dir) => (CardinalDir)(((int)dir + 1) % 4);

        public static bool IsNorthSouth(CardinalDir dir) => dir == CardinalDir.North || dir == CardinalDir.South;

        public static Quaternion Rotation(CardinalDir dir) => Quaternion.LookRotation(Forward(dir), Vector3.up);
    }
}
