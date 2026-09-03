using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SpinRush.Gameplay;

namespace SpinRush.Effects
{
    /// <summary>
    /// Manages visual celebration effects: screen/cabinet micro-shake,
    /// sparkling particle bursts, and jackpot fanfare presentation.
    /// </summary>
    public class WinEffectsPresenter : MonoBehaviour
    {
        [Header("Target Transforms for Shake")]
        [Tooltip("Target rect transform to apply tactile physical shake to (SlotMachineRoot or Camera).")]
        [SerializeField] private RectTransform targetToShake;

        [Header("Particle System")]
        [SerializeField] private ParticleSystem goldParticleSystem;

        private Vector2 _originalPosition;
        private Coroutine _shakeCoroutine;

        private void Awake()
        {
            if (targetToShake != null)
            {
                _originalPosition = targetToShake.anchoredPosition;
            }
        }

        public void Initialize(RectTransform shakeTarget, ParticleSystem particles = null)
        {
            targetToShake = shakeTarget;
            goldParticleSystem = particles;

            if (targetToShake != null)
            {
                _originalPosition = targetToShake.anchoredPosition;
            }
        }

        /// <summary>
        /// Light mechanical thud when an individual reel locks into position.
        /// </summary>
        public void TriggerReelStopShake()
        {
            Shake(0.06f, 2.5f);
        }

        /// <summary>
        /// Tactile celebration rumble on win presentations.
        /// </summary>
        public void TriggerWinCelebration(SpinResult result)
        {
            if (!result.IsWin) return;

            if (result.IsRoyalJackpot || (result.IsJackpot && result.Multiplier >= 50f))
            {
                Shake(0.85f, 10f); // Massive royal jackpot rumble
                PlayParticleBurst(80); // Maximum gold explosion
            }
            else if (result.IsJackpot)
            {
                Shake(0.6f, 8f);
                PlayParticleBurst(60);
            }
            else if (result.Multiplier >= 25f)
            {
                Shake(0.35f, 5f);
                PlayParticleBurst(35);
            }
            else
            {
                Shake(0.18f, 3f);
                PlayParticleBurst(15);
            }
        }

        public void Shake(float duration, float magnitude)
        {
            if (targetToShake == null) return;

            if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
            _shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude));
        }

        private IEnumerator ShakeRoutine(float duration, float magnitude)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float damper = 1f - Mathf.Clamp01(elapsed / duration);
                float x = (Random.value * 2f - 1f) * magnitude * damper;
                float y = (Random.value * 2f - 1f) * magnitude * damper;

                targetToShake.anchoredPosition = _originalPosition + new Vector2(x, y);
                yield return null;
            }

            targetToShake.anchoredPosition = _originalPosition;
            _shakeCoroutine = null;
        }

        public void PlayParticleBurst(int count = 25)
        {
            if (goldParticleSystem != null)
            {
                goldParticleSystem.Emit(count);
            }
        }
    }
}
