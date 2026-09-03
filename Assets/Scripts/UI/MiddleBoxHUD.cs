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

        private Coroutine _balanceAnimCoroutine;
        private Coroutine _deltaAnimCoroutine;
        private Vector3 _originalBalanceScale = Vector3.one;
        private Color _originalBalanceColor = Color.white;
        private int _displayedBalance = 0;
        private Text _floatingDeltaText;

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

            if (balanceText != null)
            {
                _originalBalanceScale = balanceText.rectTransform.localScale;
                _originalBalanceColor = balanceText.color;
            }

            if (walletManager != null)
            {
                _displayedBalance = walletManager.Balance;
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

        private void EnsureFloatingDeltaText()
        {
            if (_floatingDeltaText != null) return;
            if (balanceText == null) return;

            GameObject obj = new GameObject("FloatingDeltaText", typeof(RectTransform), typeof(Text), typeof(Outline), typeof(Shadow));
            obj.transform.SetParent(balanceText.transform.parent, false);
            RectTransform r = obj.GetComponent<RectTransform>();
            r.anchoredPosition = balanceText.rectTransform.anchoredPosition + new Vector2(0f, 32f);
            r.sizeDelta = new Vector2(200f, 36f);

            _floatingDeltaText = obj.GetComponent<Text>();
            _floatingDeltaText.font = balanceText.font;
            _floatingDeltaText.fontSize = 18;
            _floatingDeltaText.fontStyle = FontStyle.Bold;
            _floatingDeltaText.alignment = TextAnchor.MiddleCenter;
            _floatingDeltaText.raycastTarget = false;

            Outline o = obj.GetComponent<Outline>();
            o.effectColor = new Color(0f, 0f, 0f, 0.9f);
            o.effectDistance = new Vector2(1.5f, -1.5f);

            Shadow s = obj.GetComponent<Shadow>();
            s.effectColor = new Color(0f, 0f, 0f, 0.7f);
            s.effectDistance = new Vector2(2f, -2f);

            _floatingDeltaText.gameObject.SetActive(false);
        }

        private void HandleBalanceChanged(int newBalance, int delta)
        {
            if (balanceText == null) return;

            // If initial start broadcast or no delta, simply sync immediately
            if (delta == 0 || _displayedBalance == 0)
            {
                _displayedBalance = newBalance;
                balanceText.text = WalletManager.FormatRupees(newBalance);
                return;
            }

            EnsureFloatingDeltaText();

            // 1. Animated floating delta (-₹250 in red or +₹5,000 in green)
            if (_deltaAnimCoroutine != null) StopCoroutine(_deltaAnimCoroutine);
            _deltaAnimCoroutine = StartCoroutine(AnimateFloatingDelta(delta));

            // 2. Rolling number counter with pop-in / pop-out bounce
            if (_balanceAnimCoroutine != null) StopCoroutine(_balanceAnimCoroutine);
            _balanceAnimCoroutine = StartCoroutine(AnimateBalanceCounter(_displayedBalance, newBalance, delta));
        }

        private IEnumerator AnimateFloatingDelta(int delta)
        {
            if (_floatingDeltaText == null || balanceText == null) yield break;

            bool isPositive = delta > 0;
            string sign = isPositive ? "+" : "";
            _floatingDeltaText.text = $"{sign}{WalletManager.FormatRupees(delta)}";

            Color baseColor = isPositive ? new Color(0.15f, 1f, 0.45f, 1f) : new Color(1f, 0.25f, 0.25f, 1f);
            _floatingDeltaText.color = baseColor;
            _floatingDeltaText.gameObject.SetActive(true);

            RectTransform r = _floatingDeltaText.rectTransform;
            Vector2 startPos = balanceText.rectTransform.anchoredPosition + new Vector2(0f, 26f);
            Vector2 targetPos = startPos + new Vector2(0f, 34f);

            float duration = 0.75f;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float progress = Mathf.Clamp01(timer / duration);

                // Upward float
                r.anchoredPosition = Vector2.Lerp(startPos, targetPos, progress);

                // Subtle scale pulse
                float scale = 1f + Mathf.Sin(progress * Mathf.PI) * 0.25f;
                r.localScale = new Vector3(scale, scale, 1f);

                // Fade out in second half
                float alpha = (progress > 0.4f) ? Mathf.Lerp(1f, 0f, (progress - 0.4f) / 0.6f) : 1f;
                _floatingDeltaText.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);

                yield return null;
            }

            _floatingDeltaText.gameObject.SetActive(false);
            _deltaAnimCoroutine = null;
        }

        private IEnumerator AnimateBalanceCounter(int fromBalance, int toBalance, int delta)
        {
            if (balanceText == null) yield break;

            bool isPositive = delta > 0;
            float duration = isPositive ? 0.70f : 0.38f;
            float timer = 0f;

            Color flashColor = isPositive ? new Color(0.3f, 1f, 0.5f) : new Color(1f, 0.35f, 0.35f);

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float progress = Mathf.Clamp01(timer / duration);
                float ease = 1f - Mathf.Pow(1f - progress, 3f);

                int currentVal = Mathf.RoundToInt(Mathf.Lerp(fromBalance, toBalance, ease));
                balanceText.text = WalletManager.FormatRupees(currentVal);

                // Dynamic color flash during roll
                balanceText.color = Color.Lerp(flashColor, _originalBalanceColor, progress * 0.8f);

                yield return null;
            }

            // Lock to final value
            _displayedBalance = toBalance;
            balanceText.text = WalletManager.FormatRupees(toBalance);

            // Pop-in / pop-out punch scale bounce
            float bounceTime = 0.22f;
            float bTimer = 0f;
            while (bTimer < bounceTime)
            {
                bTimer += Time.deltaTime;
                float bp = Mathf.Clamp01(bTimer / bounceTime);
                float s = 1f + Mathf.Sin(bp * Mathf.PI) * 0.30f;
                balanceText.rectTransform.localScale = _originalBalanceScale * s;
                yield return null;
            }

            balanceText.rectTransform.localScale = _originalBalanceScale;
            balanceText.color = _originalBalanceColor;
            _balanceAnimCoroutine = null;
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
