using MeteGame.Controls;
using MeteGame.Vehicle;
using UnityEngine;

namespace MeteGame.Core
{
    /// <summary>
    /// Dosyasız müzik + motor. Menü/garajda sadece müzik; şehirde hıza göre vınlama.
    /// Duraklatınca AudioListener.pause ile susar.
    /// </summary>
    public class GameAudio : MonoBehaviour
    {
        VehicleController _vehicle;
        AudioSource _music;
        AudioSource _engine;

        static AudioClip _musicClip;
        static AudioClip _engineClip;

        public static void PlayCity(Transform parent, VehicleController vehicle)
        {
            var audio = Create(parent);
            audio._vehicle = vehicle;
            audio._music.volume = 0.12f;
            audio.EnsureEngine();
            audio._music.Play();
            audio._engine.Play();
        }

        public static void PlayMenu(Transform parent)
        {
            var audio = Create(parent);
            audio._music.volume = 0.09f;
            audio._music.Play();
        }

        static GameAudio Create(Transform parent)
        {
            var go = new GameObject("GameAudio");
            go.transform.SetParent(parent, false);
            var audio = go.AddComponent<GameAudio>();
            audio._music = go.AddComponent<AudioSource>();
            audio._music.clip = MusicClip();
            audio._music.loop = true;
            audio._music.playOnAwake = false;
            audio._music.spatialBlend = 0f;
            audio._music.priority = 200;
            return audio;
        }

        void EnsureEngine()
        {
            _engine = gameObject.AddComponent<AudioSource>();
            _engine.clip = EngineClip();
            _engine.loop = true;
            _engine.playOnAwake = false;
            _engine.spatialBlend = 0f;
            _engine.priority = 180;
            _engine.volume = 0f;
        }

        void Update()
        {
            if (_engine == null || _vehicle == null)
                return;

            float max = Mathf.Max(4f, _vehicle.maxForwardSpeed);
            float t = Mathf.Clamp01(Mathf.Abs(_vehicle.CurrentSpeed) / max);
            float wantVol = DriveInput.Locked ? 0f : Mathf.Lerp(0.03f, 0.20f, t);
            float wantPitch = Mathf.Lerp(0.86f, 1.48f, t);
            _engine.volume = Mathf.MoveTowards(_engine.volume, wantVol, Time.unscaledDeltaTime * 2.2f);
            _engine.pitch = Mathf.MoveTowards(_engine.pitch, wantPitch, Time.unscaledDeltaTime * 1.6f);
        }

        static AudioClip MusicClip()
        {
            if (_musicClip != null)
                return _musicClip;

            const int hz = 22050;
            const float bpm = 120f;
            const int beats = 8;
            float seconds = beats * (60f / bpm); // 4.0 sn, tam döngü
            int n = (int)(hz * seconds);
            var data = new float[n];

            // Pentatonik: C D E G A — neşeli, gergin değil.
            float[] pent = { 261.63f, 293.66f, 329.63f, 392.00f, 440.00f };
            int[] melody = { 0, 2, 4, 2, 3, 4, 2, 0 };
            int[] bassDeg = { 0, 0, 3, 0, 0, 2, 3, 0 };
            float beat = 60f / bpm;

            for (int b = 0; b < beats; b++)
            {
                int start = (int)(b * beat * hz);
                int len = Mathf.Min((int)(beat * 0.82f * hz), n - start);
                StampNote(data, hz, start, len, pent[melody[b]] * 2f, 0.11f);
                StampNote(data, hz, start, len, pent[bassDeg[b]] * 0.5f, 0.09f);
                if (b % 2 == 0)
                    StampNote(data, hz, start, Mathf.Min(len / 2, n - start), pent[4] * 2f, 0.035f);
            }

            for (int i = 0; i < n; i++)
                data[i] = Mathf.Clamp(data[i], -0.9f, 0.9f);

            // Döngü tıklamasını kes.
            int fade = Mathf.Min(400, n / 8);
            for (int i = 0; i < fade; i++)
            {
                float t = i / (float)fade;
                data[i] *= t;
                data[n - 1 - i] *= t;
            }

            _musicClip = AudioClip.Create("mete-music", n, 1, hz, false);
            _musicClip.SetData(data, 0);
            return _musicClip;
        }

        static AudioClip EngineClip()
        {
            if (_engineClip != null)
                return _engineClip;

            const int hz = 22050;
            int n = hz / 4; // 0.25 sn
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)hz;
                float wave = Mathf.Sin(2f * Mathf.PI * 88f * t) * 0.55f
                            + Mathf.Sin(2f * Mathf.PI * 176f * t) * 0.22f
                            + Mathf.Sin(2f * Mathf.PI * 44f * t) * 0.18f;
                data[i] = wave * 0.22f;
            }

            _engineClip = AudioClip.Create("mete-engine", n, 1, hz, false);
            _engineClip.SetData(data, 0);
            return _engineClip;
        }

        static void StampNote(float[] data, int hz, int start, int len, float freq, float volume)
        {
            if (len <= 4 || start >= data.Length)
                return;
            len = Mathf.Min(len, data.Length - start);
            for (int i = 0; i < len; i++)
            {
                float t = i / (float)len;
                float env = t < 0.08f ? t / 0.08f : (t > 0.7f ? (1f - t) / 0.3f : 1f);
                env = Mathf.Clamp01(env);
                float sample = Mathf.Sin(2f * Mathf.PI * freq * ((start + i) / (float)hz));
                sample += 0.18f * Mathf.Sin(2f * Mathf.PI * freq * 2f * ((start + i) / (float)hz));
                data[start + i] += sample * volume * env;
            }
        }
    }
}
