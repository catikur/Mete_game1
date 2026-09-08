using UnityEngine;

namespace MeteGame.Core
{
    /// <summary>Görev kutlamasında uçuşan renkli parçalar — dosya/partikül asset gerekmez.</summary>
    public class ConfettiBurst : MonoBehaviour
    {
        Vector3 _velocity;
        float _life;

        public static void Play(Vector3 origin)
        {
            var rng = new System.Random();
            int count = 28;
            for (int i = 0; i < count; i++)
            {
                Color color = GameConfig.CarPalette[rng.Next(GameConfig.CarPalette.Length)];
                var bit = PartFactory.Create(PrimitiveType.Cube, "Confetti", null,
                    origin + Vector3.up * 1.6f, Vector3.one * Random.Range(0.18f, 0.38f),
                    color, castShadows: false);
                var burst = bit.AddComponent<ConfettiBurst>();
                Vector3 dir = new Vector3(
                    (float)(rng.NextDouble() * 2.0 - 1.0),
                    (float)(0.6 + rng.NextDouble() * 1.2),
                    (float)(rng.NextDouble() * 2.0 - 1.0)).normalized;
                burst._velocity = dir * Random.Range(7f, 14f);
                burst._life = Random.Range(0.7f, 1.25f);
            }
        }

        void Update()
        {
            float dt = Time.deltaTime;
            _velocity += Vector3.down * 18f * dt;
            transform.position += _velocity * dt;
            transform.Rotate(_velocity.z * 40f * dt, _velocity.x * 40f * dt, 80f * dt, Space.World);
            _life -= dt;
            if (_life <= 0f)
                Destroy(gameObject);
        }
    }
}
