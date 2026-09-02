using MeteGame.Core;
using UnityEngine;

namespace MeteGame.City
{
    /// <summary>
    /// Üretilen şehrin yol ağı verisi. Görev üreticisi alış/bırakış noktalarını
    /// bu sınıftan ister — noktalar her zaman yol üzerindedir.
    /// Trafik AI'si de aynı grid'i kavşak/şerit hesabı için kullanır.
    /// </summary>
    public class CityLayout
    {
        /// <summary>Dikey (kuzey-güney) yolların merkez x koordinatları.</summary>
        public float[] RoadXs;

        /// <summary>Yatay (doğu-batı) yolların merkez z koordinatları.</summary>
        public float[] RoadZs;

        /// <summary>Şehir merkezinden kenara olan mesafe.</summary>
        public float HalfExtent;

        public int RoadCount => RoadXs.Length;

        /// <summary>Oyuncunun başlangıç noktası: ortadaki dikey yolun merkezi.</summary>
        public Vector3 PlayerSpawnPosition
        {
            get
            {
                float x = RoadXs[RoadXs.Length / 2];
                return new Vector3(x, 0.5f, 0f);
            }
        }

        public Vector3 IntersectionCenter(int ix, int iz)
        {
            return new Vector3(RoadXs[ix], 0f, RoadZs[iz]);
        }

        public bool HasRoad(int ix, int iz, CardinalDir dir)
        {
            switch (dir)
            {
                case CardinalDir.North: return iz + 1 < RoadZs.Length;
                case CardinalDir.East: return ix + 1 < RoadXs.Length;
                case CardinalDir.South: return iz - 1 >= 0;
                default: return ix - 1 >= 0;
            }
        }

        public bool TryAdvance(int ix, int iz, CardinalDir dir, out int nx, out int nz)
        {
            nx = ix;
            nz = iz;
            switch (dir)
            {
                case CardinalDir.North: nz = iz + 1; break;
                case CardinalDir.East: nx = ix + 1; break;
                case CardinalDir.South: nz = iz - 1; break;
                default: nx = ix - 1; break;
            }
            return nx >= 0 && nz >= 0 && nx < RoadXs.Length && nz < RoadZs.Length;
        }

        /// <summary>
        /// Sağ şeritte, kavşağa yaklaşırken dur çizgisi (ışık arkası).
        /// </summary>
        public Vector3 StopLine(int ix, int iz, CardinalDir heading)
        {
            float dist = GameConfig.RoadWidth / 2f + GameConfig.StopLinePadding;
            Vector3 center = IntersectionCenter(ix, iz);
            return center
                   - CardinalUtil.Forward(heading) * dist
                   + CardinalUtil.Right(heading) * GameConfig.LaneOffset
                   + Vector3.up * 0.5f;
        }

        /// <summary>Kavşağı terk ederken sağ şeridin çıkış noktası.</summary>
        public Vector3 ExitPoint(int ix, int iz, CardinalDir heading)
        {
            float dist = GameConfig.RoadWidth / 2f + GameConfig.StopLinePadding;
            Vector3 center = IntersectionCenter(ix, iz);
            return center
                   + CardinalUtil.Forward(heading) * dist
                   + CardinalUtil.Right(heading) * GameConfig.LaneOffset
                   + Vector3.up * 0.5f;
        }

        /// <summary>
        /// Şerit üzerinde iki kavşak arasındaki bir nokta (t = 0 çıkış, t = 1 dur çizgisi).
        /// </summary>
        public Vector3 LanePoint(int fromIx, int fromIz, CardinalDir heading, float t)
        {
            if (!TryAdvance(fromIx, fromIz, heading, out int toIx, out int toIz))
                return ExitPoint(fromIx, fromIz, heading);

            Vector3 a = ExitPoint(fromIx, fromIz, heading);
            Vector3 b = StopLine(toIx, toIz, heading);
            return Vector3.Lerp(a, b, Mathf.Clamp01(t));
        }

        public Vector3 RandomRoadPoint(System.Random rng)
        {
            const float margin = 12f;
            bool vertical = rng.NextDouble() < 0.5;
            float along = Mathf.Lerp(-HalfExtent + margin, HalfExtent - margin, (float)rng.NextDouble());
            if (vertical)
            {
                float x = RoadXs[rng.Next(RoadXs.Length)];
                return new Vector3(x, 0f, along);
            }
            float z = RoadZs[rng.Next(RoadZs.Length)];
            return new Vector3(along, 0f, z);
        }

        /// <summary>
        /// Verilen noktadan istenen mesafe aralığında bir yol noktası arar;
        /// bulamazsa aralığa en yakın adayı döndürür.
        /// </summary>
        public Vector3 RandomRoadPointAwayFrom(System.Random rng, Vector3 from, float minDistance, float maxDistance)
        {
            float idealDistance = (minDistance + maxDistance) * 0.5f;
            Vector3 best = RandomRoadPoint(rng);
            float bestError = Mathf.Abs(Vector3.Distance(best, from) - idealDistance);

            for (int i = 0; i < 40; i++)
            {
                Vector3 candidate = RandomRoadPoint(rng);
                float distance = Vector3.Distance(candidate, from);
                if (distance >= minDistance && distance <= maxDistance)
                    return candidate;

                float error = Mathf.Abs(distance - idealDistance);
                if (error < bestError)
                {
                    best = candidate;
                    bestError = error;
                }
            }
            return best;
        }

        /// <summary>
        /// Dünya noktasını en yakın yola yapıştırır ve gidiş kavşağını hesaplar
        /// (hırsız / şerit spawn için).
        /// </summary>
        public void SnapToLane(Vector3 world, out Vector3 pos, out int destIx, out int destIz, out CardinalDir heading)
        {
            int ix = ClosestIndex(RoadXs, world.x);
            int iz = ClosestIndex(RoadZs, world.z);
            bool vertical = Mathf.Abs(world.x - RoadXs[ix]) <= Mathf.Abs(world.z - RoadZs[iz]);

            if (vertical)
            {
                destIx = ix;
                int north = NextIndexAlong(RoadZs, world.z, +1);
                if (north >= 0)
                {
                    heading = CardinalDir.North;
                    destIz = north;
                }
                else
                {
                    heading = CardinalDir.South;
                    destIz = NextIndexAlong(RoadZs, world.z, -1);
                    if (destIz < 0)
                        destIz = iz;
                }

                pos = new Vector3(RoadXs[ix], 0.5f, world.z)
                      + CardinalUtil.Right(heading) * GameConfig.LaneOffset;
            }
            else
            {
                destIz = iz;
                int east = NextIndexAlong(RoadXs, world.x, +1);
                if (east >= 0)
                {
                    heading = CardinalDir.East;
                    destIx = east;
                }
                else
                {
                    heading = CardinalDir.West;
                    destIx = NextIndexAlong(RoadXs, world.x, -1);
                    if (destIx < 0)
                        destIx = ix;
                }

                pos = new Vector3(world.x, 0.5f, RoadZs[iz])
                      + CardinalUtil.Right(heading) * GameConfig.LaneOffset;
            }
        }

        static int ClosestIndex(float[] values, float sample)
        {
            int best = 0;
            float bestErr = Mathf.Abs(values[0] - sample);
            for (int i = 1; i < values.Length; i++)
            {
                float err = Mathf.Abs(values[i] - sample);
                if (err < bestErr)
                {
                    bestErr = err;
                    best = i;
                }
            }
            return best;
        }

        static int NextIndexAlong(float[] values, float sample, int sign)
        {
            int best = -1;
            float bestDist = float.MaxValue;
            for (int i = 0; i < values.Length; i++)
            {
                float d = (values[i] - sample) * sign;
                if (d > 0.25f && d < bestDist)
                {
                    bestDist = d;
                    best = i;
                }
            }
            return best;
        }

        /// <summary>Kaldırım üzerindeki yürüme döngüsünün dört köşesi (saat yönü, y = 0).</summary>
        public void BlockWalkCorners(int blockI, int blockJ, Vector3[] corners)
        {
            float road = GameConfig.RoadWidth;
            float block = GameConfig.BlockSize;
            float cx = RoadXs[blockI] + road / 2f + block / 2f;
            float cz = RoadZs[blockJ] + road / 2f + block / 2f;
            float h = block / 2f - 1.7f;
            corners[0] = new Vector3(cx - h, 0f, cz - h); // GB
            corners[1] = new Vector3(cx + h, 0f, cz - h); // GD
            corners[2] = new Vector3(cx + h, 0f, cz + h); // KD
            corners[3] = new Vector3(cx - h, 0f, cz + h); // KB
        }
    }
}
