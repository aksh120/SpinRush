using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SpinRush.Gameplay;

namespace SpinRush.UI
{
    /// <summary>
    /// Ensures 100% responsive, zero-latency trigger for the Spin button on mouse, touch, and WebGL pointer events.
    /// Also provides an attractive pulsing idle animation.
    /// </summary>
    public class SpinButtonTrigger : MonoBehaviour, IPointerDownHandler, IPointerClickHandler
    {
        [SerializeField] private SlotMachineController controller;
        [SerializeField] private RectTransform rectTransform;

        private Vector3 _baseScale = Vector3.one;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            _baseScale = rectTransform != null ? rectTransform.localScale : Vector3.one;

            if (controller == null)
            {
                controller = FindObjectOfType<SlotMachineController>();
            }
        }

        public void Initialize(SlotMachineController machineController)
        {
            controller = machineController;
            rectTransform = GetComponent<RectTransform>();
            _baseScale = rectTransform != null ? rectTransform.localScale : Vector3.one;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // Visual press feedback
            if (rectTransform != null) rectTransform.localScale = _baseScale * 0.92f;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (rectTransform != null) rectTransform.localScale = _baseScale;

            if (controller != null)
            {
                controller.OnSpinButtonClicked();
            }
        }

        private void Update()
        {
            // Gentle inviting pulse when idle
            if (controller != null && !controller.IsSpinning && rectTransform != null)
            {
                float pulse = 1f + Mathf.Sin(Time.time * 4f) * 0.04f;
                rectTransform.localScale = _baseScale * pulse;
            }
        }
    }
}
