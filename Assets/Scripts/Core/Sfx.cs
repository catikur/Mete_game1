using UnityEngine;

namespace MeteGame.Core
{
    /// <summary>Kısa prosedürel sesler — dosya gerekmez.</summary>
    public static class Sfx
    {
        static AudioClip _ding;
        static AudioClip _success;
        static AudioClip _go;

        public static void Ding(Vector3 position) =>
            AudioSource.PlayClipAtPoint(DingClip(), position, 0.7f);

        public static void Success(Vector3 position) =>
            AudioSource.PlayClipAtPoint(SuccessClip(), position, 0.78f);

        public static void Go(Vector3 position) =>
            AudioSource.PlayClipAtPoint(GoClip(), position, 0.62f);

        static AudioClip DingClip()
        {
            if (_ding == null)
                _ding = ToneSweep("ding", 0.16f, 880f, 1320f, 0.22f);
            return _ding;
        }

        static AudioClip SuccessClip()
        {
            if (_success == null)
                _success = Chord("success", 0.32f, new[] { 523f, 659f, 784f });
            return _success;
        }

        static AudioClip GoClip()
        {
            if (_go == null)
                _go = ToneSweep("go", 0.18f, 392f, 587f, 0.2f);
            return _go;
        }

        static AudioClip ToneSweep(string name, float seconds, float fromHz, float toHz, float volume)
        {
            const int hz = 22050;
            int n = Mathf.Max(1, (int)(hz * seconds));
            var clip = AudioClip.Create(name, n, 1, hz, false);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)n;
                float freq = Mathf.Lerp(fromHz, toHz, t);
                float env = Mathf.Sin(t * Mathf.PI);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)hz)) * volume * env;
            }
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip Chord(string name, float seconds, float[] freqs)
        {
            const int hz = 22050;
            int n = Mathf.Max(1, (int)(hz * seconds));
            var clip = AudioClip.Create(name, n, 1, hz, false);
            var data = new float[n];
            float amp = 0.16f / freqs.Length;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)hz;
                float env = 1f - i / (float)n;
                float sample = 0f;
                for (int f = 0; f < freqs.Length; f++)
                    sample += Mathf.Sin(2f * Mathf.PI * freqs[f] * t);
                data[i] = sample * amp * env;
            }
            clip.SetData(data, 0);
            return clip;
        }
    }
}
