using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SpinRush.Core;
using SpinRush.Gameplay;

namespace SpinRush.UI
{
    /// <summary>
    /// Controls the dashboard HUD inside the slot machine middle box.
    /// Updates formatted Indian Rupee (₹) credit balance, active VIP bet, and animated winning score counters.
    /// </summary>
    public class MiddleBoxHUD : MonoBehaviour
    {
        [Header("UI Text Displays")]
        [SerializeField] private Text balanceText;
        [SerializeField] private Text betText;
        [SerializeField] private Text winText;

        [Header("Controllers")]
        [SerializeField] private WalletManager walletManager;
        [SerializeField] private SlotMachineController slotMachineController;

        private Coroutine _countUpCoroutine;
        private Vector3 _originalWinScale = Vector3.one;

        private void Awake()
        {
            if (walletManager == null)
            {
                walletManager = FindObjectOfType<WalletManager>();
            }

            if (slotMachineController == null)
            {
                slotMachineController = FindObjectOfType<SlotMachineController>();
            }

            if (winText != null)
            {
                _originalWinScale = winText.rectTransform.localScale;
            }

            // Ensure HUD width and labels prevent border overlap at runtime
            RectTransform hudRect = GetComponent<RectTransform>();
            if (hudRect != null && hudRect.sizeDelta.x < 740f)
            {
                hudRect.sizeDelta = new Vector2(760f, hudRect.sizeDelta.y);
            }

            if (balanceText != null)
            {
                balanceText.resizeTextForBestFit = true;
                balanceText.resizeTextMinSize = 16;
                balanceText.resizeTextMaxSize = 26;
                if (balanceText.rectTransform != null)
                {
                    balanceText.rectTransform.sizeDelta = new Vector2(230f, 40f);
                }
            }

            if (winText != null)
            {
                winText.resizeTextForBestFit = true;
                winText.resizeTextMinSize = 16;
                winText.resizeTextMaxSize = 26;
                if (winText.rectTransform != null)
                {
                    winText.rectTransform.sizeDelta = new Vector2(230f, 40f);
                }
            }
        }

        private void OnEnable()
        {
            if (walletManager != null)
            {
                walletManager.OnBalanceChanged += HandleBalanceChanged;
                walletManager.OnBetChanged += HandleBetChanged;
                walletManager.OnWinAwarded += HandleWinAwarded;

                // Initial display
                UpdateAllDisplays();
            }

            if (slotMachineController != null)
            {
                slotMachineController.OnSpinStarted += HandleSpinStarted;
            }
        }

        private void OnDisable()
        {
            if (walletManager != null)
            {
                walletManager.OnBalanceChanged -= HandleBalanceChanged;
                walletManager.OnBetChanged -= HandleBetChanged;
                walletManager.OnWinAwarded -= HandleWinAwarded;
            }

            if (slotMachineController != null)
            {
                slotMachineController.OnSpinStarted -= HandleSpinStarted;
            }
        }

        public void Initialize(Text balTxt, Text bTxt, Text wTxt, WalletManager wallet, SlotMachineController controller)
        {
            balanceText = balTxt;
            betText = bTxt;
            winText = wTxt;
            walletManager = wallet;
            slotMachineController = controller;

            if (winText != null)
            {
                _originalWinScale = winText.rectTransform.localScale;
            }

            UpdateAllDisplays();
        }

        private void UpdateAllDisplays()
        {
            if (walletManager != null)
            {
                if (balanceText != null) balanceText.text = walletManager.GetFormattedBalance();
                if (betText != null) betText.text = walletManager.GetFormattedBet();
                if (winText != null) winText.text = walletManager.GetFormattedWin();
            }
        }

        private void HandleBalanceChanged(int newBalance, int delta)
        {
            if (balanceText != null)
            {
                balanceText.text = WalletManager.FormatRupees(newBalance);
            }
        }

        private void HandleBetChanged(int newBet)
        {
            if (betText != null)
            {
                betText.text = WalletManager.FormatRupees(newBet);
            }
        }

        private void HandleSpinStarted(SymbolData[] targets)
        {
            if (winText != null)
            {
                winText.text = WalletManager.FormatRupees(0);
                winText.color = new Color(0.2f, 1f, 0.4f); // Reset green
                winText.rectTransform.localScale = _originalWinScale;
            }
        }

        /// <summary>
        /// Displays the classic arcade near-miss tease alert ("SO CLOSE!") on heartbreaking near-misses.
        /// </summary>
        public void ShowNearMissAlert()
        {
            if (winText != null)
            {
                if (_countUpCoroutine != null) StopCoroutine(_countUpCoroutine);
                winText.text = "SO CLOSE!";
                winText.color = new Color(1f, 0.4f, 0.35f); // Reddish orange ragebait color
                winText.rectTransform.localScale = _originalWinScale * 1.25f;
            }
        }

        private void HandleWinAwarded(int winAmount)
        {
            if (winText != null)
            {
                if (_countUpCoroutine != null) StopCoroutine(_countUpCoroutine);
                _countUpCoroutine = StartCoroutine(AnimateWinCounter(winAmount));
            }
        }

        private IEnumerator AnimateWinCounter(int targetWin)
        {
            float duration = 0.8f;
            float timer = 0f;
            int startVal = 0;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float progress = Mathf.Clamp01(timer / duration);
                int currentVal = Mathf.RoundToInt(Mathf.Lerp(startVal, targetWin, progress));

                if (winText != null)
                {
                    winText.text = WalletManager.FormatRupees(currentVal);
                    float pulse = 1f + Mathf.Sin(progress * Mathf.PI) * 0.25f;
                    winText.rectTransform.localScale = _originalWinScale * pulse;
                }

                yield return null;
            }

            if (winText != null)
            {
                winText.text = WalletManager.FormatRupees(targetWin);
                winText.rectTransform.localScale = _originalWinScale;
            }

            _countUpCoroutine = null;
        }
    }
}
