using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SpinRush.Core;
using SpinRush.Gameplay;

namespace SpinRush.UI
{
    /// <summary>
    /// Manages modal celebration dialogs (Big Win, Kohinoor Jackpot)
    /// and low-balance recovery popups with animated scaling and input blocking.
    /// </summary>
    public class WinPopupController : MonoBehaviour
    {
        [Header("UI Hierarchy References")]
        [SerializeField] private GameObject backdropObj;
        [SerializeField] private RectTransform modalContainer;
        [SerializeField] private Text titleText;
        [SerializeField] private Text messageText;
        [SerializeField] private Text amountText;
        [SerializeField] private Button yesClaimButton;
        [SerializeField] private Button noButton;

        [Header("Animation")]
        [SerializeField] private float popupDuration = 0.35f;

        private Action _onYesAction;
        private Action _onNoAction;
        private Coroutine _animateCoroutine;

        private void Awake()
        {
            if (yesClaimButton != null)
            {
                yesClaimButton.onClick.AddListener(OnYesClaimClicked);
            }

            if (noButton != null)
            {
                noButton.onClick.AddListener(OnNoClicked);
            }

            // Start closed
            HideImmediate();
        }

        public void Initialize(GameObject backdrop, RectTransform container, Text title, Text msg, Text amount, Button yesBtn, Button noBtn)
        {
            backdropObj = backdrop;
            modalContainer = container;
            titleText = title;
            messageText = msg;
            amountText = amount;
            yesClaimButton = yesBtn;
            noButton = noBtn;

            if (yesClaimButton != null)
            {
                yesClaimButton.onClick.RemoveAllListeners();
                yesClaimButton.onClick.AddListener(OnYesClaimClicked);
            }

            if (noButton != null)
            {
                noButton.onClick.RemoveAllListeners();
                noButton.onClick.AddListener(OnNoClicked);
            }

            HideImmediate();
        }

        /// <summary>
        /// Displays the Big Win or Jackpot celebration modal.
        /// </summary>
        public void ShowWinPopup(SpinResult result, Action onClaimed = null)
        {
            _onYesAction = onClaimed;
            _onNoAction = null;

            if (titleText != null)
            {
                titleText.text = result.WinTitle;
                titleText.color = result.IsJackpot ? new Color(1f, 0.85f, 0.2f) : new Color(1f, 0.95f, 0.7f);
            }

            if (messageText != null)
            {
                messageText.text = result.IsJackpot ?
                    "ROYAL DHAMAKA!\nYou won a colossal payout!" :
                    result.Description;
            }

            if (amountText != null)
            {
                amountText.text = result.FormattedPayout;
                amountText.gameObject.SetActive(true);
            }

            if (noButton != null)
            {
                noButton.gameObject.SetActive(false); // Only Claim button for win
            }

            if (yesClaimButton != null)
            {
                yesClaimButton.gameObject.SetActive(true);
                RectTransform yRect = yesClaimButton.GetComponent<RectTransform>();
                if (yRect != null) yRect.anchoredPosition = new Vector2(0f, -115f); // Centered!

                Text btnText = yesClaimButton.GetComponentInChildren<Text>();
                if (btnText != null) btnText.text = "COLLECT";
            }

            ShowModal();
        }

        /// <summary>
        /// Displays the low balance recovery dialog with YES/NO actions.
        /// </summary>
        public void ShowInsufficientFundsPopup(Action onResetConfirmed, Action onCancelled = null)
        {
            _onYesAction = onResetConfirmed;
            _onNoAction = onCancelled;

            if (titleText != null)
            {
                titleText.text = "PAISE KHATAM!";
                titleText.color = new Color(1f, 0.4f, 0.4f);
            }

            if (messageText != null)
            {
                messageText.text = $"Aapka balance kam ho gaya hai!\nReset credits to {WalletManager.FormatRupees(GameConstants.DefaultStartingBalance)}?";
            }

            if (amountText != null)
            {
                amountText.text = "";
                amountText.gameObject.SetActive(false);
            }

            if (yesClaimButton != null)
            {
                yesClaimButton.gameObject.SetActive(true);
                RectTransform yRect = yesClaimButton.GetComponent<RectTransform>();
                if (yRect != null) yRect.anchoredPosition = new Vector2(-110f, -115f);

                Text btnText = yesClaimButton.GetComponentInChildren<Text>();
                if (btnText != null) btnText.text = "YES";
            }

            if (noButton != null)
            {
                noButton.gameObject.SetActive(true);
                RectTransform nRect = noButton.GetComponent<RectTransform>();
                if (nRect != null) nRect.anchoredPosition = new Vector2(110f, -115f);
            }

            ShowModal();
        }

        private void ShowModal()
        {
            gameObject.SetActive(true);
            if (backdropObj != null) backdropObj.SetActive(true);

            if (_animateCoroutine != null) StopCoroutine(_animateCoroutine);
            _animateCoroutine = StartCoroutine(AnimateOpen());
        }

        private IEnumerator AnimateOpen()
        {
            if (modalContainer == null) yield break;

            modalContainer.localScale = Vector3.zero;
            float timer = 0f;

            while (timer < popupDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / popupDuration);

                // Overshoot bounce ease-out
                float scale = 1f + 0.15f * Mathf.Sin(t * Mathf.PI);
                if (t >= 1f) scale = 1f;

                modalContainer.localScale = Vector3.one * Mathf.Lerp(0f, scale, t);
                yield return null;
            }

            modalContainer.localScale = Vector3.one;
            _animateCoroutine = null;
        }

        private void OnYesClaimClicked()
        {
            Action callback = _onYesAction;
            ClosePopup(() => callback?.Invoke());
        }

        private void OnNoClicked()
        {
            Action callback = _onNoAction;
            ClosePopup(() => callback?.Invoke());
        }

        public void ClosePopup(Action onClosed = null)
        {
            if (_animateCoroutine != null) StopCoroutine(_animateCoroutine);
            _animateCoroutine = StartCoroutine(AnimateClose(onClosed));
        }

        private IEnumerator AnimateClose(Action onClosed)
        {
            if (modalContainer != null)
            {
                float timer = 0f;
                float duration = 0.2f;

                while (timer < duration)
                {
                    timer += Time.deltaTime;
                    float t = Mathf.Clamp01(timer / duration);
                    modalContainer.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t * t);
                    yield return null;
                }
            }

            HideImmediate();
            _animateCoroutine = null;
            onClosed?.Invoke();
        }

        public void HideImmediate()
        {
            if (modalContainer != null) modalContainer.localScale = Vector3.zero;
            if (backdropObj != null) backdropObj.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}
