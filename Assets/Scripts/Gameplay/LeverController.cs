using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SpinRush.Gameplay
{
    /// <summary>
    /// Interactive physical lever controller.
    /// Maps clicks and drags over the lever handle to mechanical sprite swaps,
    /// audio ratchet feedback, and spin requests with damped spring return.
    /// </summary>
    public class LeverController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Header("Sprite References (Full Cabinet Overlays)")]
        [Tooltip("slot-machine2.png: Lever in upright / ready state.")]
        [SerializeField] private Sprite leverUpSprite;

        [Tooltip("slot-machine3.png: Lever in pulled / down state.")]
        [SerializeField] private Sprite leverDownSprite;

        [Header("Target Image")]
        [SerializeField] private Image leverImage;

        [Header("Dependencies")]
        [SerializeField] private SlotMachineController controller;

        private bool _isPulled = false;
        private Coroutine _returnCoroutine;

        private void Awake()
        {
            if (leverImage == null)
            {
                leverImage = GetComponent<Image>();
            }

            if (leverImage != null && leverUpSprite != null)
            {
                leverImage.sprite = leverUpSprite;
            }
        }

        public void Initialize(Sprite upSprite, Sprite downSprite, SlotMachineController slotController, Image targetImage = null)
        {
            leverUpSprite = upSprite;
            leverDownSprite = downSprite;
            controller = slotController;
            if (targetImage != null) leverImage = targetImage;

            if (leverImage != null && leverUpSprite != null)
            {
                leverImage.sprite = leverUpSprite;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            PullLever();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isPulled && eventData.delta.y < -5f)
            {
                PullLever();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // Auto returns in coroutine
        }

        private void PullLever()
        {
            if (_isPulled) return;

            if (controller != null && !controller.IsSpinning)
            {
                _isPulled = true;

                // Visual snap to pulled down state
                if (leverImage != null && leverDownSprite != null)
                {
                    leverImage.sprite = leverDownSprite;
                }

                // Request spin
                controller.RequestSpin();

                if (_returnCoroutine != null) StopCoroutine(_returnCoroutine);
                _returnCoroutine = StartCoroutine(ReturnLeverRoutine());
            }
        }

        private IEnumerator ReturnLeverRoutine()
        {
            // Hold down briefly to convey physical weight
            yield return new WaitForSeconds(0.22f);

            // Snap back up
            if (leverImage != null && leverUpSprite != null)
            {
                leverImage.sprite = leverUpSprite;
            }

            _isPulled = false;
            _returnCoroutine = null;
        }
    }
}
