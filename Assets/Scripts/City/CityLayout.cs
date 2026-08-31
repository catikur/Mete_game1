using UnityEngine;

namespace MeteGame.City
{
    /// <summary>
    /// Üretilen şehrin yol ağı verisi. Görev üreticisi alış/bırakış noktalarını
    /// bu sınıftan ister — noktalar her zaman yol üzerindedir.
    /// </summary>
    public class CityLayout
    {
        /// <summary>Dikey (kuzey-güney) yolların merkez x koordinatları.</summary>
        public float[] RoadXs;

        /// <summary>Yatay (doğu-batı) yolların merkez z koordinatları.</summary>
        public float[] RoadZs;

        /// <summary>Şehir merkezinden kenara olan mesafe.</summary>
        public float HalfExtent;

        /// <summary>Oyuncunun başlangıç noktası: ortadaki dikey yolun merkezi.</summary>
        public Vector3 PlayerSpawnPosition
        {
            get
            {
                float x = RoadXs[RoadXs.Length / 2];
                return new Vector3(x, 0.5f, 0f);
            }
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
    }
}
