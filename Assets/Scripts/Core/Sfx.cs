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
            Play2D(DingClip(), 0.85f);

        public static void Success(Vector3 position) =>
            Play2D(SuccessClip(), 0.9f);

        public static void Go(Vector3 position) =>
            Play2D(GoClip(), 0.8f);

        public static void Honk() =>
            Play2D(HonkClip(), 0.92f);

        static AudioSource _bus;

        static AudioSource Bus()
        {
            if (_bus != null)
                return _bus;

            var go = new GameObject("SfxBus");
            Object.DontDestroyOnLoad(go);
            _bus = go.AddComponent<AudioSource>();
            _bus.playOnAwake = false;
            _bus.spatialBlend = 0f;
            _bus.priority = 40;
            _bus.bypassListenerEffects = true;
            _bus.bypassReverbZones = true;
            return _bus;
        }

        public static void Play2D(AudioClip clip, float volume)
        {
            if (clip == null)
                return;
            GameAudio.PrepareListener();
            Bus().PlayOneShot(clip, volume);
        }

        static AudioClip _honk;

        static AudioClip HonkClip()
        {
            if (_honk != null)
                return _honk;
            const int hz = 22050;
            int n = (int)(hz * 0.22f);
            var clip = AudioClip.Create("honk", n, 1, hz, false);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)hz;
                float freq = t < 0.11f ? 420f : 330f;
                float env = 1f - i / (float)n;
                data[i] = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * t)) * 0.42f * env;
            }
            clip.SetData(data, 0);
            _honk = clip;
            return _honk;
        }

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
