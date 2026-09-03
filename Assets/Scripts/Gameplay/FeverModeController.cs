using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SpinRush.Audio;
using SpinRush.Core;
using SpinRush.Effects;

namespace SpinRush.Gameplay
{
    /// <summary>
    /// Manages the Royal Fever Meter and Frenzy Free Spins Bonus Round.
    /// Every spin and near-miss charges the meter.
    /// At 100%, triggers 5 Free Spins with a 3x Global Payout Multiplier!
    /// </summary>
    public class FeverModeController : MonoBehaviour
    {
        [Header("Fever Settings")]
        [SerializeField] private int maxCharge = 100;
        [SerializeField] private int currentCharge = 0;
        [SerializeField] private int freeSpinsAwarded = 5;
        [SerializeField] private float feverMultiplier = 3.0f;

        [Header("Runtime State")]
        private bool _isFeverActive = false;
        private int _freeSpinsRemaining = 0;
        private int _totalFeverEarnings = 0;

        [Header("UI References")]
        [SerializeField] private RectTransform feverBarContainer;
        [SerializeField] private Image feverFillImage;
        [SerializeField] private Text feverStatusText;

        [Header("Controllers")]
        [SerializeField] private SlotMachineController slotController;
        [SerializeField] private AudioController audioController;
        [SerializeField] private WinEffectsPresenter effectsPresenter;

        // Events
        public event Action<int, int> OnFeverProgressChanged; // current, max
        public event Action<int> OnFeverModeStarted; // free spins count
        public event Action<int> OnFeverModeEnded; // total won

        public bool IsFeverActive => _isFeverActive;
        public int FreeSpinsRemaining => _freeSpinsRemaining;
        public float FeverMultiplier => feverMultiplier;
        public int CurrentCharge => currentCharge;

        private void Awake()
        {
            if (slotController == null) slotController = FindObjectOfType<SlotMachineController>();
            if (audioController == null) audioController = FindObjectOfType<AudioController>();
            if (effectsPresenter == null) effectsPresenter = FindObjectOfType<WinEffectsPresenter>();

            BuildFeverBarUI();
        }

        private void OnEnable()
        {
            if (slotController != null)
            {
                slotController.OnSpinEvaluated += HandleSpinEvaluated;
            }
        }

        private void OnDisable()
        {
            if (slotController != null)
            {
                slotController.OnSpinEvaluated -= HandleSpinEvaluated;
            }
        }

        /// <summary>
        /// Charges the fever meter based on spin outcome.
        /// Near-misses award a massive +25% rage charge!
        /// </summary>
        private void HandleSpinEvaluated(SpinResult result)
        {
            if (_isFeverActive)
            {
                _freeSpinsRemaining--;
                if (result.IsWin)
                {
                    _totalFeverEarnings += result.Payout;
                }

                UpdateUI();

                if (_freeSpinsRemaining <= 0)
                {
                    EndFeverMode();
                }
                return;
            }

            // Normal spin charge logic:
            int chargeToAdd = 10; // Base spin charge
            if (!result.IsWin)
            {
                // If it's a near-miss loss, grant bonus Rage Charge!
                chargeToAdd = 25;
            }
            else
            {
                chargeToAdd = 15;
            }

            AddCharge(chargeToAdd);
        }

        public void AddCharge(int amount)
        {
            if (_isFeverActive) return;

            currentCharge = Mathf.Clamp(currentCharge + amount, 0, maxCharge);
            OnFeverProgressChanged?.Invoke(currentCharge, maxCharge);
            UpdateUI();

            if (currentCharge >= maxCharge)
            {
                StartCoroutine(ActivateFeverRoutine());
            }
        }

        private IEnumerator ActivateFeverRoutine()
        {
            yield return new WaitForSeconds(0.4f);

            _isFeverActive = true;
            _freeSpinsRemaining = freeSpinsAwarded;
            _totalFeverEarnings = 0;

            if (audioController != null) audioController.PlayWinCelebration(50f, true);
            if (effectsPresenter != null)
            {
                effectsPresenter.Shake(1.0f, 12f);
                effectsPresenter.PlayParticleBurst(80);
            }

            OnFeverModeStarted?.Invoke(_freeSpinsRemaining);
            UpdateUI();
        }

        private void EndFeverMode()
        {
            _isFeverActive = false;
            currentCharge = 0;
            _freeSpinsRemaining = 0;

            OnFeverModeEnded?.Invoke(_totalFeverEarnings);
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (feverFillImage != null)
            {
                float targetFill = _isFeverActive ? ((float)_freeSpinsRemaining / freeSpinsAwarded) : ((float)currentCharge / maxCharge);
                feverFillImage.fillAmount = targetFill;
                feverFillImage.color = _isFeverActive ? new Color(1f, 0.2f, 0.6f) : new Color(1f, 0.85f, 0.2f);
            }

            if (feverStatusText != null)
            {
                if (_isFeverActive)
                {
                    feverStatusText.text = $"★ FEVER FRENZY! FREE SPINS: {_freeSpinsRemaining} (3X WINS!) ★";
                    feverStatusText.color = new Color(1f, 0.95f, 0.4f);
                }
                else if (currentCharge >= 75)
                {
                    feverStatusText.text = $"FEVER METER: {currentCharge}% (ALMOST FULL!)";
                    feverStatusText.color = new Color(1f, 0.85f, 0.2f);
                }
                else
                {
                    feverStatusText.text = $"FEVER METER: {currentCharge}%";
                    feverStatusText.color = new Color(0.85f, 0.80f, 1f);
                }
            }
        }

        /// <summary>
        /// Procedurally constructs the glowing Fever Meter bar across the cabinet marquee/reels.
        /// </summary>
        private void BuildFeverBarUI()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            Transform existing = canvas.transform.Find("FeverBarContainer");
            if (existing != null)
            {
                DestroyImmediate(existing.gameObject);
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // Container
            GameObject container = new GameObject("FeverBarContainer", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Shadow));
            container.transform.SetParent(canvas.transform, false);
            feverBarContainer = container.GetComponent<RectTransform>();
            feverBarContainer.anchoredPosition = new Vector2(0f, 142f);
            feverBarContainer.sizeDelta = new Vector2(370f, 22f);

            Image bgImg = container.GetComponent<Image>();
            bgImg.color = new Color(0.06f, 0.03f, 0.16f, 0.95f);

            Outline bgOutline = container.GetComponent<Outline>();
            bgOutline.effectColor = new Color(0.95f, 0.75f, 0.2f, 0.85f);
            bgOutline.effectDistance = new Vector2(1.5f, -1.5f);

            Shadow bgShadow = container.GetComponent<Shadow>();
            bgShadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
            bgShadow.effectDistance = new Vector2(2f, -2f);

            // Fill Bar
            GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillObj.transform.SetParent(container.transform, false);
            RectTransform fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = new Vector2(-4f, -4f);
            feverFillImage = fillObj.GetComponent<Image>();
            feverFillImage.type = Image.Type.Filled;
            feverFillImage.fillMethod = Image.FillMethod.Horizontal;
            feverFillImage.fillAmount = 0f;
            feverFillImage.color = new Color(1f, 0.85f, 0.2f);

            // Status Text
            GameObject textObj = new GameObject("StatusText", typeof(RectTransform), typeof(Text), typeof(Shadow));
            textObj.transform.SetParent(container.transform, false);
            RectTransform txtRect = textObj.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.sizeDelta = Vector2.zero;
            feverStatusText = textObj.GetComponent<Text>();
            feverStatusText.font = font;
            feverStatusText.text = "FEVER METER: 0%";
            feverStatusText.fontSize = 11;
            feverStatusText.fontStyle = FontStyle.Bold;
            feverStatusText.alignment = TextAnchor.MiddleCenter;
            feverStatusText.color = new Color(1f, 0.95f, 0.8f);

            Shadow tShadow = textObj.GetComponent<Shadow>();
            tShadow.effectColor = new Color(0f, 0f, 0f, 0.95f);
            tShadow.effectDistance = new Vector2(1f, -1f);

            UpdateUI();
        }
    }
}
