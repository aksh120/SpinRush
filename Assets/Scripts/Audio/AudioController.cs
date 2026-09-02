using System.Collections.Generic;
using UnityEngine;
using SpinRush.Core;

namespace SpinRush.Audio
{
    /// <summary>
    /// Procedural audio engine generating real-time PCM waveforms for all slot machine SFX.
    /// Provides 100% self-contained, high-performance, WebGL-compatible audio feedback.
    /// </summary>
    public class AudioController : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource loopSource;
        [SerializeField] private AudioSource musicSource;

        [Header("Volume Controls")]
        [Range(0f, 1f)] [SerializeField] private float sfxVolume = 0.85f;
        [Range(0f, 1f)] [SerializeField] private float loopVolume = 0.45f;

        // Cached procedural clips
        private AudioClip _clickClip;
        private AudioClip _leverClip;
        private AudioClip _spinLoopClip;
        private AudioClip[] _reelStopClips = new AudioClip[3];
        private AudioClip _winChimeClip;
        private AudioClip _bigWinFanfareClip;
        private AudioClip _jackpotFanfareClip;
        private AudioClip _warningClip;

        private const int SampleRate = 44100;

        private void Awake()
        {
            SetupAudioSources();
            GenerateAllProceduralClips();
        }

        private void SetupAudioSources()
        {
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }

            if (loopSource == null)
            {
                loopSource = gameObject.AddComponent<AudioSource>();
                loopSource.loop = true;
                loopSource.playOnAwake = false;
            }
        }

        private void GenerateAllProceduralClips()
        {
            _clickClip = GenerateButtonClickClip();
            _leverClip = GenerateLeverPullClip();
            _spinLoopClip = GenerateSpinLoopClip();

            _reelStopClips[0] = GenerateReelStopClip(220f); // A3 (220 Hz)
            _reelStopClips[1] = GenerateReelStopClip(277f); // C#4 (277 Hz) - builds anticipation
            _reelStopClips[2] = GenerateReelStopClip(330f); // E4 (330 Hz) - resolves chord!

            _winChimeClip = GenerateWinChimeClip();
            _bigWinFanfareClip = GenerateBigWinFanfareClip();
            _jackpotFanfareClip = GenerateJackpotFanfareClip();
            _warningClip = GenerateWarningToneClip();
        }

        // ==========================================
        //  PUBLIC PLAY METHODS
        // ==========================================

        public void PlayButtonClick()
        {
            if (sfxSource != null && _clickClip != null)
                sfxSource.PlayOneShot(_clickClip, sfxVolume * 0.7f);
        }

        public void PlayLeverPull()
        {
            if (sfxSource != null && _leverClip != null)
                sfxSource.PlayOneShot(_leverClip, sfxVolume * 0.9f);
        }

        public void StartReelSpinLoop()
        {
            if (loopSource != null && _spinLoopClip != null)
            {
                loopSource.clip = _spinLoopClip;
                loopSource.volume = loopVolume;
                loopSource.Play();
            }
        }

        public void StopReelSpinLoop()
        {
            if (loopSource != null && loopSource.isPlaying)
            {
                loopSource.Stop();
            }
        }

        public void PlayReelStop(int reelIndex)
        {
            int idx = Mathf.Clamp(reelIndex, 0, _reelStopClips.Length - 1);
            if (sfxSource != null && _reelStopClips[idx] != null)
            {
                sfxSource.PlayOneShot(_reelStopClips[idx], sfxVolume);
            }
        }

        public void PlayWinCelebration(float multiplier, bool isJackpot)
        {
            StopReelSpinLoop();

            if (isJackpot)
            {
                if (sfxSource != null && _jackpotFanfareClip != null)
                    sfxSource.PlayOneShot(_jackpotFanfareClip, sfxVolume * 1.0f);
            }
            else if (multiplier >= 25f)
            {
                if (sfxSource != null && _bigWinFanfareClip != null)
                    sfxSource.PlayOneShot(_bigWinFanfareClip, sfxVolume * 0.95f);
            }
            else
            {
                if (sfxSource != null && _winChimeClip != null)
                    sfxSource.PlayOneShot(_winChimeClip, sfxVolume * 0.85f);
            }
        }

        public void PlayLowBalanceAlert()
        {
            if (sfxSource != null && _warningClip != null)
                sfxSource.PlayOneShot(_warningClip, sfxVolume * 0.8f);
        }

        // ==========================================
        //  PROCEDURAL SYNTHESIS ENGINES
        // ==========================================

        private AudioClip GenerateButtonClickClip()
        {
            int length = (int)(SampleRate * 0.05f); // 50ms
            float[] samples = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = (float)i / SampleRate;
                float decay = Mathf.Exp(-t * 80f);
                samples[i] = Mathf.Sin(2f * Mathf.PI * 1400f * t) * decay;
            }
            return CreateClip("SFX_ButtonClick", samples);
        }

        private AudioClip GenerateLeverPullClip()
        {
            int length = (int)(SampleRate * 0.22f); // 220ms
            float[] samples = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = (float)i / SampleRate;
                float freq = Mathf.Lerp(450f, 150f, t / 0.22f);
                float ratchet = Mathf.Sin(2f * Mathf.PI * 45f * t) > 0 ? 1f : -0.5f;
                float decay = 1f - (t / 0.22f);
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * ratchet * decay * 0.8f;
            }
            return CreateClip("SFX_LeverPull", samples);
        }

        private AudioClip GenerateSpinLoopClip()
        {
            int length = (int)(SampleRate * 0.4f); // 400ms looping cycle
            float[] samples = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = (float)i / SampleRate;
                float rhythmicPulse = Mathf.Sin(2f * Mathf.PI * 12f * t); // 12 clicks per sec
                float noise = (Random.value * 2f - 1f) * 0.25f;
                float sub = Mathf.Sin(2f * Mathf.PI * 90f * t) * 0.4f;
                samples[i] = (sub + noise) * (0.6f + 0.4f * rhythmicPulse);
            }
            return CreateClip("SFX_SpinLoop", samples);
        }

        private AudioClip GenerateReelStopClip(float baseFreq)
        {
            int length = (int)(SampleRate * 0.12f); // 120ms
            float[] samples = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = (float)i / SampleRate;
                float decay = Mathf.Exp(-t * 35f);
                float fundamental = Mathf.Sin(2f * Mathf.PI * baseFreq * t);
                float metallicHarmonic = Mathf.Sin(2f * Mathf.PI * (baseFreq * 2.8f) * t) * 0.35f;
                float clickTransient = (i < 80) ? (Random.value * 2f - 1f) * 0.5f : 0f;
                samples[i] = (fundamental + metallicHarmonic + clickTransient) * decay;
            }
            return CreateClip($"SFX_ReelStop_{baseFreq}", samples);
        }

        private AudioClip GenerateWinChimeClip()
        {
            // Ascending major arpeggio: C5 (523Hz), E5 (659Hz), G5 (784Hz)
            float[] freqs = new float[] { 523.25f, 659.25f, 783.99f };
            float noteDuration = 0.14f;
            int totalLength = (int)(SampleRate * (noteDuration * freqs.Length + 0.3f));
            float[] samples = new float[totalLength];

            for (int note = 0; note < freqs.Length; note++)
            {
                int startSample = (int)(note * noteDuration * SampleRate);
                float f = freqs[note];

                for (int i = 0; i < (int)(SampleRate * 0.45f); i++)
                {
                    int sampleIdx = startSample + i;
                    if (sampleIdx >= totalLength) break;

                    float t = (float)i / SampleRate;
                    float decay = Mathf.Exp(-t * 9f);
                    float tone = Mathf.Sin(2f * Mathf.PI * f * t) + 0.3f * Mathf.Sin(2f * Mathf.PI * f * 2f * t);
                    samples[sampleIdx] += tone * decay * 0.35f;
                }
            }

            return CreateClip("SFX_WinChime", samples);
        }

        private AudioClip GenerateBigWinFanfareClip()
        {
            // Celebratory melody: C5, E5, G5, C6 (1046Hz), with warm bell decay
            float[] freqs = new float[] { 523.25f, 659.25f, 783.99f, 1046.50f, 1046.50f };
            float noteDuration = 0.12f;
            int totalLength = (int)(SampleRate * 1.2f);
            float[] samples = new float[totalLength];

            for (int note = 0; note < freqs.Length; note++)
            {
                int startSample = (int)(note * noteDuration * SampleRate);
                float f = freqs[note];
                float duration = (note == freqs.Length - 1) ? 0.7f : 0.25f;

                for (int i = 0; i < (int)(SampleRate * duration); i++)
                {
                    int sampleIdx = startSample + i;
                    if (sampleIdx >= totalLength) break;

                    float t = (float)i / SampleRate;
                    float decay = Mathf.Exp(-t * 6f);
                    float tone = Mathf.Sin(2f * Mathf.PI * f * t) + 0.4f * Mathf.Sin(2f * Mathf.PI * f * 2f * t) + 0.2f * Mathf.Sin(2f * Mathf.PI * f * 3f * t);
                    samples[sampleIdx] += tone * decay * 0.3f;
                }
            }

            return CreateClip("SFX_BigWinFanfare", samples);
        }

        private AudioClip GenerateJackpotFanfareClip()
        {
            // Grand Royal Victory: Rapid victory arpeggio + triumphant sustained chord
            float[] freqs = new float[] { 523.25f, 659.25f, 783.99f, 1046.50f, 1318.51f, 1567.98f };
            float noteDuration = 0.09f;
            int totalLength = (int)(SampleRate * 2.0f);
            float[] samples = new float[totalLength];

            for (int note = 0; note < freqs.Length; note++)
            {
                int startSample = (int)(note * noteDuration * SampleRate);
                float f = freqs[note];

                for (int i = 0; i < (int)(SampleRate * 1.4f); i++)
                {
                    int sampleIdx = startSample + i;
                    if (sampleIdx >= totalLength) break;

                    float t = (float)i / SampleRate;
                    float decay = Mathf.Exp(-t * 3.5f);
                    float shimmer = 1f + 0.08f * Mathf.Sin(2f * Mathf.PI * 6f * t);
                    float tone = (Mathf.Sin(2f * Mathf.PI * f * t) + 0.5f * Mathf.Sin(2f * Mathf.PI * f * 2f * t)) * shimmer;
                    samples[sampleIdx] += tone * decay * 0.25f;
                }
            }

            return CreateClip("SFX_KohinoorJackpot", samples);
        }

        private AudioClip GenerateWarningToneClip()
        {
            // Soft descending two-tone alert
            float[] freqs = new float[] { 440f, 330f };
            float noteDuration = 0.15f;
            int totalLength = (int)(SampleRate * 0.45f);
            float[] samples = new float[totalLength];

            for (int note = 0; note < freqs.Length; note++)
            {
                int startSample = (int)(note * noteDuration * SampleRate);
                float f = freqs[note];

                for (int i = 0; i < (int)(SampleRate * 0.22f); i++)
                {
                    int sampleIdx = startSample + i;
                    if (sampleIdx >= totalLength) break;

                    float t = (float)i / SampleRate;
                    float decay = Mathf.Exp(-t * 12f);
                    samples[sampleIdx] += Mathf.Sin(2f * Mathf.PI * f * t) * decay * 0.35f;
                }
            }

            return CreateClip("SFX_WarningTone", samples);
        }

        private AudioClip CreateClip(string name, float[] samples)
        {
            AudioClip clip = AudioClip.Create(name, samples.Length, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
