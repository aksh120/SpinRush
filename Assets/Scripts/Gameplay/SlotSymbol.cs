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

            if (iconImage != null && symbolData != null && symbolData.Icon != null)
            {
                iconImage.sprite = symbolData.Icon;
                iconImage.enabled = true;
            }
            else if (iconImage != null)
            {
                iconImage.enabled = false;
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
            }
        }

        private System.Collections.IEnumerator AnimateHighlight()
        {
            float timer = 0f;
            while (true)
            {
                timer += Time.deltaTime * 5f;
                float scaleMod = 1f + Mathf.Sin(timer) * 0.12f;
                RectTransform.localScale = _originalScale * scaleMod;
                yield return null;
            }
        }
    }
}
