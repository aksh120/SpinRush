using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SpinRush.Audio;
using SpinRush.Core;
using SpinRush.Effects;

namespace SpinRush.UI
{
    /// <summary>
    /// Double-or-Nothing Gamble Mini-Game.
    /// Allows the player to risk any win amount in a 50/50 Red vs Black card flip to double their payout!
    /// </summary>
    public class GambleMiniGameController : MonoBehaviour
    {
        [Header("State")]
        private int _currentGamblePot = 0;
        private Action<int> _onGambleFinished;
        private bool _isFlipping = false;

        [Header("UI Objects")]
        private GameObject _modalRoot;
        private Text _potText;
        private Text _resultText;
        private Image _cardFace;
        private Button _redButton;
        private Button _blackButton;
        private Button _collectButton;

        [Header("Controllers")]
        [SerializeField] private AudioController audioController;
        [SerializeField] private WinEffectsPresenter effectsPresenter;

        private void Awake()
        {
            if (audioController == null) audioController = FindObjectOfType<AudioController>();
            if (effectsPresenter == null) effectsPresenter = FindObjectOfType<WinEffectsPresenter>();
            BuildGambleModalUI();
        }

        public void StartGamble(int initialWin, Action<int> onFinished)
        {
            _currentGamblePot = initialWin;
            _onGambleFinished = onFinished;
            _isFlipping = false;

            if (_modalRoot != null)
            {
                _modalRoot.SetActive(true);
                UpdateDisplay();
            }
        }

        private void UpdateDisplay()
        {
            if (_potText != null)
            {
                _potText.text = $"CURRENT STASH: {WalletManager.FormatRupees(_currentGamblePot)}\n<color=#FFD700>DOUBLE TARGET: {WalletManager.FormatRupees(_currentGamblePot * 2)}</color>";
            }

            if (_resultText != null)
            {
                _resultText.text = "CHOOSE RED OR BLACK TO DOUBLE!";
                _resultText.color = Color.white;
            }

            if (_cardFace != null)
            {
                _cardFace.color = new Color(0.2f, 0.15f, 0.4f);
            }
        }

        private void OnChoiceClicked(bool choseRed)
        {
            if (_isFlipping) return;
            StartCoroutine(ExecuteFlipRoutine(choseRed));
        }

        private IEnumerator ExecuteFlipRoutine(bool choseRed)
        {
            _isFlipping = true;

            if (audioController != null) audioController.PlayButtonClick();

            // Rapid card flip animation
            float elapsed = 0f;
            float duration = 0.8f;
            bool outcomeIsRed = UnityEngine.Random.value < 0.5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float scaleX = Mathf.Cos(t * Mathf.PI * 4f);
                if (_cardFace != null)
                {
                    _cardFace.rectTransform.localScale = new Vector3(Mathf.Abs(scaleX), 1f, 1f);
                    _cardFace.color = (scaleX > 0) ? new Color(0.9f, 0.2f, 0.2f) : new Color(0.15f, 0.15f, 0.2f);
                }
                yield return null;
            }

            if (_cardFace != null)
            {
                _cardFace.rectTransform.localScale = Vector3.one;
                _cardFace.color = outcomeIsRed ? new Color(0.9f, 0.15f, 0.2f) : new Color(0.12f, 0.12f, 0.15f);
            }

            bool won = (choseRed == outcomeIsRed);

            if (won)
            {
                _currentGamblePot *= 2;
                if (audioController != null) audioController.PlayWinCelebration(25f, false);
                if (effectsPresenter != null) effectsPresenter.PlayParticleBurst(40);

                if (_resultText != null)
                {
                    _resultText.text = $"★ DOUBLED! YOU WON {WalletManager.FormatRupees(_currentGamblePot)}! ★";
                    _resultText.color = new Color(0.2f, 1f, 0.4f);
                }

                if (_potText != null)
                {
                    _potText.text = $"CURRENT STASH: {WalletManager.FormatRupees(_currentGamblePot)}\n<color=#FFD700>DOUBLE TARGET: {WalletManager.FormatRupees(_currentGamblePot * 2)}</color>";
                }
            }
            else
            {
                _currentGamblePot = 0;
                if (audioController != null) audioController.PlayNearMissSigh();

                if (_resultText != null)
                {
                    _resultText.text = "HOUSE WINS! BETTER LUCK NEXT TIME!";
                    _resultText.color = new Color(1f, 0.35f, 0.35f);
                }

                yield return new WaitForSeconds(1.2f);
                CloseModal();
                yield break;
            }

            _isFlipping = false;
        }

        private void OnCollectClicked()
        {
            if (_isFlipping) return;
            CloseModal();
        }

        private void CloseModal()
        {
            if (_modalRoot != null) _modalRoot.SetActive(false);
            Action<int> cb = _onGambleFinished;
            _onGambleFinished = null;
            cb?.Invoke(_currentGamblePot);
        }

        private void BuildGambleModalUI()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            Transform existing = canvas.transform.Find("GambleModalRoot");
            if (existing != null) DestroyImmediate(existing.gameObject);

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // Modal Root (Backdrop)
            _modalRoot = new GameObject("GambleModalRoot", typeof(RectTransform), typeof(Image));
            _modalRoot.transform.SetParent(canvas.transform, false);
            RectTransform rootRect = _modalRoot.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.sizeDelta = Vector2.zero;
            _modalRoot.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);

            // Dialog Card
            GameObject card = new GameObject("DialogCard", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Shadow));
            card.transform.SetParent(_modalRoot.transform, false);
            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(460f, 360f);
            card.GetComponent<Image>().color = new Color(0.06f, 0.03f, 0.16f, 0.98f);

            Outline cOutline = card.GetComponent<Outline>();
            cOutline.effectColor = new Color(0.96f, 0.78f, 0.22f, 0.95f);
            cOutline.effectDistance = new Vector2(2.5f, -2.5f);

            // Header
            GameObject hObj = new GameObject("Header", typeof(RectTransform), typeof(Text), typeof(Shadow));
            hObj.transform.SetParent(card.transform, false);
            RectTransform hRect = hObj.GetComponent<RectTransform>();
            hRect.anchoredPosition = new Vector2(0f, 135f);
            hRect.sizeDelta = new Vector2(400f, 40f);
            Text hTxt = hObj.GetComponent<Text>();
            hTxt.font = font;
            hTxt.text = "★ DOUBLE OR NOTHING ★";
            hTxt.fontSize = 20;
            hTxt.fontStyle = FontStyle.Bold;
            hTxt.alignment = TextAnchor.MiddleCenter;
            hTxt.color = new Color(1f, 0.88f, 0.35f);

            // Pot Text
            GameObject potObj = new GameObject("PotText", typeof(RectTransform), typeof(Text));
            potObj.transform.SetParent(card.transform, false);
            RectTransform pRect = potObj.GetComponent<RectTransform>();
            pRect.anchoredPosition = new Vector2(0f, 85f);
            pRect.sizeDelta = new Vector2(400f, 50f);
            _potText = potObj.GetComponent<Text>();
            _potText.font = font;
            _potText.fontSize = 14;
            _potText.fontStyle = FontStyle.Bold;
            _potText.alignment = TextAnchor.MiddleCenter;
            _potText.color = Color.white;

            // Card Face Graphic
            GameObject cfObj = new GameObject("CardFace", typeof(RectTransform), typeof(Image), typeof(Outline));
            cfObj.transform.SetParent(card.transform, false);
            RectTransform cfRect = cfObj.GetComponent<RectTransform>();
            cfRect.anchoredPosition = new Vector2(0f, 15f);
            cfRect.sizeDelta = new Vector2(80f, 75f);
            _cardFace = cfObj.GetComponent<Image>();
            _cardFace.color = new Color(0.2f, 0.15f, 0.4f);
            Outline cfOutline = cfObj.GetComponent<Outline>();
            cfOutline.effectColor = Color.white;
            cfOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // Result Text
            GameObject resObj = new GameObject("ResultText", typeof(RectTransform), typeof(Text));
            resObj.transform.SetParent(card.transform, false);
            RectTransform rRect = resObj.GetComponent<RectTransform>();
            rRect.anchoredPosition = new Vector2(0f, -40f);
            rRect.sizeDelta = new Vector2(420f, 30f);
            _resultText = resObj.GetComponent<Text>();
            _resultText.font = font;
            _resultText.fontSize = 13;
            _resultText.fontStyle = FontStyle.Bold;
            _resultText.alignment = TextAnchor.MiddleCenter;
            _resultText.color = Color.white;

            // Buttons: RED vs BLACK
            _redButton = CreateButton(card.transform, "Btn_Red", new Vector2(-110f, -95f), new Vector2(130f, 42f), new Color(0.85f, 0.15f, 0.2f), "RED ♥", font);
            _redButton.onClick.AddListener(() => OnChoiceClicked(true));

            _blackButton = CreateButton(card.transform, "Btn_Black", new Vector2(110f, -95f), new Vector2(130f, 42f), new Color(0.15f, 0.15f, 0.25f), "BLACK ♠", font);
            _blackButton.onClick.AddListener(() => OnChoiceClicked(false));

            // Collect Button
            _collectButton = CreateButton(card.transform, "Btn_Collect", new Vector2(0f, -145f), new Vector2(220f, 36f), new Color(0f, 0.65f, 0.4f), "COLLECT CASH", font);
            _collectButton.onClick.AddListener(OnCollectClicked);

            _modalRoot.SetActive(false);
        }

        private Button CreateButton(Transform parent, string name, Vector2 pos, Vector2 size, Color col, string label, Font font)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            obj.transform.SetParent(parent, false);
            RectTransform r = obj.GetComponent<RectTransform>();
            r.anchoredPosition = pos;
            r.sizeDelta = size;
            obj.GetComponent<Image>().color = col;

            Outline o = obj.GetComponent<Outline>();
            o.effectColor = Color.white;
            o.effectDistance = new Vector2(1.2f, -1.2f);

            GameObject tObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            tObj.transform.SetParent(obj.transform, false);
            RectTransform tr = tObj.GetComponent<RectTransform>();
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.sizeDelta = Vector2.zero;
            Text t = tObj.GetComponent<Text>();
            t.font = font;
            t.text = label;
            t.fontSize = 13;
            t.fontStyle = FontStyle.Bold;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;

            return obj.GetComponent<Button>();
        }
    }
}
