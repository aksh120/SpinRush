using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SpinRush.Gameplay
{
    /// <summary>
    /// Interactive mechanical lever controller.
    /// Supports mouse click and drag to pull down the slot machine arm,
    /// triggering spin requests and springing back smoothly with elastic physics.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class LeverController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Visual Sprites")]
        [Tooltip("Sprite showing the lever in the upright / idle position.")]
        [SerializeField] private Sprite leverUpSprite;

        [Tooltip("Sprite showing the lever in the fully pulled / down position.")]
        [SerializeField] private Sprite leverDownSprite;

        [Header("References")]
        [SerializeField] private SlotMachineController slotMachineController;

        [Header("Animation")]
        [SerializeField] private float springReturnDuration = 0.25f;

        private Image _image;
        private RectTransform _rectTransform;
        private Vector2 _upPosition;
        private Vector2 _downPosition;
        private bool _isPulled = false;
        private Coroutine _springCoroutine;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _rectTransform = GetComponent<RectTransform>();
            _upPosition = _rectTransform.anchoredPosition;
            _downPosition = _upPosition + new Vector2(0f, -40f);

            if (slotMachineController == null)
            {
                slotMachineController = GetComponentInParent<SlotMachineController>();
            }
        }

        public void Initialize(Sprite upSprite, Sprite downSprite, SlotMachineController controller)
        {
            leverUpSprite = upSprite;
            leverDownSprite = downSprite;
            slotMachineController = controller;

            if (_image == null) _image = GetComponent<Image>();
            if (_image != null && leverUpSprite != null)
            {
                _image.sprite = leverUpSprite;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            PullLever();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ReleaseLever();
        }

        /// <summary>
        /// Pulls the lever arm down and triggers a spin request if idle.
        /// </summary>
        public void PullLever()
        {
            if (_isPulled) return;
            _isPulled = true;

            if (_springCoroutine != null) StopCoroutine(_springCoroutine);

            if (_image != null && leverDownSprite != null)
            {
                _image.sprite = leverDownSprite;
            }

            _rectTransform.anchoredPosition = _downPosition;

            // Trigger spin request
            if (slotMachineController != null)
            {
                slotMachineController.RequestSpin();
            }

            // Automatically spring back after short delay if held
            StartCoroutine(AutoReleaseRoutine());
        }

        private IEnumerator AutoReleaseRoutine()
        {
            yield return new WaitForSeconds(0.18f);
            ReleaseLever();
        }

        /// <summary>
        /// Releases the lever to spring smoothly back to the upright position.
        /// </summary>
        public void ReleaseLever()
        {
            if (!_isPulled) return;
            _isPulled = false;

            if (_springCoroutine != null) StopCoroutine(_springCoroutine);
            _springCoroutine = StartCoroutine(SpringBackRoutine());
        }

        private IEnumerator SpringBackRoutine()
        {
            float timer = 0f;
            Vector2 startPos = _rectTransform.anchoredPosition;

            while (timer < springReturnDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / springReturnDuration);

                // Quadratic ease-out bounce
                float ease = 1f - Mathf.Pow(1f - t, 2f);
                _rectTransform.anchoredPosition = Vector2.Lerp(startPos, _upPosition, ease);

                if (t > 0.5f && _image != null && leverUpSprite != null)
                {
                    _image.sprite = leverUpSprite;
                }

                yield return null;
            }

            _rectTransform.anchoredPosition = _upPosition;
            if (_image != null && leverUpSprite != null)
            {
                _image.sprite = leverUpSprite;
            }
            _springCoroutine = null;
        }
    }
}
