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
        [SerializeField] private Button gambleButton;

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
                titleText.color = (result.IsJackpot || result.IsRoyalJackpot) ? new Color(1f, 0.85f, 0.2f) : new Color(1f, 0.95f, 0.7f);
            }

            if (messageText != null)
            {
                if (!string.IsNullOrEmpty(result.RarityBadge))
                {
                    messageText.text = $"<color=#00FFFF>★ {result.RarityBadge} ★</color>\n{result.Description}";
                }
                else
                {
                    messageText.text = (result.IsJackpot || result.IsRoyalJackpot) ?
                        "ROYAL DHAMAKA!\nYou won a colossal payout!" :
                        result.Description;
                }
            }

            if (amountText != null)
            {
                amountText.text = result.FormattedPayout;
                amountText.gameObject.SetActive(true);

                if (_amountAnimCoroutine != null) StopCoroutine(_amountAnimCoroutine);
                if (result.IsJackpot || result.IsRoyalJackpot)
                {
                    _amountAnimCoroutine = StartCoroutine(AnimateAmountShimmer());
                }
                else
                {
                    amountText.rectTransform.localScale = Vector3.one;
                    amountText.color = new Color(0.2f, 1f, 0.4f);
                }
            }

            if (noButton != null)
            {
                noButton.gameObject.SetActive(false); // Only Claim button for win
            }

            if (yesClaimButton != null)
            {
                yesClaimButton.gameObject.SetActive(true);
                RectTransform yRect = yesClaimButton.GetComponent<RectTransform>();
                if (yRect != null)
                {
                    yRect.anchoredPosition = new Vector2(-95f, -115f);
                    yRect.sizeDelta = new Vector2(170f, 48f);
                }

                Image img = yesClaimButton.GetComponent<Image>();
                Sprite cleanGold = null;
#if UNITY_EDITOR
                cleanGold = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/btn_gold_clean.png");
#endif
                if (cleanGold != null && img != null)
                {
                    img.sprite = cleanGold;
                    img.color = Color.white;
                }
                else if (img != null)
                {
                    img.color = new Color(0.95f, 0.72f, 0.15f, 1f);
                }

                Button btnComp = yesClaimButton.GetComponent<Button>();
                if (btnComp != null)
                {
                    btnComp.transition = Selectable.Transition.ColorTint;
                }

                Text btnText = yesClaimButton.GetComponentInChildren<Text>();
                if (btnText != null)
                {
                    btnText.text = "COLLECT";
                    btnText.color = new Color(0.12f, 0.08f, 0.02f);
                    btnText.fontSize = 20;
                    btnText.fontStyle = FontStyle.Bold;
                    Shadow bShadow = btnText.GetComponent<Shadow>();
                    if (bShadow == null) bShadow = btnText.gameObject.AddComponent<Shadow>();
                    bShadow.effectColor = new Color(1f, 0.90f, 0.45f, 0.6f);
                    bShadow.effectDistance = new Vector2(1f, -1f);
                }
            }

            // Procedural Gamble 2X Button
            if (gambleButton == null && modalContainer != null)
            {
                Transform gb = modalContainer.Find("Btn_Gamble");
                if (gb != null) gambleButton = gb.GetComponent<Button>();
                else
                {
                    GameObject gObj = new GameObject("Btn_Gamble", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline), typeof(Shadow));
                    gObj.transform.SetParent(modalContainer, false);
                    RectTransform gr = gObj.GetComponent<RectTransform>();
                    gr.anchoredPosition = new Vector2(95f, -115f);
                    gr.sizeDelta = new Vector2(170f, 48f);
                    Image gImg = gObj.GetComponent<Image>();
                    gImg.color = new Color(0.85f, 0.12f, 0.25f); // Crimson Red
                    Outline gOut = gObj.GetComponent<Outline>();
                    gOut.effectColor = new Color(1f, 0.85f, 0.2f); // Gold neon
                    gOut.effectDistance = new Vector2(1.5f, -1.5f);

                    Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
                    GameObject gtObj = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(Shadow));
                    gtObj.transform.SetParent(gObj.transform, false);
                    RectTransform gtr = gtObj.GetComponent<RectTransform>();
                    gtr.anchorMin = Vector2.zero; gtr.anchorMax = Vector2.one; gtr.sizeDelta = Vector2.zero;
                    Text gt = gtObj.GetComponent<Text>();
                    gt.font = font; gt.text = "DOUBLE (2X)";
                    gt.fontSize = 17; gt.fontStyle = FontStyle.Bold;
                    gt.alignment = TextAnchor.MiddleCenter;
                    gt.color = Color.white;

                    gambleButton = gObj.GetComponent<Button>();
                }
            }

            if (gambleButton != null)
            {
                gambleButton.gameObject.SetActive(true);
                gambleButton.onClick.RemoveAllListeners();
                int winPot = result.Payout;
                gambleButton.onClick.AddListener(() =>
                {
                    ClosePopup(() =>
                    {
                        GambleMiniGameController gamble = FindObjectOfType<GambleMiniGameController>() ?? FindObjectOfType<Canvas>()?.gameObject.AddComponent<GambleMiniGameController>();
                        gamble?.StartGamble(winPot, (finalWon) =>
                        {
                            if (finalWon > winPot)
                            {
                                int extra = finalWon - winPot;
                                FindObjectOfType<WalletManager>()?.AwardPayout(extra);
                            }
                            else if (finalWon == 0)
                            {
                                FindObjectOfType<WalletManager>()?.AwardPayout(-winPot);
                            }
                        });
                    });
                });
            }

            ShowModal();
        }

        /// <summary>
        /// Displays the low balance recovery dialog with YES/NO actions.
        /// </summary>
        public void ShowInsufficientFundsPopup(Action onResetConfirmed, Action onCancelled = null)
        {
            if (gambleButton != null) gambleButton.gameObject.SetActive(false);

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
            if (_amountAnimCoroutine != null)
            {
                StopCoroutine(_amountAnimCoroutine);
                _amountAnimCoroutine = null;
            }

            if (amountText != null)
            {
                amountText.rectTransform.localScale = Vector3.one;
            }

            if (modalContainer != null) modalContainer.localScale = Vector3.zero;
            if (backdropObj != null) backdropObj.SetActive(false);
            gameObject.SetActive(false);
        }

        private Coroutine _amountAnimCoroutine;

        private IEnumerator AnimateAmountShimmer()
        {
            if (amountText == null) yield break;
            Vector3 baseScale = Vector3.one;
            float timer = 0f;

            while (true)
            {
                timer += Time.deltaTime * 5f;
                float pulse = 1f + Mathf.Sin(timer) * 0.12f;
                amountText.rectTransform.localScale = baseScale * pulse;

                // Alternate between bright neon green and vibrant radiant gold
                float t = (Mathf.Sin(timer * 1.5f) + 1f) * 0.5f;
                amountText.color = Color.Lerp(new Color(0.2f, 1f, 0.4f), new Color(1f, 0.90f, 0.25f), t);

                yield return null;
            }
        }
    }
}
