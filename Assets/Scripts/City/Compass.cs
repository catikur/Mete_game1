using UnityEngine;

namespace MeteGame.City
{
    public enum Compass
    {
        North = 0, // +Z
        East = 1,  // +X
        South = 2, // -Z
        West = 3   // -X
    }

    public static class CompassUtil
    {
        public static readonly Vector3[] Vectors =
        {
            new Vector3(0f, 0f, 1f),
            new Vector3(1f, 0f, 0f),
            new Vector3(0f, 0f, -1f),
            new Vector3(-1f, 0f, 0f)
        };

        public static Vector3 Forward(Compass dir) => Vectors[(int)dir];

        public static Vector3 Right(Compass dir) => Vector3.Cross(Vector3.up, Forward(dir));

        public static Compass Opposite(Compass dir) => (Compass)(((int)dir + 2) % 4);

        public static Compass LeftOf(Compass dir) => (Compass)(((int)dir + 3) % 4);

        public static Compass RightOf(Compass dir) => (Compass)(((int)dir + 1) % 4);

        public static bool IsNorthSouth(Compass dir) => dir == Compass.North || dir == Compass.South;

        public static Quaternion Rotation(Compass dir) => Quaternion.LookRotation(Forward(dir), Vector3.up);
    }
}
