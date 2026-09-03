using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SpinRush.Gameplay
{
    /// <summary>
    /// Visual component attached to an individual symbol slot on a reel.
    /// Manages the visual presentation, sprite assignment, and win highlight animation.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SlotSymbol : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Image component used to display the symbol icon.")]
        [SerializeField] private Image iconImage;

        [Tooltip("Optional glow / outline image shown during win highlights.")]
        [SerializeField] private Image highlightImage;

        [Tooltip("Optional tile background image.")]
        [SerializeField] private Image bgImage;

        [Header("State")]
        [SerializeField] private SymbolData currentSymbol;

        private RectTransform _rectTransform;
        private Vector3 _originalScale = Vector3.one;
        private Coroutine _highlightCoroutine;

        public SymbolData CurrentSymbol => currentSymbol;
        public RectTransform RectTransform => _rectTransform != null ? _rectTransform : (_rectTransform = GetComponent<RectTransform>());

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _originalScale = _rectTransform.localScale;

            if (iconImage == null)
            {
                iconImage = GetComponent<Image>();
            }

            if (highlightImage != null)
            {
                highlightImage.gameObject.SetActive(false);
            }
        }

        public void InitializeReferences(Image icon, Image highlight = null, Image bg = null)
        {
            iconImage = icon;
            highlightImage = highlight;
            bgImage = bg;
            _rectTransform = GetComponent<RectTransform>();
            _originalScale = _rectTransform.localScale;
        }

        /// <summary>
        /// Updates the visual icon and data payload for this symbol slot.
        /// </summary>
        public void SetSymbol(SymbolData symbolData)
        {
            currentSymbol = symbolData;

            if (iconImage == null)
            {
                iconImage = GetComponent<Image>();
            }

            if (iconImage != null)
            {
                if (symbolData != null && symbolData.Icon != null)
                {
                    iconImage.sprite = symbolData.Icon;
                    iconImage.color = Color.white;
                    iconImage.enabled = true;
                }
                else
                {
                    iconImage.enabled = false;
                }
            }
        }

        /// <summary>
        /// Enables or disables the win highlight animation for this symbol.
        /// </summary>
        public void SetHighlight(bool active)
        {
            if (highlightImage != null)
            {
                highlightImage.gameObject.SetActive(active);
            }

            if (active)
            {
                if (_highlightCoroutine != null) StopCoroutine(_highlightCoroutine);
                _highlightCoroutine = StartCoroutine(AnimateHighlight());
            }
            else
            {
                if (_highlightCoroutine != null)
                {
                    StopCoroutine(_highlightCoroutine);
                    _highlightCoroutine = null;
                }
                RectTransform.localScale = _originalScale;
                if (iconImage != null) iconImage.color = Color.white;
            }
        }

        private IEnumerator AnimateHighlight()
        {
            float timer = 0f;
            while (true)
            {
                timer += Time.deltaTime * 6f;
                float scaleMod = 1f + Mathf.Sin(timer) * 0.18f;
                RectTransform.localScale = _originalScale * scaleMod;

                if (iconImage != null)
                {
                    // Flash between pure gold and white
                    float flash = (Mathf.Sin(timer * 2f) + 1f) * 0.5f;
                    iconImage.color = Color.Lerp(Color.white, new Color(1f, 0.92f, 0.4f), flash);
                }

                yield return null;
            }
        }
    }
}
